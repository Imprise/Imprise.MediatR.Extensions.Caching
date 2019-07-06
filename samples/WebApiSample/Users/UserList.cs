using System.Collections.Generic;

namespace WebApiSample.Users
{ 
    /// <summary>
    /// Serves as a simple way to store users in memory in order to demonstrate cache retrieval and invalidation
    /// </summary>
    internal static class UserList
    {
        public static readonly List<User> Users = new List<User>
        {
            new User()
            {
                Id = 1,
                FirstName = "Alice",
                LastName = "Ally"
            },
            new User
            {
                Id = 2,
                FirstName = "Bob",
                LastName = "Brolly"
            }
        };
    }
}