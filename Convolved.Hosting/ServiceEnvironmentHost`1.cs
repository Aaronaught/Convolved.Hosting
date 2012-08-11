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
using System.ServiceProcess;

namespace Convolved.Hosting
{
    /// <summary>
    /// Provides a base class for a <see cref="ServiceEnvironment"/> host, which can run as either
    /// a Windows Service or Console application.
    /// </summary>
    /// <typeparam name="T">The concrete type of the environment host.</typeparam>
    public abstract class ServiceEnvironmentHost<T> : IFluentSyntax<T>
    {
        private readonly IServiceRegistry serviceRegistry;
        private string name;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceEnvironmentHost{T}"/> class using
        /// the specified service registry.
        /// </summary>
        /// <param name="serviceRegistry">The service registry, used to register and create
        /// individual service instances.</param>
        public ServiceEnvironmentHost(IServiceRegistry serviceRegistry)
        {
            if (serviceRegistry == null)
                throw new ArgumentNullException("serviceRegistry");
            this.serviceRegistry = serviceRegistry;
        }

        /// <summary>
        /// Changes the service name used for the Windows service controller.
        /// </summary>
        /// <param name="name">The <see cref="ServiceBase.ServiceName"/> that the created
        /// <see cref="ServiceBase"/> will have in a Windows service environment.</param>
        /// <returns>The current <typeparamref name="T"/> host instance.</returns>
        public T Name(string name)
        {
            this.name = name;
            return Self;
        }

        /// <inheritdoc cref="IServiceRegistry.Add{T}()" />
        /// <returns>The current <typeparamref name="T"/> host instance.</returns>
        public T Service<TService>()
        {
            serviceRegistry.Add<TService>();
            return Self;
        }

        /// <inheritdoc cref="IServiceRegistry.Add{T}(InstanceContextMode)" />
        /// <returns>The current <typeparamref name="T"/> host instance.</returns>
        public T Service<TService>(InstanceContextMode mode)
        {
            serviceRegistry.Add<TService>(mode);
            return Self;
        }

        /// <summary>
        /// Starts a service environment running in the active console window.
        /// </summary>
        public void RunAsConsoleApplication()
        {
            Initialize();
            using (var env = serviceRegistry.CreateServiceEnvironment())
            {
                ServiceConsole.Run(env);
            }
        }

        /// <summary>
        /// Starts a service environment running as a Windows service.
        /// </summary>
        /// <param name="additionalServicesToRun">An array of additional services to run in the
        /// service process.</param>
        public void RunAsWindowsService(params ServiceBase[] additionalServicesToRun)
        {
            Initialize();
            var env = serviceRegistry.CreateServiceEnvironment();
            var wcfService = new GenericWcfService(env);
            if (!string.IsNullOrEmpty(name))
                wcfService.ServiceName = name;
            var servicesToRun = new List<ServiceBase> { wcfService };
            if (additionalServicesToRun != null)
                servicesToRun.AddRange(additionalServicesToRun);
            ServiceBase.Run(servicesToRun.ToArray());
        }

        /// <summary>
        /// Configures and runs an environment based on a set of command-line arguments.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        public void RunWithCommandLineArgs(params string[] args)
        {
            if ((args != null) &&
                args.Contains("/console", StringComparer.InvariantCultureIgnoreCase))
                RunAsConsoleApplication();
            else
                RunAsWindowsService();
        }

        /// <inheritdoc />
        public abstract T Self { get; }

        /// <summary>
        /// Executes any required initialization actions before creating the service environment.
        /// </summary>
        protected virtual void Initialize()
        {
        }
    }
}