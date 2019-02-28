using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolProgram.Models
{
    public class Lesson
    {
        public int LessonID { get; set; }
        public string LessonName { get; set; }
        public int TeacherID { get; set; }
        public int NumberOfLesson { get; set; }
        
    }
}
