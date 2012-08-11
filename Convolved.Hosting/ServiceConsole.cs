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
    /// Runs a <see cref="ServiceEnvironment"/> in the active <see cref="Console"/>, providing
    /// support for safe shutdown via Ctrl-C.
    /// </summary>
    public static class ServiceConsole
    {
        /// <summary>
        /// Starts a service console using a <see cref="ServiceEnvironment"/> created from the
        /// specified <paramref name="factories"/>.
        /// </summary>
        /// <param name="factories">An array of factories each responsible for instantiating a
        /// <see cref="System.ServiceModel.ServiceHost"/> instance for a specific service 
        /// type.</param>
        public static void Run(params IServiceHostFactory[] factories)
        {
            Run(new ServiceEnvironment(factories));
        }

        /// <summary>
        /// Starts a service console using the specified environment.
        /// </summary>
        /// <param name="environment">A <see cref="ServiceEnvironment"/> initialized with the
        /// <see cref="IServiceHostFactory"/> instances responsible for instantiating individual
        /// service hosts.</param>
        public static void Run(ServiceEnvironment environment)
        {
            if (environment == null)
                throw new ArgumentNullException("environment");
            Console.TreatControlCAsInput = false;
            bool aborted = false;
            Console.CancelKeyPress += (sender, e) =>
            {
                Console.WriteLine();
                Console.WriteLine("Interrupted - shutting down services");
                e.Cancel = false;
                aborted = true;
                environment.Dispose();
                Console.WriteLine("All services shut down.");
            };
            environment.Start();
            if (aborted)
                return;
            Console.WriteLine("Server started. To safely shut down, press Ctrl+C.");
            while (true)
                Console.ReadLine();
        }
    }
}