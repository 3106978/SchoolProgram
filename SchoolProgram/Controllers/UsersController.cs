﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SchoolProgram.Models;

namespace SchoolProgram.Controllers
{
    [Produces("application/json")]
    [Route("api/Users")]
    public class UsersController : Controller
    {
        [HttpGet]
        public List<User> Get()
        {
            return DB.Users.GetUsers();
        }

        [HttpPost]
        public bool InsertUser([FromBody]User us)
        {
            if (us != null && us.NewPassword.Length>=6 && us.NewPassword.Length<=8)
                return DB.Users.InsertUser(us);
            return false;
            
        }
    }
}