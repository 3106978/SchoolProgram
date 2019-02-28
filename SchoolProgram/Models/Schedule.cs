using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolProgram.Models
{
    public class Schedule
    {
        public DateTime  Date { get; set; }
        public int NumberOfLesson { get; set; }
        public Lesson Lesson { get; set; }
        public Class Class { get; set; }
        public Teacher Teacher { get; set; }
        public string TeachersComment { get; set; }

        
    }
}
