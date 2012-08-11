/*
Copyright (C) 2012 Convolved Software

Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using log4net;

namespace Convolved.Hosting
{
    /// <summary>
    /// Encapsulates a service host which is automatically restarted if the service faults, up to
    /// a specified number of retries.
    /// </summary>
    /// <remarks>The retry counter will be reset if the host is started successfully.</remarks>
    public class ServiceHostContainer : IDisposable
    {
        private readonly ILog log = LogManager.GetLogger(typeof(ServiceHostContainer));
        private readonly IServiceHostFactory factory;
        private readonly IList<TimeSpan> retryTimes;

        private ServiceHost runningHost;
        private int retryCount;
        private bool isDisposing;
        private bool isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceHostContainer"/> using the
        /// specified factory and retry settings.
        /// </summary>
        /// <param name="factory">The factory responsible for creating new service hosts.</param>
        /// <param name="maxRetries">The maximum number of retries, after which the service will
        /// no longer attempt to restart.</param>
        /// <param name="initialRetryTime">The amount of time, in milliseconds, to wait before
        /// first attempting to restart the service.</param>
        /// <param name="retryTimeMultiplier">The amount by which to multiply the previous retry
        /// time to get the next retry time; a value greater than 1 indicates geometrically
        /// increasing time between successive attempts to restart the service.</param>
        /// <param name="maxRetryTime">The maximum time the container should wait before scheduling
        /// another restart attempt. This overrides the <paramref name="retryTimeMultiplier"/> if
        /// the new retry time would exceed the max time.</param>
        public ServiceHostContainer(IServiceHostFactory factory, uint maxRetries = 10,
            uint initialRetryTime = 1000, double retryTimeMultiplier = 5,
            uint maxRetryTime = 3600000)
        {
            if (factory == null)
                throw new ArgumentNullException("factory");
            if (retryTimeMultiplier < 1)
                throw new ArgumentOutOfRangeException("retryTimeMultiplier",
                    "Parameter 'retryTimeMultiplier' must be greater than 1.");
            if (maxRetryTime < initialRetryTime)
                throw new ArgumentException("Parameter 'maxRetryTime' cannot be less than the " +
                    "initial retry time. For unlimited time, use UInt32.MaxValue.", "maxRetryTime");
            this.factory = factory;
            this.retryTimes = CalculateRetryTimes(TimeSpan.FromMilliseconds(initialRetryTime),
                retryTimeMultiplier, maxRetries, TimeSpan.FromMilliseconds(maxRetryTime))
                .ToList();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceHostContainer"/> using the
        /// specified factory and retry settings.
        /// </summary>
        /// <param name="factory">The factory responsible for creating new service hosts.</param>
        /// <param name="retryTimes">A sequence of <see cref="TimeSpan"/> instances representing
        /// the specific times to wait between successive attempts to restart the service.</param>
        public ServiceHostContainer(IServiceHostFactory factory, IEnumerable<TimeSpan> retryTimes)
        {
            if (factory == null)
                throw new ArgumentNullException("factory");
            if ((retryTimes != null) && retryTimes.Any(t => t < TimeSpan.Zero))
                throw new ArgumentException("Parameter 'retryTimes' cannot contain any negative " +
                    "time intervals.");
            this.factory = factory;
            this.retryTimes = (retryTimes ?? Enumerable.Empty<TimeSpan>()).ToList();
        }

        /// <summary>
        /// Starts up a service host if there is not one already running.
        /// </summary>
        public void Run()
        {
            if (runningHost == null)
                StartHost();
        }

        /// <summary>
        /// Shuts down the service host.
        /// </summary>
        public void Dispose()
        {
            if (isDisposed)
                return;
            isDisposing = true;
            if (runningHost != null)
            {
                log.DebugFormat("Cleaning up service host, service name = {0}",
                    runningHost.Description.Name);
                runningHost.Faulted -= ServiceHostFaulted;
                if (runningHost.State == CommunicationState.Faulted)
                {
                    log.Warn("Service host is faulted; aborting");
                    runningHost.Abort();
                }
                else
                {
                    log.Debug("Service host is OK; closing");
                    runningHost.Close();
                }
            }
            isDisposing = false;
            isDisposed = true;
        }

        private IEnumerable<TimeSpan> CalculateRetryTimes(TimeSpan initial, double multiplier,
            uint count, TimeSpan max)
        {
            yield return initial;
            var next = initial;
            for (int i = 1; i < count; i++)
            {
                next = TimeSpan.FromTicks((long)(next.Ticks * multiplier));
                if (next > max)
                    next = max;
                yield return next;
            }
        }

        private void ServiceHostFaulted(object sender, EventArgs e)
        {
            var host = sender as ServiceHost;
            log.ErrorFormat("Service host raised Faulted event, service name = {0}",
                (host != null) ? host.Description.Name : "(unknown)");
            RestartHost(host);
        }

        private void StartHost()
        {
            bool needRestart = false;
            log.Debug("Attempting to create a new service host");
            ServiceHost serviceHost;
            try
            {
                serviceHost = factory.CreateServiceHost();
            }
            catch (Exception e)
            {
                log.Error("Unable to create a service host", e);
                if (retryCount > retryTimes.Count)
                    throw;
                RestartHost(null);
                return;
            }
            log.InfoFormat("Service host created, service name = {0}",
                serviceHost.Description.Name);
            log.Debug("Attempting to start the service host");
            try
            {
                serviceHost.Open();
                log.Debug("Attaching event handler for Faulted event");
                serviceHost.Faulted += ServiceHostFaulted;
                log.Info("Service host was started successfully");
                runningHost = serviceHost;
            }
            catch (Exception ex)
            {
                needRestart = true;
                log.Error("Failed to open the service host", ex);
                serviceHost.Abort();
            }
            if (needRestart)
                RestartHost(null);
            else
                retryCount = 0;
        }

        private void RestartHost(ServiceHost faultedHost)
        {
            if (faultedHost != null)
            {
                log.Debug("Aborting faulted service host");
                try
                {
                    faultedHost.Abort();
                }
                catch (Exception ex)
                {
                    log.Error("Unable to abort faulted service host", ex);
                }
            }
            if (retryCount < retryTimes.Count)
            {
                retryCount++;
                if (!WaitForRetryDelay(retryCount - 1))
                    return;
                log.DebugFormat("Attempting to restart the service host, retry #{0}", retryCount);
                StartHost();
            }
            else
                log.FatalFormat("Maximum retries reached ({0}). No further attempts will be " +
                    "made to restart the service.", retryTimes.Count);
        }

        private bool WaitFor(TimeSpan delay)
        {
            var incrementalDelay = TimeSpan.FromSeconds(1);
            for (var total = TimeSpan.Zero; total < delay; total += incrementalDelay)
            {
                if (isDisposing || isDisposed)
                    return false;
                Thread.Sleep(incrementalDelay);
            }
            return true;
        }

        private bool WaitForRetryDelay(int retryNumber)
        {
            var delay = (retryNumber < retryTimes.Count) ?
                retryTimes[retryNumber] : retryTimes.Last();
            log.InfoFormat("Going to restart the service host; next retry is in {0}", delay);
            return WaitFor(delay);
        }
    }
}