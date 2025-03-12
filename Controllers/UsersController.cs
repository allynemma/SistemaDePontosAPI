using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.WebEncoders.Testing;
using SistemaDePontosAPI.Model;

namespace SistemaDePontosAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;

        public UsersController(ILogger<UsersController> logger)
        {
            _logger = logger;
        }

        [HttpPost(Name = "PostUsers")]
        public void Post(string Name, string Password, string Email, string role)
        {
            using (Context ctx = new Context())
            {
                Users users = new Users()
                {
                    Name = Name,
                    Password = Password,
                    Email = Email,
                    Role = role
                };
                ctx.Users.Add(users);
                ctx.SaveChanges();
            }
        }
        [HttpGet(Name = "GetUsers")]
        public IActionResult Get()
        {
            using (Context ctx = new Context()) 
            {
                var users = ctx.Users.ToList();
                return Ok(users);
            } 
        }
        [HttpPut(Name = "PutUsers")]
        public void Put(int id, [FromBody] Users updateUser)
        {
            using (Context ctx = new Context())
            {
                var user = ctx.Users.FirstOrDefault(x => x.Id == id);
                if (user != null)
                {
                    ctx.Entry(updateUser).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    ctx.SaveChanges();
                }
            }
        }

        [HttpDelete(Name = "DeleteUser")]
        public void Delete(int id)
        {
            using (Context ctx = new Context())
            {
                var user = ctx.Users.FirstOrDefault(x => x.Id == id);
                if (user != null)
                {
                    ctx.Users.Remove(user);
                    ctx.SaveChanges();
                }
            }
        }

    }

}
