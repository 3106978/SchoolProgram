using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolProgram.Models
{
    public class Attendance
    {
        public DateTime Date { get; set; }
        public Lesson Lesson { get; set; }
        //public int LessonID { get; set; }
        public Pupil Pupil { get; set; }
        public Teacher Teacher { get; set; }
        public int ClassID { get; set; }
        public bool? Presence { get; set; }
        public int NumberOfLesson { get; set; }
        public string Comment { get; set; }
        
    }
    public class AttendanceWithTeacherComment
    {
        public List<Attendance> AttendanceList { get; set; }
        public string TeachersComment { get; set; }

        public AttendanceWithTeacherComment(List<Attendance> list, string comment)
        {
            AttendanceList = list;
            TeachersComment = comment;
        }
    }
}
