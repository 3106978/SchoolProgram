using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolProgram.Models
{
    public class UserLogAndPas
    {
        public int UserID { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public bool IsATeacher { get; set; }
        public bool IsNewUser { get; set; }
        public bool IsAdmin { get; set; }
        
    }
}
