/*
Copyright (C) 2012 Convolved Software

Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

using System;
using System.ServiceModel;

namespace Convolved.Hosting
{
    /// <summary>
    /// Provides functionality for registering WCF services on behalf of a
    /// <see cref="ServiceEnvironmentHost"/>.
    /// </summary>
    public interface IServiceRegistry
    {
        /// <summary>
        /// Registers a service using the default instancing mode.
        /// </summary>
        /// <typeparam name="TService">The type of service to host.</typeparam>
        void Add<TService>();

        /// <summary>
        /// Registers a service using the specified instancing mode.
        /// </summary>
        /// <typeparam name="TService">The type of service to host.</typeparam>
        /// <param name="mode">The instancing mode for the service.</param>
        void Add<TService>(InstanceContextMode mode);

        /// <summary>
        /// Initializes a <see cref="ServiceEnvironment"/> instance with all services registered
        /// via the <see cref="Add{TService}()"/> method/overloads.
        /// </summary>
        /// <returns>A new <see cref="ServiceEnvironment"/> configured to run all registered
        /// services.</returns>
        ServiceEnvironment CreateServiceEnvironment();
    }
}