/*
Copyright (C) 2012 Convolved Software

Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace Convolved.Hosting
{
    /// <summary>
    /// Implements a generic Windows service which runs a <see cref="ServiceEnvironment"/>.
    /// </summary>
    public partial class GenericWcfService : ServiceBase
    {
        private readonly ServiceEnvironment environment;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericWcfService"/> class using the
        /// specified service environment.
        /// </summary>
        /// <param name="environment">A <see cref="ServiceEnvironment"/> comprising the WCF
        /// services that are part of the application.</param>
        public GenericWcfService(ServiceEnvironment environment)
            : base()
        {
            if (environment == null)
                throw new ArgumentNullException("environment");
            this.environment = environment;
            InitializeComponent();
        }

        /// <inheritdoc />
        protected override void OnStart(string[] args)
        {
            // The environment will make several attempts to restart the service with escalating
            // delays, so this should run in a background thread, otherwise a bad start could hang
            // the service controller.
            Task.Factory.StartNew(() => environment.Start());
        }

        /// <inheritdoc />
        protected override void OnStop()
        {
            environment.Stop();
        }
    }
}