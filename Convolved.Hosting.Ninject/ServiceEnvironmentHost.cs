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
using Ninject;
using Ninject.Extensions.Wcf;
using Ninject.Modules;

namespace Convolved.Hosting.Ninject
{
    /// <summary>
    /// Implements a runnable Windows host which can run as either a Windows Service or Console
    /// application, using Ninject for dependency injection.
    /// </summary>
    public class ServiceEnvironmentHost : ServiceEnvironmentHost<ServiceEnvironmentHost>
    {
        private readonly IKernel kernel;
        private readonly List<Action<IKernel>> initializationActions = new List<Action<IKernel>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceEnvironmentHost"/> class using a standard kernel.
        /// </summary>
        public ServiceEnvironmentHost()
            : this(new StandardKernel())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceEnvironmentHost"/> class using the specified kernel.
        /// </summary>
        /// <param name="kernel">An existing Ninject <see cref="IKernel"/> instance.</param>
        public ServiceEnvironmentHost(IKernel kernel)
            : base(new ServiceRegistry(kernel))
        {
            if (kernel == null)
                throw new ArgumentNullException("kernel");
            this.kernel = kernel;
        }

        /// <summary>
        /// Register an action to be run on initialization, before the service host is started.
        /// </summary>
        /// <param name="action">The action to run.</param>
        /// <returns>The current <see cref="ServiceEnvironmentHost"/> instance.</returns>
        public ServiceEnvironmentHost InitializeWith(Action<IKernel> action)
        {
            if (action == null)
                throw new ArgumentNullException("action");
            initializationActions.Add(action);
            return this;
        }

        /// <summary>
        /// Loads additional modules into the <see cref="Kernel"/>.
        /// </summary>
        /// <param name="modules">An array of <see cref="NinjectModule"/> instances to 
        /// load.</param>
        /// <returns>The current <see cref="ServiceEnvironmentHost"/> instance.</returns>
        public ServiceEnvironmentHost Modules(params NinjectModule[] modules)
        {

            Kernel.Load(modules);
            return this;
        }

        /// <inheritdoc />
        protected override void Initialize()
        {
            base.Initialize();
            foreach (var action in initializationActions)
                action(Kernel);
        }

        /// <summary>
        /// Gets the Ninject kernel ued to resolve dependencies.
        /// </summary>
        public IKernel Kernel
        {
            get { return kernel; }
        }

        /// <inheritdoc />
        public override ServiceEnvironmentHost Self
        {
            get { return this; }
        }
    }
}