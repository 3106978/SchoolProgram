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
    [Route("api/Teacher")]
    public class TeacherController : Controller
    {
        [HttpGet("GetClasses")]
        public List<Class> GetClasses()
        {
            return DB.Teachers.GetClasses();
        }

        [HttpGet("GetLessons")]
        public List<Lesson> GetLessonsFromSchedule([FromQuery]DateTime date, [FromQuery]int classId)
        {
            if (date != null && classId >= 0)
            {
                return DB.Teachers.GetLessons(date, classId);
            }
            return null;
        }

        [HttpGet("GetPupilsListToAttendance")]

        public AttendanceWithTeacherComment GetPupilsListToAttendance([FromQuery]DateTime date, [FromQuery]int classId, [FromQuery]int lessonId, [FromQuery] int numberOfLesson)
        {
            if (date!=null && classId>=0 && lessonId>=0 && numberOfLesson>0)
            {
                return DB.Teachers.GetPupilsListFromAttendance(date, classId, lessonId, numberOfLesson);
            }
            return null;
        }

        [HttpPost]
        public Teacher InsertDataToAttendanceTable([FromBody]AttendanceWithTeacherComment attendanceFromClientWithComment)
        {
            if (DB.Teachers.InsertDataToAttendanceTable(attendanceFromClientWithComment))
            {
                return DB.Teachers.GetNameOfTeacherById(attendanceFromClientWithComment.AttendanceList[0].Teacher.TeacherID);
            }
            return null;
        }

        [HttpGet("GetTeacherID")]
        public int GetTeacherID([FromQuery]int userID)
        {
            if (userID < 0)
                throw new Exception("The input data from client is not correct");
            else
                return DB.Teachers.GetTeacherID(userID);
        }

        [HttpGet("getMyClass")]
        public List<Pupil> GetPupilsFromMyClass([FromQuery] int teacherID)
        {
            if (teacherID < 0)
                throw new Exception("The input data from client is not correct");
            return DB.Teachers.GetPupilsFromMyClass(teacherID);
        }

        [HttpPut]
        public bool SendMessage([FromBody]Message m)
        {
            if (m == null)
                throw new Exception("The input data from client is not correct");

            return DB.Teachers.SendMessageToServer(m);
        }

        [HttpGet("getSchedule")]

        public List<Schedule> GetSchedule( DateTime from, DateTime to, int teacherID, bool forTeacher)
        {
            if (from == default(DateTime) || to == default(DateTime) || teacherID <= 0)
                throw new Exception("The input data from client is not correct");
            return DB.Teachers.GetSchedule(from, to, teacherID, forTeacher);
        }

        
    }
}