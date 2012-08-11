/*
Copyright (C) 2012 Convolved Software

Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

using System;

namespace Convolved.Hosting
{
    /// <summary>
    /// Provides a minimal implementation of the <see cref="ServiceEnvironmentHost{T}"/> class.
    /// </summary>
    public class ServiceEnvironmentHost : ServiceEnvironmentHost<ServiceEnvironmentHost>
    {
        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceEnvironmentHost{T}"/> class using
        /// the specified service registry.
        /// </summary>
        public ServiceEnvironmentHost(IServiceRegistry serviceRegistry)
            : base(serviceRegistry)
        {
        }

        /// <inheritdoc />
        public override ServiceEnvironmentHost Self
        {
            get { return this; }
        }
    }
}