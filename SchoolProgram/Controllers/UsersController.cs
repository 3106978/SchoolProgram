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

        [HttpGet("getAttendanceforUser")]
        public List<Attendance> GetAttendanceListForUser(int userID, DateTime date)
        {
            if (userID <= 0 || date == null)
                throw new Exception("The input data from client side is not correct");

            return DB.Users.GetPupilAttendanceForUser(userID, date);
        }

        [HttpGet("getMessages")]
        public List<Message> GetMessages (int userID)
        {
            if (userID <= 0)
                throw new Exception("The input data from client side is not correct");
            return DB.Users.GetMessages(userID);
        }
        [HttpGet("getSchedule")]

        public List<Schedule> GetSchedule(DateTime from, DateTime to, int userID)
        {
            if (from == default(DateTime) || to == default(DateTime) || userID <= 0)
                throw new Exception("The input data from client is not correct");
            return DB.Users.GetSchedule(userID, from, to );
        }

    }
}