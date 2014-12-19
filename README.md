ObjectPersistence
=================

Watch the video walkthrough on my YouTube channel at http://youtu.be/DxSGSZ3k89s

It has been several years (six years to be precise) since I published my article “What is Object Persistence” (https://thomasjaeger.wordpress.com/2008/06/15/what-is-object-persistence/). I have received great feedback since then from the blog post and from presentations I gave about object persistence. However, the challenge of using an appropriate object persistence mechanism still exists today. Fortunately, there are even more ways to store your objects nowadays when compared to 2008. With the great opportunities that cloud computing offers, object persistence gets even more exciting.

So, I decided to publish a follow-up blog post about object persistence. In addition, I will also provide working C# code so that you can try it out yourself. Since object persistence is such an important piece of a software architecture and the depth of technical information about it can be overwhelming, I may have to spread out my thoughts and example source code over additional blog posts.

Most of the example source code I will be providing is coming straight from production systems I have built over the years with .Net and C#. Specifically, I will be providing persistence providers that you can use in your own systems or at least provide you with a huge head start. Some of the source code is changed to accommodate the example better but the provider pattern and the overall design is identical. The source code will be in C# and I will be using a .Net feature that has been available since .NET 2.0 – The Provider Pattern.

So, having said all this, I can pickup where I have left off with my first blog post. Say, you are familiar with the challenges of finding the right object persistence for your project. Let’s also assume you know “where” you want to store your objects in. If you are not familiar of what object persistence is, please take a look at my previous blog post “What is Object Persistence”.

Let’s start with a straight forward object persistence that will help you see the bigger picture and not get lost in the actual details of “how” to store the objects. At least for now. Let’s start with storing our objects in an object database named db4o. The reason why I want to start out with db4o is because it actually is the easiest way in .Net to persist your objects. I would argue that object databases can be used in at least 90% of .Net projects developed today. db4o has another advantage in that it can also run entirely in memory alone which is great for unit testing your persistence. In addition, db4o has such an extremely low learning curve that you will be up and running in no-time. Of course, the beauty of using a provider model is that you can do entirely different persistence implementations of the same domain model. So, you can use a different object database such as VelocityDB, for example.

Later on, I will show you how to store the same C# objects in different ways including the in memory version of db4o, a NoSQL solution such as Redis, which is a key-value store, and to round it out, a typical SQL storage such as SQL Server.

From an architecture point of view, it is very, very, important that our domain model has absolutely no clue about persistence. Our domain model will have no references to any persistence assemblies. Our domain model will be a lone assembly with no references to any service, interfaces, UI, and especially any persistence technologies. This is important because we want our domain model to be maintainable over time. You want your domain model to be independent from any other building blocks of your architecture because it will reflect your business domain and processes. This will make your entire system much easier to maintain and therefore much easier to react to requirements changes.

The Provider Model

The great thing about using the provider model is that the entire implementation is done inside a provider. Your entire source code on “how” to persist your objects is inside the specific provider. If you are not familiar with the provider model, please take a look at these resources to get familiar with it.

Microsoft ASP.NET 2.0 Providers: Introduction
(http://msdn.microsoft.com/en-us/library/aa478948.aspx)

Develop Provider-based Features of Your Application
(http://www.codemag.com/Article/0903091)

Besides the introduction of generics in .NET 2, the provider model was in my opinion one of the most powerful features introduced. The .NET framework has been using the provider model internally ever since, all the way to the latest version of .NET 4.5. Here are some examples, where Microsoft is using the provider model:

Membership
Role management
Site map
Profile
Session state
Web events
Web Parts personalization
Protected configuration

and these are just a few of the current providers that ship with the framework. You can even build providers based on certain features of your system, for example. See the link above.

One of the great features of the provider model is that the framework will automatically load your persistence provider based on configuration information. This means that you do not even need any assembly references to your provider, the .NET framework will take care of the discovery, loading, and instanciation for you. This offers a truly decoupled implementation, a true plug-play mechanism out of the box. How cool is that?

Please keep in mind that this has nothing to do with the Repository pattern. I have used the provider model pattern for persistence for many years now and the Repository pattern does not come close to what the provider pattern can do for you. The Repository pattern violates the domain model encapsulation because the domain model is now aware of some sort of persistence idea even if the Repository is exposed via IRepository, for example. That is a big no no in my book because your goal should be to create something that is easy to maintain over a very long time.

If you are building a professional software solution, you should go with the provider pattern for abstracting persistence.

This took longer than I thought but I believe that I needed to set the stage first before we can continue. In the next blog post, we’ll get our hands dirty and start writing code and you will see how it can be done.
