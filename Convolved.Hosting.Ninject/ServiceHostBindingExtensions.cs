/*
Copyright (C) 2012 Convolved Software

Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

using System;
using System.ServiceModel;
using Ninject;
using Ninject.Syntax;
using Ninject.Extensions.Wcf;

namespace Convolved.Hosting.Ninject
{
    static class ServiceHostBindingExtensions
    {
        public static IBindingWithOrOnSyntax<NinjectServiceHost> Instancing<TService>(
            this IBindingWithOrOnSyntax<NinjectServiceHost> binding, InstanceContextMode mode)
        {
            return (mode == InstanceContextMode.Single) ?
                binding.Singleton<TService>() : binding.Factory<TService>();
        }

        private static IBindingWithOrOnSyntax<NinjectServiceHost> Factory<TService>(
            this IBindingWithOrOnSyntax<NinjectServiceHost> binding)
        {
            return binding
                .WithConstructorArgument("serviceType", typeof(TService))
                .WithConstructorArgument("baseAddresses", new Uri[0]);
        }

        private static IBindingWithOrOnSyntax<NinjectServiceHost> Singleton<TService>(
            this IBindingWithOrOnSyntax<NinjectServiceHost> binding)
        {
            return binding
                .WithConstructorArgument("singletonInstance",
                    ctx => ctx.Kernel.Get<TService>());
        }
    }
}