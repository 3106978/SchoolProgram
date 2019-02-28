using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SchoolProgram.Models;

namespace SchoolProgram.Controllers
{
    [Produces("application/json")]
    [Route("api/LogAndPas")]
    public class LogAndPasController : Controller
    {
        [HttpGet]
        public UserLogAndPas Login([FromQuery]string login, [FromQuery]string password)
        {
            if (login.Length == 0 || password.Length == 0)
                throw new Exception("The login or password length is 0");
            return DB.Login(login, password);
        }

    }
}