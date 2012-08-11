/*
Copyright (C) 2012 Convolved Software

Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

using System;
using Castle.DynamicProxy;

namespace Convolved.Hosting
{
    /// <summary>
    /// Provides a method to create a WCF client which restarts after a fault, using an interface
    /// proxy and a <see cref="RestartOnFaultInterceptor{TService}"/>.
    /// </summary>
    public static class AutoRestartClient
    {
        private static readonly ProxyGenerator proxyGenerator = new ProxyGenerator();

        /// <summary>
        /// Creates a service proxy which automatically recreates faulted channels.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="factory">A delegate for creating the wrapped instances of
        /// <typeparamref name="TService"/>.</param>
        /// <returns>A new <typeparamref name="TService"/> wrapper object which will continue to
        /// handle invocations after a fault.</returns>
        public static TService Create<TService>(Func<TService> factory)
            where TService : class
        {
            if (factory == null)
                throw new ArgumentNullException("factory");
            return proxyGenerator.CreateInterfaceProxyWithoutTarget<TService>(
                new RestartOnFaultInterceptor<TService>(factory));
        }
    }
}