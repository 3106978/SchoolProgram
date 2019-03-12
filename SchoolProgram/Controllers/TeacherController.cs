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
        /// <summary>
        /// Get List of all classes from data base
        /// </summary>
        /// <returns>List of all classes from data base</returns>
        [HttpGet("GetClasses")]
        public List<Class> GetClasses()
        {
            return DB.Teachers.GetClasses();
        }
        /// <summary>
        /// Get List of lessons from Schedule table in data base by date and class
        /// </summary>
        /// <param name="date"></param>
        /// <param name="classId"></param>
        /// <returns>List of lessons from Schedule table</returns>
        [HttpGet("GetLessons")]
        public List<Lesson> GetLessonsFromSchedule([FromQuery]DateTime date, [FromQuery]int classId)
        {
            if (date == null)
                throw new ArgumentNullException("The Date is null", nameof(date));
            classId.CheckValue("classID");
            return DB.Teachers.GetLessons(date, classId);

        }
        /// <summary>
        /// get list of pupils from DB to make attendance
        /// </summary>
        /// <param name="date">selected date</param>
        /// <param name="classId">id of selected class</param>
        /// <param name="lessonId">id of selected lesson</param>
        /// <param name="numberOfLesson">number of selected lesson in schedule</param>
        /// <returns></returns>
        [HttpGet("GetPupilsListToAttendance")]

        public AttendanceWithTeacherComment GetPupilsListToAttendance([FromQuery]DateTime date, [FromQuery]int classId, [FromQuery]int lessonId, [FromQuery] int numberOfLesson)
        {
            if (date == null)
                throw new ArgumentNullException("The Date is null", nameof(date));
            classId.CheckValue("class ID");
            lessonId.CheckValue("lesson ID");
            numberOfLesson.CheckValue("number of lesson");

            return DB.Teachers.GetPupilsListFromAttendance(date, classId, lessonId, numberOfLesson);

        }
        /// <summary>
        /// insert to db data of attendance from teacher and return object Teacher to show in table in client side who maked an attendance
        /// </summary>
        /// <param name="attendanceFromClientWithComment"> list of pupils with attendance and comments about pupil's work + teacher's comment to lesson</param>
        /// <returns> object Teacher who maked this attendance</returns>
        [HttpPost]
        public Teacher InsertDataToAttendanceTable([FromBody]AttendanceWithTeacherComment attendanceFromClientWithComment)
        {
            if (attendanceFromClientWithComment==null)
                throw new ArgumentNullException("The object attendance from client with comment is null", nameof(attendanceFromClientWithComment));
            if (DB.Teachers.InsertDataToAttendanceTable(attendanceFromClientWithComment))
            {
                return DB.Teachers.GetNameOfTeacherById(attendanceFromClientWithComment.AttendanceList[0].Teacher.TeacherID);
            }
            return null;
        }

        /// <summary>
        /// Get from DB Teacher ID by User ID 
        /// </summary>
        /// <param name="userID">User ID</param>
        /// <returns>int teacher ID</returns>
        [HttpGet("GetTeacherID")]
        public int GetTeacherID([FromQuery]int userID)
        {
            userID.CheckValue("userID");
                return DB.Teachers.GetTeacherID(userID);
        }

        /// <summary>
        /// Get list of Pupils from teacher's class
        /// </summary>
        /// <param name="teacherID">teacher id</param>
        /// <returns>list of pupils</returns>
        [HttpGet("getMyClass")]
        public List<Pupil> GetPupilsFromMyClass([FromQuery] int teacherID)
        {
            teacherID.CheckValue("teacherID");
            return DB.Teachers.GetPupilsFromMyClass(teacherID);
        }
        /// <summary>
        /// Send message from teacher to user (pearent of pupil)
        /// </summary>
        /// <param name="m">message</param>
        /// <returns>true - if message is sent successfully to db</returns>
        [HttpPut]
        public bool SendMessage([FromBody]Message m)
        {
            if (m == null)
                throw new Exception("The input data from client is not correct");

            return DB.Teachers.SendMessageToServer(m);
        }
        /// <summary>
        /// Get Schedule from db by dates and teacher id
        /// </summary>
        /// <param name="from"> from date</param>
        /// <param name="to">to date</param>
        /// <param name="teacherID">teacher id </param>
        /// <param name="forTeacher">true- if teacher need his work schedule, and false - if he need schedule of his class</param>
        /// <returns>list Schedule</returns>
        [HttpGet("getSchedule")]

        public List<Schedule> GetSchedule(DateTime from, DateTime to, int teacherID, bool forTeacher)
        {
            teacherID.CheckValue("TeacherID");
            if (from == null || to == null )
                throw new Exception("The dates from client is not correct");
            return DB.Teachers.GetSchedule(from, to, teacherID, forTeacher);
        }


    }
}