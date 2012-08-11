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

namespace Convolved.Hosting
{
    /// <summary>
    /// Encapsulates an environment which hosts several restartable services.
    /// </summary>
    public class ServiceEnvironment : IDisposable
    {
        private readonly IList<ServiceHostContainer> containers;

        private bool isStopped;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceEnvironment"/> class with
        /// the specified service host factories.
        /// </summary>
        /// <param name="factories">An array of factories each responsible for instantiating a
        /// <see cref="ServiceHost"/> instance for a specific service type.</param>
        public ServiceEnvironment(params IServiceHostFactory[] factories)
        {
            containers = (factories ?? Enumerable.Empty<IServiceHostFactory>())
                .Select(f => new ServiceHostContainer(f))
                .ToArray();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Stop();
        }

        /// <summary>
        /// Starts all of the services in the environment.
        /// </summary>
        public virtual void Start()
        {
            if (isStopped)
                throw new ObjectDisposedException(this.GetType().FullName);
            foreach (var container in containers)
                container.Run();
        }

        /// <summary>
        /// Stops all of the services in the environment.
        /// </summary>
        public virtual void Stop()
        {
            if (isStopped)
                return;
            foreach (var container in containers)
                container.Dispose();
            isStopped = true;
        }
    }
}