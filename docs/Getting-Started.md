# Getting Started with MediatR Caching Extensions

These extensions add caching behaviors to [MediatR](https://github.com/JBogard/MediatR) that can be quickly and easily
dropped into a new or existing pipeline.

## Reference the library 

Add a reference to `Imprise.MediatR.Extensions.Caching` to the project that configures and initialises
[MediatR](https://github.com/JBogard/MediatR).

> NOTE: Ensure you have configured implementations for either `IMemoryCache` or `IDistributedMemoryCache` configured. If
> you have an ASP.NET Core 2.0 or greater web application and you are calling `services.AddMvc()` in your
> `ConfigureServices` method then there is a good chance both of these have been configured with default in-memory
> implementations already.
> 
> Otherwise, simple add `services.AddMemoryCache()`  and `services.AddDistrubutedMemoryCache()` to get started.

## Create a request cache class

For any `IRequest<TResponse>` requests you wish to cache, create a new class using this pattern:

```csharp
public class PingCache : DistributedCache<Ping, Pong>
{
    protected override TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(30);

    public PingCache(IDistributedCache distributedCache) : base(distributedCache)
    {
    }

    protected override string GetCacheKeyIdentifier(Ping request)
    {
        return request.Message.ToString();
    }
}
```

These classes inherit from either the `DistributedCache` or `MemoryCache` `abstract` base classes which wrap the
corresponding cache type implementation to cache your requests response.

The generic parameters `TRequest` and `TResponse` are the request type (`Ping` in this case) and the respons 
type the request returns to be cached (`Pong`).

You must override `GetCacheKeyIdentifier` even if it returns null or an empty string. The key identifier is used by the
base implementation to separate requests in the cache. For example, if you want to cache a user profile response from a
`GetUser` request, you could return the user ID to ensure each user profile is cached separately.

In the example above, every `Message` on the `Ping` class that is different will be cached.

You can also optionally override one of `SlidingExpiration`, `AbsoluteExpiration` and `AbsoluteExpirationRelativeToNow`.
These will be passed to the underlying cache mechanism to control how long requests are cached for. Documentation for
these are in Microsoft's [ASP.NET Core documentation](https://docs.microsoft.com/en-us/aspnet/core/performance/caching/memory?view=aspnetcore-2.2#memorycacheentryoptions)

> NOTE: When using the DistributedCache base class that ships with these extensions, the TResponse type returned by the
> cached request must be marked with the `[Serializable]` attribute as all objects need to be serialized to a byte array
> when stored in an `IDistributedCache` implementation. It is out of scope for this getting started guide, but if needed
> it is possible to derive your own custom class from `ICache<TRequest, TResponse>` if you have particular serialization
> requirements such JSON, BSON or protobuf for example.

## Add the cache behavior to the MediatR pipeline

Consult the [MediatR documentation](https://github.com/jbogard/MediatR/wiki) for full information on configuring your
container to support the pipeline behaviors. In the case of the ASP.NET Core default container, you will likely need
at least to install the [Scrutor](https://github.com/khellang/Scrutor) package to ensure all implementations of your
cached requests are registered.

A basic version may look something like the following, and of course you may have additional pipeline behaviors and
registration logic:

```csharp
    private static IServiceCollection AddMediator(IServiceCollection services)
    {
        // MediatR ServiceFactory
        services.AddScoped<ServiceFactory>(p => p.GetService);
        
        // Configure MediatR Pipeline with cache invalidation and cached request behaviors
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(RequestPreProcessorBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(CacheInvalidationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(CacheBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(RequestPostProcessorBehavior<,>));

        // Use Scrutor to scan and register all classes as their implemented interfaces.
        // This simplifies hooking up any ICache<Request, Response> implementation for the pipeline
        services.Scan(scan => scan
            .FromAssembliesOf(typeof(IMediator), typeof(Startup))
            .AddClasses()
            .AsImplementedInterfaces());

        return services;
    }
```

## Invalidating a cached request

If you don't invalidate your cached requests when something changes, say from another request, you will have to rely
on the cache expiration values before a new version of the request response will be returned.

There are at least two straight-forward ways you can invalidate cached requests:

* Automatic invalidation using instances of `ICacheInvalidator` in conjunction with the `CacheInvalidationBehavior`
* Manually invalidation by injecting an `ICache<Request, Response>` into, for example, an `INotificationHandler` that
responds to a notification that should invalidate the request.

In most case the first option is probably the quickest and easiest, whereas the second is better when you need complete
control of when and how the cache is invalidated.

To use automatic invalidation, create a class that follows this pattern:

```csharp
public class PingCacheInvalidator : CacheInvalidator<MessageUpdated, Ping, Pong>
{
    public PingCacheInvalidator(ICache<Ping, Pong> cache) : base(cache)
    {
    }

    protected override string GetCacheKeyIdentifier(MessageUpdated request)
    {
        return request.message.ToString();
    }
}
```

In this example, let's say the `UpdateMessage` command updated the message for a given `Ping` request, so when we update
the message, the cached versions of `Ping` for that message should be invalidated so that the new message can be return.

We inherit from `CacheInvalidator<TRequest, TCachedRequest, TResponse>` where `TRequest` (`UpdateMessage`) is the
request that will invalidate the `TCachedRequest` (`Ping`) that returns a `TResponse` (`Pong`).

To use the invalidation, just make sure it is added to the pipeline as it is in the example above:

`services.AddScoped(typeof(IPipelineBehavior<,>), typeof(CacheInvalidationBehavior<,>));`

> NOTE: It can be helpful to group your invalidators in the same file or feature folder as the request they invalidate
> as one request could be invalidated by many other requests.

### Manual Invalidation

If it's not possible to invalidate the cached requests through the pipeline behavior, for example, your invalidation
needs to be done through a separate process to your normal MediatR pipeline such as messages coming from an external
queue, another is option is to create an `INotificationHandler` that handles one or `INotification`s and inject an
`ICache<TRequest, TResponse>` directly into it.

For example, let's say we're caching a list of friend connection requests from one user to another. Every user can see
a list of connection requests they've sent and received. Anytime a user sends a connection request to another user we
should invalidate both their cached list of requests. Likewise, if either user revokes the request, we should invalidate
the cache:

```csharp 
public class GetConnectionRequestsCacheInvalidator : INotificationHandler<ConnectionRequestSent>, INotificationHandler<ConnectionRequestRevoked>
{
    private readonly ICache<GetConnectionRequests, List<ConnectionRequest>> _cache;

    public GetConnectionRequestsCacheInvalidator(ICache<GetConnectionRequests, List<ConnectionRequests>> cache)
    {
        _cache = cache;
    }

    public async Task Handle(ConnectionRequestSent notification, CancellationToken cancellationToken)
    {
        await _cache.Remove(notification.FromUserId.ToString());
        await _cache.Remove(notification.ToUserId.ToString());
    }

    public async Task Handle(ConnectionRequestRevoked notification, CancellationToken cancellationToken)
    {
        await _cache.Remove(notification.FromUserId.ToString());
        await _cache.Remove(notification.ToUserId.ToString());
    }
}
```