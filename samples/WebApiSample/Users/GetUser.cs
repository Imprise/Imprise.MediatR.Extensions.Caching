using System;
using System.Threading;
using System.Threading.Tasks;
using Imprise.MediatR.Extensions.Caching;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;

namespace WebApiSample.Users
{
    public class GetUser : IRequest<User>
    {
        public int UserId { get; set; }
    }

    public class GetUserHandler : IRequestHandler<GetUser, User>
    {
        public Task<User> Handle(GetUser request, CancellationToken cancellationToken)
        {
            return Task.FromResult(
                UserList.Users.Find(u => u.Id == request.UserId)
            );
        }
    }

    /// <summary>
    /// This class will be automatically processed in the request pipeline by CacheBehavior.
    /// 
    /// It will cause the User response from GetUser to be added to the registered implementation of IDistributedCached
    /// with a sliding expiry of 30 minutes.
    ///
    /// The UserId property on the request will be used as the cache key identifier so each different user will be
    /// cached separately based on their user id.
    /// </summary>
    public class GetUserCache : DistributedCache<GetUser, User>
    {
        /// <summary>
        /// Each time the cached response is retrieved another 30 minutes will be added to the time before the cached
        /// response expires.
        /// </summary>
        protected override TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(30);

        public GetUserCache(IDistributedCache distributedCache) : base(distributedCache)
        {
        }

        protected override string GetCacheKeyIdentifier(GetUser request)
        {
            // cache every response where the user id is different
            return request.UserId.ToString();
        }
    }

    /// <summary>
    /// This class will be automatically processed in the request pipeline by CacheInvalidationBehavior.
    ///
    /// It will cause the User response from GetUser to be removed from the cache using the registered implementation of
    /// IDistributedCached
    ///
    /// The UserId property on the update request will be used as the cache key identifier to identify the cached response
    /// that will be removed.
    /// </summary>
    public class UpdateUserGetUserCacheInvalidator : CacheInvalidator<UpdateUser, GetUser, User>
    {
        public UpdateUserGetUserCacheInvalidator(ICache<GetUser, User> cache) : base(cache)
        {
        }

        protected override string GetCacheKeyIdentifier(UpdateUser request)
        {
            return request.UserId.ToString();
        }
    }
}