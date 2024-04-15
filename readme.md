## Apotea AB system design actor model using Microsoft Orleans demo.

### Microsoft Orleans
Orleans is a cross-platform framework for building robust, scalable distributed applications. Distributed applications are defined as apps that span more than a single process, often beyond hardware boundaries using peer-to-peer communication. Orleans scales from a single on-premises server to hundreds to thousands of distributed, highly available applications in the cloud. Orleans extends familiar concepts and C# idioms to multi-server environments. Orleans is designed to scale elastically. When a host joins a cluster, it can accept new activations. When a host leaves the cluster, either because of scale down or a machine failure, the previous activations on that host will be reactivated on the remaining hosts as needed. An Orleans cluster can be scaled down to a single host. The same properties that enable elastic scalability also enable fault tolerance. The cluster automatically detects and quickly recovers from failures.

One of the primary design objectives of Orleans is to simplify the complexities of distributed application development by providing a common set of patterns and APIs. Developers familiar with single-server application development can easily transition to building resilient, scalable cloud-native services and other distributed applications using Orleans. For this reason, Orleans has often been referred to as "Distributed .NET" and is the framework of choice when building cloud-native apps. Orleans runs anywhere that .NET is supported. This includes hosting on Linux, Windows, and macOS. Orleans apps can be deployed to Kubernetes, virtual machines, and PaaS services such as Azure App Service and Azure Container Apps.
### The "Actor Model"
Orleans is based on the "actor model". The actor model originated in the early 1970s and is now a core component of Orleans. The actor model is a programming model in which each actor is a lightweight, concurrent, immutable object that encapsulates a piece of state and corresponding behavior. Actors communicate exclusively with each other using asynchronous messages. Orleans notably invented the Virtual Actor abstraction, wherein actors exist perpetually.

