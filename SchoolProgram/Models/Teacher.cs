using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolProgram.Models
{
    public class Teacher
    {
        public int TeacherID { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string PhoneNumber { get; set; }
        public int ClassID { get; set; }
        public int CategoryID { get; set; }
        public int UserID { get; set; }
    }
}
