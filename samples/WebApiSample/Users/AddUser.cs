using MediatR;

namespace WebApiSample.Users
{
    public record AddUser : IRequest
    {
        public required string FirstName;
        public required string LastName;
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