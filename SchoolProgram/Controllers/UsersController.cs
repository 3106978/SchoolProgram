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
    {/// <summary>
    /// Insert new User with new password to db
    /// </summary>
    /// <param name="us">object User</param>
    /// <returns>true - if user was inserted to db successfully</returns>
        [HttpPost]
        public bool InsertUser([FromBody]User us)
        {
            if (us != null && us.NewPassword.Length>=6 && us.NewPassword.Length<=8)
                return DB.Users.InsertUser(us);
            return false;
            
        }
        /// <summary>
        /// Get attendance of pupil from db by date and user id of his pearent
        /// </summary>
        /// <param name="userID">user id - pupil's pearent</param>
        /// <param name="date">selected date</param>
        /// <returns></returns>
        [HttpGet("getAttendanceforUser")]
        public List<Attendance> GetAttendanceListForUser(int userID, DateTime date)
        {
            userID.CheckValue("userID");
            if (date == null)
                throw new Exception("The input data from client side is not correct");

            return DB.Users.GetPupilAttendanceForUser(userID, date);
        }

        /// <summary>
        /// Get List of Messages from Teachers
        /// </summary>
        /// <param name="userID">user id</param>
        /// <returns>List of Messages</returns>
        [HttpGet("getMessages")]
        public List<Message> GetMessages (int userID)
        {
            userID.CheckValue("userID");
            return DB.Users.GetMessages(userID);
        }

        /// <summary>
        /// Get  Schedule of lessons with Teacher's comment by dates  
        /// </summary>
        /// <param name="from">from date</param>
        /// <param name="to">to date</param>
        /// <param name="userID">id of user (pupil's pearent)</param>
        /// <returns>List Schedule</returns>
        [HttpGet("getSchedule")]
        public List<Schedule> GetSchedule(DateTime from, DateTime to, int userID)
        {
            userID.CheckValue("userID");
            if (from == null || to == null )
                throw new Exception("The input date from client is not correct");
            return DB.Users.GetSchedule(userID, from, to );
        }

    }
}