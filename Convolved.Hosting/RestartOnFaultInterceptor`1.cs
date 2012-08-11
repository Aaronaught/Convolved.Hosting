/*
Copyright (C) 2012 Convolved Software

Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

using System;
using System.Reflection;
using System.ServiceModel;
using Castle.DynamicProxy;
using log4net;

namespace Convolved.Hosting
{
    /// <summary>
    /// Implements an interceptor which recreates the underlying communication object before a
    /// method call if the communication object is faulted.
    /// </summary>
    /// <typeparam name="TService">The service type or interface.</typeparam>
    public class RestartOnFaultInterceptor<TService> : IInterceptor
        where TService : class
    {
        private readonly ILog log = 
            LogManager.GetLogger(typeof(RestartOnFaultInterceptor<TService>));
        private readonly Func<TService> factory;

        private TService instance;
        private ICommunicationObject communicationObject;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestartOnFaultInterceptor{TService}"/>
        /// using the specified factory method.
        /// </summary>
        /// <param name="factory">A delegate method for creating new instances of the
        /// <typeparamref name="TService"/> type.</param>
        public RestartOnFaultInterceptor(Func<TService> factory)
        {
            if (factory == null)
                throw new ArgumentNullException("factory");
            this.factory = factory;
            CreateService();
        }

        /// <inheritdoc />
        public void Intercept(IInvocation invocation)
        {
            log.DebugFormat("Intercepting service call, target type = {0}, method = {1}",
                invocation.TargetType.FullName, invocation.Method.Name);
            if (invocation.Method.Name == "Dispose")
            {
                SafeDispose();
                return;
            }
            EnsureService();
            try
            {
                log.Debug("Executing the intercepted method");
                invocation.ReturnValue = invocation.Method.Invoke(instance, invocation.Arguments);
            }
            catch (TargetInvocationException e)
            {
                log.InfoFormat("Invocation threw an exception of type {0}.", 
                    typeof(TargetInvocationException).FullName);
                if (e.InnerException != null)
                {
                    log.DebugFormat("Found inner exception of type {0}.",
                        e.InnerException.GetType().FullName);
                    throw e.InnerException;
                }
                log.DebugFormat("No inner exception found.");
                throw;
            }
        }

        private void CreateService()
        {
            log.DebugFormat("Creating service, type = {0}", typeof(TService).FullName);
            instance = factory();
            if (instance == null)
                log.Warn("Service instance factory returned a null instance.");
            else
                log.InfoFormat("Service instance created, actual type = {1}", 
                    instance.GetType().FullName);
            communicationObject = instance as ICommunicationObject;
            if ((communicationObject == null) && (instance != null))
                log.WarnFormat("Service instance of concrete type {0} does not implement {1}. " +
                    "The interceptor will not be able to detect faults or restart the service.",
                    instance.GetType().FullName, typeof(ICommunicationObject).FullName);
        }

        private void EnsureService()
        {
            if ((communicationObject != null) &&
                ((communicationObject.State & CommunicationState.Faulted) != 0))
            {
                log.Debug("Service instance is in faulted state - aborting...");
                communicationObject.Abort();
                communicationObject = null;
                instance = null;
                log.Info("Faulted service instance was removed.");
            }
            if (instance == null)
                CreateService();
        }

        private void SafeDispose()
        {
            log.Debug("Dispose requested for service instance.");
            if (communicationObject == null)
            {
                log.Debug("No communication object found to dispose.");
                return;
            }
            if ((communicationObject.State & CommunicationState.Faulted) != 0)
            {
                log.Debug("Communication object is in faulted state - aborting...");
                communicationObject.Abort();
            }
            else
            {
                log.Debug("Communication object is OK - closing...");
                communicationObject.Close();
            }
            log.Debug("Communication object was successfully disposed.");
        }
    }
}