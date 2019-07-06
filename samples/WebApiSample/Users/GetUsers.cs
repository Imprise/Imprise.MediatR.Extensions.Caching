using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Imprise.MediatR.Extensions.Caching;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;

namespace WebApiSample.Users
{
    public class GetUsers : IRequest<List<User>>
    {
    }

    public class GetUsersHandler : IRequestHandler<GetUsers, List<User>>
    {
        // returns the complete list of users from the static, in-memory collection
        public Task<List<User>> Handle(GetUsers request, CancellationToken cancellationToken)
        {
            return Task.FromResult(UserList.Users);
        }
    }

    /// <summary>
    /// This class will be automatically processed in the request pipeline by CacheBehavior.
    /// 
    /// It will cause the List<User> response from GetUsers to be added to the registered implementation of
    /// IDistributedCached with a sliding expiry of 30 minutes.
    ///
    /// The UserId property on the request will be used as the cache key identifier so each different user will be
    /// cached separately based on their user id.
    /// </summary>
    public class GetUsersCache : DistributedCache<GetUsers, List<User>>
    {
        // expire the cache in five minutes from now regardless of how many times it's accessed in this time.
        protected override TimeSpan? AbsoluteExpirationRelativeToNow => new TimeSpan(0, 5, 0);

        public GetUsersCache(IDistributedCache memoryCache) : base(memoryCache)
        {
        }

        protected override string GetCacheKeyIdentifier(GetUsers getUsers)
        {
            // in this case, return the whole list from cache. In production apps, the key could be a criteria object
            // including skip and take numbers to only cache a particular page etc.
            return string.Empty;
        }
    }

    /// <summary>
    /// Invalidates the cached list of users (GetUsers Request) whenever a User is Added
    /// </summary>
    public class AddUserGetUsersCacheInvalidator : CacheInvalidator<AddUser, GetUsers, List<User>>
    {
        public AddUserGetUsersCacheInvalidator(ICache<GetUsers, List<User>> cache) : base(cache)
        {
        }

        protected override string GetCacheKeyIdentifier(AddUser request)
        {
            // invalidates entire list so no extra cache key / criteria is sent
            return string.Empty;
        }
    }
    
    /// <summary>
    /// Invalidates the cached list of users (GetUsers Request) whenever a User is Updated
    /// </summary>
    public class UpdateUserGetUsersCacheInvalidator : CacheInvalidator<UpdateUser, GetUsers, List<User>>
    {
        public UpdateUserGetUsersCacheInvalidator(ICache<GetUsers, List<User>> cache) : base(cache)
        {
        }

        protected override string GetCacheKeyIdentifier(UpdateUser request)
        {
            // invalidates entire list so no extra cache key / criteria is sent
            return string.Empty;
        }
    }

    /// <summary>
    /// Invalidates the cached list of users (GetUsers Request) whenever a User is Deleted
    /// </summary>
    public class DeleteUserGetUsersCacheInvalidator : CacheInvalidator<DeleteUser, GetUsers, List<User>>
    {
        public DeleteUserGetUsersCacheInvalidator(ICache<GetUsers, List<User>> cache) : base(cache)
        {
        }

        protected override string GetCacheKeyIdentifier(DeleteUser request)
        {
            // invalidates entire list so no extra cache key / criteria is sent
            return string.Empty;
        }
    }
}