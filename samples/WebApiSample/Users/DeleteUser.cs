using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace WebApiSample.Users
{
    public class DeleteUser : IRequest
    {
        public int UserId { get; set; }
    }

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