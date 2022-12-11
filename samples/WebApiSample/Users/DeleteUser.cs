using MediatR;

namespace WebApiSample.Users
{
    public record DeleteUser(int UserId) : IRequest;

    public class DeleteUserHandler : IRequestHandler<DeleteUser, Unit>
    {
        public Task<Unit> Handle(DeleteUser request, CancellationToken cancellationToken)
        {
            var user = UserList.Users.Find(u => u.Id == request.UserId);
            if (user != null)
            {
                UserList.Users.Remove(user);
            }

            return Unit.Task;
        }
    }
}