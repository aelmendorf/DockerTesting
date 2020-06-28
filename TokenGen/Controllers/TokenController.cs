using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace TokenGen.Controllers {
    [ApiController]
    [Route("[controller]")]
    public class TokenController : Controller {
        [HttpGet]
        public dynamic Get() {
            return new {
                Guid = Guid.NewGuid().ToString(),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = Environment.MachineName
            };
        }
    }
}
