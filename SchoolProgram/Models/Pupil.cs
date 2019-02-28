using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolProgram.Models
{
    public class Pupil
    {
        public int PupilID { get; set; }
        public string  Name { get; set; }
        public string  Surname { get; set; }
        public string  DateOfBirth { get; set; }
        public string  PhoneNumber { get; set; }
        public string  Address { get; set; }
        //public int ClassID { get; set; }
        public int TeacherID { get; set; }
        public int UserID { get; set; }
        public Class Class { get; set; }
         //public string ClassName { get; set; }

    }

    
}
