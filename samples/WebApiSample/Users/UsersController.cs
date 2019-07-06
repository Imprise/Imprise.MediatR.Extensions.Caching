using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebApiSample.Users
{
    /// <summary>
    /// Controller for Users with Get by ID, Get all, Post, Put and Delete methods
    /// </summary>
    [Route("users")]
    public class UsersController : Controller
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            return Ok(await _mediator.Send(new GetUsers()));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser([FromRoute] int id)
        {
            return Ok(await _mediator.Send(new GetUser
            {
                UserId = id
            }));
        }

        [HttpPost]
        public async Task<IActionResult> PostUser([FromBody] AddUser request)
        {
            await _mediator.Send(request);
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> PutUser([FromBody] UpdateUser request)
        {
            await _mediator.Send(request);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser([FromRoute] int id)
        {
            await _mediator.Send(new DeleteUser
            {
                UserId = id
            });
            return Ok();
        }
    }
}