[Read More](https://learn.microsoft.com/en-us/dotnet/orleans/overview).


### Problems and how to resolve in context.
#### Developer productivity
The Orleans programming model raises the productivity of both expert and non-expert programmers by providing the following key abstractions, guarantees, and system services.

#### Familiar object-oriented programming (OOP) paradigm
Grains are .NET classes that implement declared .NET grain interfaces with asynchronous methods. Grains appear to the programmer as remote objects whose methods can be directly invoked. This provides the programmer the familiar OOP paradigm by turning method calls into messages, routing them to the right endpoints, invoking the target grain's methods, and dealing with failures and corner cases transparently.

#### Single-threaded execution of grains
The runtime guarantees that a grain never executes on more than one thread at a time. Combined with the isolation from other grains, the programmer never faces concurrency at the grain level, and never needs to use locks or other synchronization mechanisms to control access to shared data. This feature alone makes the development of distributed applications tractable for non-expert programmers.

#### Transparent activation
The runtime activates a grain only when there is a message for it to process. This cleanly separates the notion of creating a reference to a grain, which is visible to and controlled by application code, and physical activation of the grain in memory, which is transparent to the application. This is similar to virtual memory in that it decides when to "page out" (deactivate) or "page in" (activate) a grain; The application has uninterrupted access to the full "memory space" of logically created grains, whether or not they are in the physical memory at any particular point in time.

Transparent activation enables dynamic, adaptive load balancing via placement and migration of grains across the pool of hardware resources. This feature is a significant improvement on the traditional actor model, in which actor lifetime is application-managed.

#### Location transparency
A grain reference (proxy object) that the programmer uses to invoke the grain's methods or pass to other components contains only the logical identity of the grain. The translation of the grain's logical identity to its physical location and the corresponding routing of messages is done transparently by the Orleans runtime.

Application code communicates with grains while remaining oblivious to their physical location, which may change over time due to failures or resource management or because a grain is deactivated at the time it is called.

#### Transparent integration with a persistent store
Orleans allows for declarative mapping of a grain's in-memory state to a persistent store. It synchronizes updates, transparently guaranteeing that callers receive results only after the persistent state has been successfully updated. Extending and/or customizing the set of existing persistent storage providers available is straightforward.

#### Automatic propagation of errors
The runtime automatically propagates unhandled errors up the call chain with the semantics of asynchronous and distributed try/catch. As a result, errors do not get lost within an application. This allows the programmer to put error handling logic at the appropriate places, without the tedious work of manually propagating errors at each level.

#### Transparent scalability by default
The Orleans programming model is designed to guide the programmer down a path of likely success in scaling an application or service through several orders of magnitude. This is done by incorporating proven best practices and patterns and by providing an efficient implementation of the lower-level system functionality.

Here are some key factors that enable scalability and performance:

#### Implicit fine-grain partitioning of application state
By using grains as directly addressable entities, the programmer implicitly breaks down the overall state of their application. While the Orleans programming model does not prescribe how big or small a grain should be, in most cases it makes sense to have a relatively large number of grains – millions or more – with each representing a natural entity of the application, such as a user account or a purchase order.

With grains being individually addressable and their physical location abstracted away by the runtime, Orleans has enormous flexibility in balancing load and dealing with hot spots transparently and generically without any thought from the application developer.

#### Adaptive resource management
Grains do not assume the locality of other grains as they interact with them. Because of this location transparency, the runtime can manage and adjust the allocation of available hardware resources dynamically. The runtime does this by making fine-grained decisions on placement and migration of grains across the compute cluster in reaction to load and communication patterns—without failing incoming requests. By creating multiple replicas of a particular grain, the runtime can increase the throughput of the grain without making any changes to the application code.

#### Multiplexed communication
Grains in Orleans have logical endpoints, and messaging among them is multiplexed across a fixed set of all-to-all physical connections (TCP sockets). This allows the runtime to host millions of addressable entities with low OS overhead per grain. In addition, activation and deactivation of a grain doesn't incur the cost of registering/unregistering a physical endpoint, such as a TCP port or HTTP URL or even closing a TCP connection.

#### Efficient scheduling
The runtime schedules execution of a large number of single-threaded grains using the .NET Thread Pool, which is highly optimized for performance. With grain code written in the non-blocking, continuation-based style (a requirement of the Orleans programming model), application code runs in a very efficient "cooperative" multi-threaded manner with no contention. This allows the system to reach high throughput and run at very high CPU utilization (up to 90%+) with great stability.

The fact that growth in the number of grains in the system and an increase in the load does not lead to additional threads or other OS primitives helps scalability of individual nodes and the whole system.

#### Explicit asynchrony
The Orleans programming model makes the asynchronous nature of a distributed application explicit and guides programmers to write non-blocking asynchronous code. Combined with asynchronous messaging and efficient scheduling, this enables a large degree of distributed parallelism and overall throughput without the explicit use of multi-threading.

You will find more information in microsoft orlean documentation [here](https://learn.microsoft.com/en-us/dotnet/orleans/).

### Demo Design.
the demo aims to implement the system with the minimal configuration of orleans with showcase 
of how orleans works and how it will fit the needs in multi aspect. here is what is target of 
implementing the culster.
1. Creating a distributed well structure system, not a microservices in term but more toward SOA.
2. How orleans will help into having a proper scaling in need, and how will fit the needs in disaster recovery.
3. The most critical design problem in any distributed system is having a proper state for services and how to share the state between multiply instances.
4. Hosting platform not a matter anymore, so in future if company descide to leave on promise environement and go toward cloud, there are no diffrence in codebase.
1. How to create a test solution to cover the integration tests between services.
5. Manymore.

#### Sytem design in nutshell
design created using lucide site, 
please visit the following link to discover what's inside [the-design-system](https://lucid.app/lucidchart/05c99ddb-2a39-4f78-a7bf-3f86e58fe1fa/edit?viewport_loc=498%2C-113%2C1497%2C703%2C0_0&invitationId=inv_f9315704-81d5-458a-95dc-710951e2e914)

### How to run
#### prerequisites
1. dotnet runtime version 7.x
2. docker desktop or running redis instance.
3. web browser

#### spining up.
1. use [docker-compose](./env/service-dependencies.yml) to start the required service dependencies using the following command `docker compose -f ./.env/service-dependencies.yml --project-name apotea-am-demo up -d`
2. to start the [barcode-service](./src/services/Apotea.Design.ActorModel.Services.Barcode) run the following command `dotnet run --urls=http://localhost:2001 --project ./src/services/Apotea.Design.ActorModel.Services.Barcode` and change the url each time you need to spin up a new instance of the same application.
3. to start the [weight-service](./src/services/Apotea.Design.ActorModel.Services.Weight) run the following command `dotnet run --urls=http://localhost:3001 --project ./src/services/Apotea.Design.ActorModel.Services.Weight` and change the url each time you need to spin up a new instance of the same application.
4. open your web browser to [`http://localhost:2001/api/get-weight/:id`](http://localhost:2001/api/get-weight/1234567)
5. for intensive test run NBomber application to create a load traffic toward the service, then you can navigate to test result dashboard to figure out what's happend.

#### observabillity, monitoring system