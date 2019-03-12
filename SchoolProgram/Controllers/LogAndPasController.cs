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
    {/// <summary>
    /// Validation of user.
    /// Check login and password with httpGet query
    /// 
    /// </summary>
    /// <param name="login"></param>
    /// <param name="password"></param>
    /// <returns> if login and password are valides - return instatnce of model UserLogAndPas 
    /// with primary data about user (who is he (teacher, new user, admin))</returns>
        [HttpGet]
        public UserLogAndPas Login([FromQuery]string login, [FromQuery]string password)
        {
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
                throw new ArgumentNullException("The login or password is null or empty");
            return DB.Login(login, password);
        }

    }
}