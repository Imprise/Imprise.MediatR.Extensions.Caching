using System;

namespace WebApiSample.Users
{
    // when a class instance could be added to a distributed cache using binary serialisation,
    // we need to mark the class with the Serializable attribute
    [Serializable] 
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}