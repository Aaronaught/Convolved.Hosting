/*
Copyright (C) 2012 Convolved Software

Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.ServiceModel;
using Ninject;
using Ninject.Extensions.Wcf;

namespace Convolved.Hosting.Ninject
{
    class ServiceRegistry : IServiceRegistry
    {
        private readonly IKernel kernel;
        private readonly List<Action<IKernel>> bindActions = new List<Action<IKernel>>();

        public ServiceRegistry(IKernel kernel)
        {
            if (kernel == null)
                throw new ArgumentNullException("kernel");
            this.kernel = kernel;
        }

        public void Add<TService>()
        {
            Add<TService>(InstanceContextMode.PerCall);
        }

        public void Add<TService>(InstanceContextMode mode)
        {
            bindActions.Add(kernel => BindService<TService>(kernel, mode));
        }

        public ServiceEnvironment CreateServiceEnvironment()
        {
            kernel.Bind<ServiceEnvironment>().ToSelf();
            foreach (var bindAction in bindActions)
                bindAction(kernel);
            return kernel.Get<ServiceEnvironment>();
        }

        internal void CreateBindings(IKernel kernel)
        {
            foreach (var bindAction in bindActions)
                bindAction(kernel);
        }

        private static void AddDefaultBehavior(ServiceHost serviceHost, InstanceContextMode mode)
        {
            var behavior = serviceHost.Description.Behaviors.Find<ServiceBehaviorAttribute>();
            if (behavior == null)
            {
                behavior = new ServiceBehaviorAttribute();
                serviceHost.Description.Behaviors.Add(behavior);
            }
            behavior.InstanceContextMode = mode;
        }

        private void BindService<TService>(IKernel kernel, InstanceContextMode mode)
        {
            string name = typeof(TService).FullName;
            kernel.Bind<ServiceHost>()
                .To<NinjectServiceHost>()
                .Named(name)
                .Instancing<TService>(mode);
            Func<ServiceHost> factoryFunc = () => kernel.Get<ServiceHost>(name);
            kernel.Bind<IServiceHostFactory>()
                .To<AnonymousServiceHostFactory>()
                .Named(name)
                .WithConstructorArgument("factory", factoryFunc);
        }
    }
}
