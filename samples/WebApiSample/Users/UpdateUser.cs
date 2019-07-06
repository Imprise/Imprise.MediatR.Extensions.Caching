using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace WebApiSample.Users
{
    public class UpdateUser : IRequest
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class UpdateUserHandler : IRequestHandler<UpdateUser, Unit>
    {
        public Task<Unit> Handle(UpdateUser request, CancellationToken cancellationToken)
        {
            var user = UserList.Users.Find(u => u.Id == request.UserId);
            if (user != null)
            {
                user.FirstName = request.FirstName;
                user.LastName = request.LastName;
            }

            return Unit.Task;
        }
    }
}