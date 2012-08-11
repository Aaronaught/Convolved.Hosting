Convolved.Hosting
===========================

Overview
--------
The Convolved Hosting Framework is, quite simply, an attempt at making self-hosted WCF services not suck.

Self-hosting is critical for unattended/high-availability services, and also the clear choice over WPA for message-queuing based services (i.e. `netMsmqBinding`). Unfortunately, if you follow the vast majority of self-hosting examples on the web, you'll find that your system is incredibly fragile and not even close to being production-ready.

The framework performs several important functions for you:

* Restarting the host if it faults, using an escalating retry pattern to minimize thrashing and maximize the chance of success in case of downtime;
* Trapping and logging all unhandled exceptions using log4net;
* Allowing convenient integration of Dependency Injection frameworks;
* Running custom initialization actions before any services start;
* Dual-hosting as a Windows Service or Console Application via a command-line switch;
* A fluent configuration syntax which makes it trivially easy to configure and run a host;
* And also some utilities for enabling fault-tolerance on client proxies, to allow for proper DI without incurring the overhead of closing/reopening channels all the time.

Usage
-----
It's super easy! Just import `Convolved.Hosting` assembly into your self-hosting project. If you're using [Ninject](http://www.ninject.org/) (my framework of choice), you should also import `Convolved.Hosting.Ninject` to make your life a little easier. Then, write the startup code in your `Main` method or other entry point, as below:

**Ninject Version**

    static void Main(string[] args)
    {
		// If you haven't already configured log4net...
		log4net.Config.XmlConfigurator.Configure();
		
		// Initialize and start the service hosts
	    new Convolved.Hosting.Ninject.ServiceEnvironmentHost()
            .Name("MyService")
            .Modules(new MyNinjectModule1(), new MyNinjectModule2())
            .Service<MyService1>()
			.Service<MyService2>()
            .InitializeWith(Init)
            .RunWithCommandLineArgs(args);
	}
	
	private static void Init(IKernel kernel)
	{
		// Add your custom initialization code here
	}
	
**Other Containers**

You will need to implement `IServiceRegistry`. Refer to the `Convolved.Hosting.Ninject.ServiceRegistry` class for an example. Then pass this in your constructor to the generic `ServiceEnvironmentHost`. For example, a Windsor version might look like the following:

	var container = new WindsorContainer();
	// Register types in the container here...
	var registry = container.Resolve<IServiceRegistry>();
	new Convolved.Hosting.ServiceEnvironmentHost(registry)
		.Name("MyService")
		.Service<MyServiceImplementation>()
		.InitializeWith(Init)
		.RunWithCommandLineArgs(args);

If you want to create a reusable IoC integration library that supports custom initialization actions, you may also want to create a custom `ServiceEnvironmentHost` class similar to the `Convolved.Hosting.Ninject.ServiceEnvironmentHost` and override the `Initialize` method. Please refer to the Ninject version for an example.

**Installing/Running the Host**

To install a hosted service as a Windows service, use the `ServiceInstaller` and `ProjectInstaller` classes together with the `installutil` command-line tool, as with any other Windows service.

To run a Convolved host as a console application, add the `/console` switch, e.g.

    MyService.exe /console
	
That's it!

Copyright and License
---------------------
Copyright (C) 2012 Convolved Software

Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.