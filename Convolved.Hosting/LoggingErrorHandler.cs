/*
Copyright (C) 2012 Convolved Software

Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using log4net;

namespace Convolved.Hosting
{
    /// <summary>
    /// Implements a WCF error handler which uses log4net to log exceptions as fatal errors.
    /// </summary>
    public class LoggingErrorHandler : IErrorHandler, IServiceBehavior
    {
        private readonly ILog log = LogManager.GetLogger(typeof(LoggingErrorHandler));

        #region IErrorHandler Members

        /// <inheritdoc />
        public bool HandleError(Exception error)
        {
            log.Fatal("An unhandled exception occurred during the operation", error);
            return false;
        }

        /// <inheritdoc />
        public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {
        }

        #endregion

        #region IServiceBehavior Members

        /// <inheritdoc />
        public void AddBindingParameters(ServiceDescription serviceDescription, 
            ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, 
            BindingParameterCollection bindingParameters)
        {
        }

        /// <inheritdoc />
        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, 
            ServiceHostBase serviceHostBase)
        {
            foreach (ChannelDispatcher dispatcher in serviceHostBase.ChannelDispatchers)
                dispatcher.ErrorHandlers.Add(this);
        }

        /// <inheritdoc />
        public void Validate(ServiceDescription serviceDescription, 
            ServiceHostBase serviceHostBase)
        {
        }

        #endregion
    }
}