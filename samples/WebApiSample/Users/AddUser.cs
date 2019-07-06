using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace WebApiSample.Users
{
    public class AddUser : IRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class AddUserHandler : IRequestHandler<AddUser, Unit>
    {
        public Task<Unit> Handle(AddUser request, CancellationToken cancellationToken)
        {
            var nextId = UserList.Users.Max(u => u.Id + 1);
            UserList.Users.Add(
                new User
                {
                    Id = nextId,
                    FirstName = request.FirstName,
                    LastName = request.LastName
                });

            return Unit.Task;
        }
    }
}