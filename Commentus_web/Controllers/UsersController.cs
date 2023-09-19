using Commentus_web.Attributes;
using Commentus_web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http;

namespace Commentus_web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private TestContext _context { get; }

        public UsersController(TestContext context)
        {
            _context = context;
        }

        public IActionResult GetUsers()
        {
            return Ok(_context.Users);
        }

        [SessionFilter]
        [HttpGet("id/{id}")]
        public IActionResult GetUserById(int id)
        {
            var tempUser = _context.Users.Where(u => u.Id == id).FirstOrDefault();

            if (tempUser != null)
            {
                return Ok(new UsersApiModel() 
                { 
                    Id = tempUser.Id, 
                    isAdmin = tempUser.IsAdmin, 
                    Name = tempUser.Name 
                });
            }

            return BadRequest();
        }

        [SessionFilter]
        [HttpGet("name/{name}")]
        public IActionResult GetUserByName(string name)
        {
            var tempUser = _context.Users.Where(u => u.Name == name).FirstOrDefault();

            if (tempUser != null)
            {
                return Ok(new UsersApiModel()
                {
                    Id = tempUser.Id,
                    isAdmin = tempUser.IsAdmin,
                    Name = tempUser.Name
                });
            }
            return BadRequest();
        }
    }
}
