using SchoolProgram.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.Diagnostics;

namespace SchoolProgram
{
    public class DB
    {/// <summary>
    /// connection string to data base
    /// </summary>
        internal static string CONN_STRING;

        public static UserLogAndPas Login(string login, string password)
        {
            using (SqlConnection conn = new SqlConnection(CONN_STRING))
            {
                conn.CheckDBConnection();
                conn.Open();
                return ValidateUser(login, password, conn);
            }
        }

        /// <summary>
        /// Validation of user. Check login and password in Data Base and if user is new user 
        /// the method return instance of model UserLogAndPass with primary data about user 
        /// (who is he? Teacher, User, Admin, New User). 
        /// If User is not new the method send to the database about the time of the last visit of user 
        /// </summary>
        /// <param name="login"></param>
        /// <param name="password"></param>
        /// <param name="conn"> opened SqlConnection</param>
        /// <returns></returns>
        private static UserLogAndPas ValidateUser(string login, string password, SqlConnection conn)
        {
            string sql = "SELECT UserID, IsNewUser, IsAdmin, IsATeacher FROM LogsAndPass WHERE " +
                "Login=@Login AND Password=@Password";
            using (SqlCommand command = new SqlCommand(sql, conn))
            {
                command.Add("@Login", login);
                command.Add("@Password", password);
                UserLogAndPas user;
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.HasRows)
                        return null;
                    
                    reader.Read();
                    user = new UserLogAndPas
                    {
                        UserID = reader.GetInt32(0),
                        Login = null,
                        Password = null,
                        IsNewUser = reader.GetBoolean(reader.GetOrdinal("IsNewUser")),
                        IsATeacher = reader.GetBoolean(reader.GetOrdinal("IsATeacher")),
                        IsAdmin = reader.GetBoolean(reader.GetOrdinal("IsAdmin"))
                    };
                }

                if (!user.IsNewUser)
                {
                    using (SqlCommand updateCommand = new SqlCommand("UPDATE Users SET " +
                        "LastLogin=@LastLogin WHERE UserID=@UserID ", conn))
                    {
                        updateCommand.Add("@LastLogin", DateTime.Now);
                        updateCommand.Add("@UserID", user.UserID);
                        updateCommand.ExecuteNonQuery();
                    }
                    return user;
                }
                return user;
            }


        }
        public static class Users
        {/// <summary>
        /// Insert new User to table Users with new password
        /// </summary>
        /// <param name="user">object User</param>
        /// <returns>true - if user was inserted successfully</returns>
            internal static bool InsertUser(User user)
            {
                using (SqlConnection conn = new SqlConnection(CONN_STRING))
                {
                    conn.CheckDBConnection();
                    conn.Open();
                    string sql = "SET IDENTITY_INSERT Users ON INSERT INTO Users(UserID, Email, LastLogin)" +
                        " VALUES (@UserID, @Email, @LastLogin) SET IDENTITY_INSERT Users OFF;" +
                        " UPDATE LogsAndPass SET Password=@newPassword, IsNewUser=@IsNewUser WHERE UserID=@UserID ";
                    using (SqlCommand command = new SqlCommand(sql, conn))
                    {
                        command.Add("@UserID", user.UserID);
                        command.Add("@Email", user.EMail);
                        command.Add("@LastLogin", DateTime.Now);
                        command.Add("@newPassword", user.NewPassword);
                        command.Add("@IsNewUser", false);
                        return command.ExecuteNonQuery() == 2;
                    }
                }

            }

            /// <summary>
            /// Get Attendance of pupil by date and pearent's user id
            /// </summary>
            /// <param name="userID">pearent's user id</param>
            /// <param name="date">selected date </param>
            /// <returns>list Attendance with data about pupil's attendance in corrent date </returns>
            internal static List<Attendance> GetPupilAttendanceForUser(int userID, DateTime date)
            {
                using (SqlConnection conn = new SqlConnection(CONN_STRING))
                {
                    conn.CheckDBConnection();
                    conn.Open();
                    string sql = "SELECT Pupils.PupilID,  Presence, Teachers.Name, Teachers.Surname, Comment, " +
                        "NumberOfLesson, Lessons.LessonName FROM Pupils INNER JOIN(SELECT PupilID, Presence, TeacherID," +
                        " NumberOfLesson, LessonID, Comment FROM Attendance WHERE  Date = @date AND PupilID IN " +
                        "(select PupilID FROM Pupils WHERE UserID = @userID)) t ON Pupils.PupilID = t.PupilID " +
                        "INNER JOIN Teachers ON t.TeacherID = Teachers.TeacherID INNER JOIN Lessons ON " +
                        "Lessons.LessonID = t.LessonID";
                    using (SqlCommand comm = new SqlCommand(sql, conn))
                    {
                        comm.Add("@date", date);
                        comm.Add("@userID", userID);
                        using (SqlDataReader rd = comm.ExecuteReader())
                        {
                            List<Attendance> list = new List<Attendance>();
                            while (rd.Read())
                            {
                                Attendance att = new Attendance
                                {
                                    Pupil = new Pupil { PupilID = rd.GetInt32(0) },
                                    Presence = rd.GetBoolean(rd.GetOrdinal("Presence")),
                                    Teacher = new Teacher
                                    {
                                        Name = rd.GetString(2),
                                        Surname = rd.GetString(3)
                                    },
                                    Comment = rd.GetString(4),
                                    NumberOfLesson = rd.GetInt32(5),
                                    Lesson = new Lesson { LessonName = rd.GetString(6) }

                                };

                                list.Add(att);
                            }
                            return list;
                        }
                    }
                }
            }

            /// <summary>
            /// Get List of Messages from tachers to read
            /// </summary>
            /// <param name="userID">id of user (pearent of pupil)</param>
            /// <returns>List of Messages</returns>
            internal static List<Message> GetMessages(int userID)
            {
                using (SqlConnection conn = new SqlConnection(CONN_STRING))
                {
                    conn.CheckDBConnection();
                    conn.Open();
                    string sql = "SELECT Messages.Date, Teachers.Name, Teachers.Surname, Messages.Message " +
                        "FROM Messages INNER JOIN Teachers ON Messages.UserID = @userID AND Teachers.TeacherID IN " +
                        "(SELECT TeacherID FROM Messages WHERE UserID = @userID) ORDER BY Date";
                    using (SqlCommand comm = new SqlCommand(sql, conn))
                    {
                        comm.Add("@userID", userID);
                        using (SqlDataReader rd = comm.ExecuteReader())
                        {
                            List<Message> list = new List<Message>();
                            while (rd.Read())
                            {
                                Message m = new Message()
                                {
                                    Date = rd.GetDateTime(0),
                                    Teacher = new Teacher
                                    {
                                        Name = rd.GetString(1),
                                        Surname = rd.GetString(2)
                                    },
                                    MessageText = rd.GetString(3)
                                };
                                list.Add(m);
                            }
                            return list;
                        }
                    }


                }

            }

            /// <summary>
            /// Get Schedule from db by date and by user id with teacher's comment to lesson
            /// </summary>
            /// <param name="userID">id of user (pupil's pearent)</param>
            /// <param name="from">from date</param>
            /// <param name="to">to date</param>
            /// <returns>List Schedule</returns>
            internal static  List<Schedule> GetSchedule(int userID, DateTime from, DateTime to)
            {
                using (SqlConnection conn = new SqlConnection(CONN_STRING))
                {
                    conn.CheckDBConnection();
                    conn.Open();
                    string sql = "SELECT Date, [Number Of Lesson], TeachersComment, Lessons.LessonName," +
                        " Teachers.Name, Teachers.Surname, Teachers.TeacherID FROM(SELECT * FROM Schedule WHERE Date >= @from AND Date <= @to" +
                        " AND ClassID IN(SELECT ClassID FROM Pupils WHERE  UserID = @userID)) t LEFT OUTER JOIN Lessons" +
                        " ON Lessons.LessonID = t.LessonID  LEFT OUTER JOIN Teachers ON Teachers.TeacherID = t.TeacherID " +
                        " ORDER BY Date";
                    using (SqlCommand comm = new SqlCommand(sql, conn))
                    {
                        comm.Add("@from", from);
                        comm.Add("@to", to);
                        comm.Add("@userID", userID);
                        using (SqlDataReader rd = comm.ExecuteReader())
                        {
                            List<Schedule> list = new List<Schedule>();
                            while (rd.Read())
                            {
                                Schedule s = new Schedule();
                                s.Date = rd.GetDateTime(0);
                                s.NumberOfLesson = rd.GetInt32(1);
                                s.TeachersComment = rd.CheckEmptyStringFromDB(2);
                                s.Lesson = new Lesson
                                {
                                    LessonName = rd.GetString(3)
                                };
                                s.Teacher = new Teacher
                                {
                                    Name = rd.GetString(4),
                                    Surname = rd.GetString(5)
                                };
                                list.Add(s);
                            }
                            return list;
                        }
                    }
                }
            }
        }

        public static class Teachers
        {/// <summary>
        /// Get Teacher ID from DB by User ID
        /// </summary>
        /// <param name="userID">user id</param>
        /// <returns>int teacher id</returns>
            internal static int GetTeacherID(int userID)
            {
                using (SqlConnection conn = new SqlConnection(CONN_STRING))
                {
                    conn.CheckDBConnection();
                    conn.Open();
                    string sql = "SELECT TeacherID FROM Teachers WHERE UserID=@userId";
                    using (SqlCommand comm = new SqlCommand(sql, conn))
                    {
                        comm.Add("@userId", userID);
                        using (SqlDataReader rd = comm.ExecuteReader())
                        {
                            rd.Read();
                            return rd.GetInt32(0);
                        }
                    }

                }

            }
           
            /// <summary>
            /// Get all classes from dataBase
            /// </summary>
            /// <returns>List of all classes</returns>
            internal static List<Class> GetClasses()
            {
                List<Class> classes = new List<Class>();
                using (SqlConnection conn = new SqlConnection(CONN_STRING))
                {
                    conn.CheckDBConnection();
                    conn.Open();
                    string sql = "SELECT * FROM Classes";
                    using (SqlCommand comm = new SqlCommand(sql, conn))
                    {
                        using (SqlDataReader reader = comm.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Class cl = new Class
                                {
                                    ClassID = reader.GetInt32(0),
                                    ClassName = reader.GetString(2)
                                };
                                classes.Add(cl);
                            }
                        }
                    }
                }
                return classes;
            }
           
            /// <summary>
            /// Check the db table "Attendance" if data about pupils is already exists
            /// </summary>
            /// <param name="date">selected date </param>
            /// <param name="classId">id of selected class</param>
            /// <param name="lessonId">id of selected lesson</param>
            /// <param name="numberOfLesson">number of selected lesson in schedule</param>
            /// <param name="conn">sql connection object</param>
            /// <returns>true- if data of pupils is already exists in table attendance</returns>
            private static bool AttendanceHasData(DateTime date, int classId, int lessonId, int numberOfLesson, SqlConnection conn)
            {
                string sql = "SELECT PupilID FROM Attendance WHERE Date=@date AND ClassID=@class" +
                    " AND LessonID=@lesson AND NumberOfLesson = @numOfLesson";
                using (SqlCommand comm = new SqlCommand(sql, conn))
                {
                    comm.Add("@date", date.ToShortDateString());
                    comm.Add("@class", classId);
                    comm.Add("@lesson", lessonId);
                    comm.Add("@numOfLesson", numberOfLesson);
                    using (SqlDataReader reader = comm.ExecuteReader())
                    {
                        if (reader.HasRows)
                            return true;
                    }
                }
                return false;
            }

            /// <summary>
            /// Get List of lessons from Schedule table in data base by date and class id
            /// </summary>
            /// <param name="date">checked date of lessons</param>
            /// <param name="classId">id of checked class</param>
            /// <returns>list of lessons from schedule by date and class id with data about each lesson
            /// (name of lesson, his number in schedule and id)</returns>
            internal static List<Lesson> GetLessons(DateTime date, int classId)
            {
                using (SqlConnection conn = new SqlConnection(CONN_STRING))
                {
                    if (conn == null)
                        throw new Exception("Connection is null");
                    conn.Open();
                    string sql = "SELECT Lessons.LessonID, Lessons.LessonName," +
                        " [Number of lesson] FROM Lessons INNER JOIN " +
                        "(SELECT LessonID, [Number of Lesson] FROM Schedule" +
                        " WHERE Date = @Date AND ClassID = @classId) temp ON Lessons.LessonID = temp.LessonID";
                    using (SqlCommand comm = new SqlCommand(sql, conn))
                    {
                        List<Lesson> lessons = new List<Lesson>();
                        comm.Add("@Date", date.ToShortDateString());
                        comm.Add("@classId", classId);
                        using (SqlDataReader reader = comm.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Lesson l = new Lesson
                                {
                                    LessonID = reader.GetInt32(0),
                                    LessonName = reader.GetString(1),
                                    NumberOfLesson = reader.GetInt32(2)
                                };
                                lessons.Add(l);
                            }

                            return lessons;
                        }
                    }
                }
            }

            /// <summary>
            /// Get list of pupils from table attendance in the db . 
            /// First there is a check whether there is already data on attendance in the table. If data is exists - 
            /// we jast get this data from table, get teacher's comment to lesson and send this to client side in object AttendanceWithTeacherComment;
            /// If data about attendance is not exists in db we firstly get pupils  list from db by selected class id and create object
            /// AttendanceWithTeacherComment with comment of teacher=null and presence of pupil=null. 
            /// </summary>
            /// <param name="date">selected date from client</param>
            /// <param name="classId">id of selected class</param>
            /// <param name="lessonId">id of selected lesson</param>
            /// <param name="numberOfLesson">number of selected lesson in schedule</param>
            /// <returns>AttendanceWithTeacherComment (List of Attendance objects and TeacherComment)</returns>
            internal static AttendanceWithTeacherComment GetPupilsListFromAttendance(DateTime date, int classId, int lessonId, int numberOfLesson)
            {
                using (SqlConnection conn = new SqlConnection(CONN_STRING))
                {
                    conn.CheckDBConnection();
                    conn.Open();
                    bool dbHaveTheData = AttendanceHasData(date, classId, lessonId, numberOfLesson, conn);
                    List<Pupil> pupilsListWithoutAttendance = new List<Pupil>();
                    List<Attendance> list = new List<Attendance>();
                    if (!dbHaveTheData)
                    {
                        pupilsListWithoutAttendance = GetPupilsToAttendance(classId, conn);
                        for (int i = 0; i < pupilsListWithoutAttendance.Count; i++)
                        {
                            Attendance a = new Attendance
                            {
                                Pupil = new Pupil
                                {
                                    PupilID = pupilsListWithoutAttendance[i].PupilID,
                                    Name = pupilsListWithoutAttendance[i].Name,
                                    Surname = pupilsListWithoutAttendance[i].Surname,
                                },
                                Presence = null,
                                Teacher = new Teacher
                                {
                                    Name = "",
                                    Surname = ""
                                },
                                Comment = null,
                                Lesson = new Lesson { LessonID = lessonId },
                                NumberOfLesson = numberOfLesson,
                                Date = date,
                                ClassID = classId,
                            };
                            list.Add(a);
                        }
                        AttendanceWithTeacherComment listWithTeacherCommentToLesson = new AttendanceWithTeacherComment(list, null);

                        return listWithTeacherCommentToLesson;
                    }
                    string sql = "SELECT Pupils.PupilID, Pupils.Name, Pupils.Surname, Presence, Teachers.Name, " +
                       "Teachers.Surname, Comment FROM Pupils INNER JOIN (SELECT PupilID, Presence, " +
                       "TeacherID, Comment FROM Attendance WHERE Date = @date AND ClassID = @classid " +
                       "AND LessonID = @lessonId) t ON Pupils.PupilID = t.PupilID " +
                       "LEFT OUTER JOIN Teachers ON t.TeacherID IS NOT NULL AND Teachers.TeacherID = t.TeacherID";
                    using (SqlCommand comm = new SqlCommand(sql, conn))
                    {
                        comm.Add("@date", date);
                        comm.Add("@classId", classId);
                        comm.Add("@lessonId", lessonId);
                        using (SqlDataReader reader = comm.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Attendance at = new Attendance();
                                at.Pupil = new Pupil
                                {
                                    PupilID = reader.GetInt32(0),
                                    Name = reader.GetString(1),
                                    Surname = reader.GetString(2)
                                };
                                at.Presence = reader.GetBoolean(reader.GetOrdinal("Presence"));
                                at.Teacher = new Teacher
                                {
                                    Name = reader.CheckEmptyStringFromDB(4),//Extension method
                                    Surname = reader.CheckEmptyStringFromDB(5)
                                };
                                at.Comment = reader.GetString(6);
                                at.Lesson = new Lesson { LessonID = lessonId };
                                at.NumberOfLesson = numberOfLesson;
                                at.Date = date;
                                at.ClassID = classId;
                                list.Add(at);
                            }

                        }
                        comm.CommandText = "SELECT TeachersComment FROM Schedule WHERE Date=@date AND" +
                               " ClassID=@classId AND [Number of Lesson]=@lessonNumber";
                        comm.Add("@lessonNumber", numberOfLesson);
                        using (SqlDataReader rd = comm.ExecuteReader())
                        {
                            AttendanceWithTeacherComment listWithComment = new AttendanceWithTeacherComment(list, null);

                            rd.Read();
                            if (!rd.IsDBNull(0))
                            {
                                listWithComment.TeachersComment = rd.GetString(0);
                            }

                            return listWithComment;
                        }

                    }
                }

            }
           
            /// <summary>
            /// get list of pupils from db by  class id
            /// </summary>
            /// <param name="classID">selected class id</param>
            /// <param name="conn"> sql connection</param>
            /// <returns> list of Pupils</returns>
            private static List<Pupil> GetPupilsToAttendance(int classID, SqlConnection conn)
            {
                string sql = "SELECT PupilID, Name, Surname FROM Pupils WHERE PupilID IN " +
                    "(SELECT PupilID FROM Pupils WHERE ClassID = @classID)";
                using (SqlCommand comm = new SqlCommand(sql, conn))
                {
                    comm.Add("@classID", classID);

                    using (SqlDataReader rd = comm.ExecuteReader())
                    {
                        List<Pupil> list = new List<Pupil>();
                        while (rd.Read())
                        {
                            Pupil p = new Pupil();
                            p.PupilID = rd.GetInt32(0);
                            p.Name = rd.GetString(1);
                            p.Surname = rd.GetString(2);

                            list.Add(p);
                        }

                        return list;
                    }
                }
            }
           
            /// <summary>
            /// Insert to db table "Attendance" list of pupils with presence and comments about their work
            /// and insert to table "Schedule" comment to lesson from teacher
            /// </summary>
            /// <param name="list"> list of pupils with attendance and comments + teacher's comment to lesson (AttendanceWithTeacherComment)</param>
            /// <returns>true- if data inserted successfully</returns>
            internal static bool InsertDataToAttendanceTable(AttendanceWithTeacherComment list)
            {
                using (SqlConnection conn = new SqlConnection(CONN_STRING))
                {
                    conn.CheckDBConnection();
                    conn.Open();
                    string sql = "INSERT INTO Attendance VALUES (@date, @lessonId, @pupilId,  @presence, @teacherId, @classId, " +
                        "@numberOfLesson, @comment)";
                    using (SqlCommand command = new SqlCommand(sql, conn))
                    {
                        int teacherID = list.AttendanceList[0].Teacher.TeacherID;
                        DateTime date = list.AttendanceList[0].Date;
                        int number = list.AttendanceList[0].NumberOfLesson;
                        command.Add("@teacherId", teacherID);
                        command.Add("@date", date);
                        command.Add("@number", number);
                        command.Add("@presence", SqlDbType.Bit);
                        command.Add("@comment", SqlDbType.VarChar);
                        command.Add("@pupilId", SqlDbType.Int);
                        command.Add("@teacherComment", list.TeachersComment);
                        command.Add("@numberOfLesson", list.AttendanceList[0].NumberOfLesson);
                        command.Add("@classId", list.AttendanceList[0].ClassID);
                        command.Add("@lessonId", list.AttendanceList[0].Lesson.LessonID);

                        int count = 0;
                        for (int i = 0; i < list.AttendanceList.Count; i++)
                        {
                            command.Parameters["@presence"].Value = list.AttendanceList[i].Presence;
                            command.Parameters["@comment"].Value = list.AttendanceList[i].Comment;
                            command.Parameters["@pupilId"].Value = list.AttendanceList[i].Pupil.PupilID;

                            count += command.ExecuteNonQuery();

                        }
                        command.CommandText = "UPDATE Schedule SET TeachersComment=@teacherComment WHERE Date=@date" +
                            " AND [Number of lesson]=@numberOfLesson AND ClassID=@classId";

                        command.ExecuteNonQuery();
                        return count == list.AttendanceList.Count;
                    }
                }
            }
           
            /// <summary>
            /// Get name and surname of teacher who maked attendance to show this in client side
            /// </summary>
            /// <param name="id">id of teacher who maked attendance</param>
            /// <returns> object Teacher with name and surname</returns>
            internal static Teacher GetNameOfTeacherById(int id)
            {
                using (SqlConnection conn = new SqlConnection(CONN_STRING))
                {
                    conn.CheckDBConnection();
                    conn.Open();
                    string sql = "SELECT Name, Surname FROM Teachers WHERE TeacherID=@id";
                    using (SqlCommand comm = new SqlCommand(sql, conn))
                    {
                        comm.Add("@id", id);
                        using (SqlDataReader dr = comm.ExecuteReader())
                        {
                            dr.Read();
                            Teacher t = new Teacher
                            {
                                Name = dr.GetString(0),
                                Surname = dr.GetString(1)
                            };
                            return t;
                        }
                    }
                }
            }
           
            /// <summary>
            /// Get list of pupils from db from teacher's class
            /// </summary>
            /// <param name="teacherId">teacher id</param>
            /// <returns>list of pupils</returns>
            internal static List<Pupil> GetPupilsFromMyClass(int teacherId)
            {
                using (SqlConnection conn = new SqlConnection(CONN_STRING))
                {
                    conn.CheckDBConnection();
                    conn.Open();
                    string sql = "SELECT PupilID, Pupils.Name, Surname, DateOfBirth, PhoneNumber, Address, UserID, " +
                        "Classes.ClassName FROM Pupils inner join Classes on Classes.ClassID in (select ClassID from Teachers " +
                        "where TeacherID = @teacherId) and Pupils.ClassID in (select ClassID from Teachers where TeacherID = @teacherId)";

                    using (SqlCommand comm = new SqlCommand(sql, conn))
                    {
                        comm.Add("@teacherId", teacherId);
                        using (SqlDataReader reader = comm.ExecuteReader())
                        {
                            List<Pupil> pupils = new List<Pupil>();
                            while (reader.Read())
                            {
                                Pupil p = new Pupil();
                                p.PupilID = reader.GetInt32(0);
                                p.Name = reader.GetString(1);
                                p.Surname = reader.GetString(2);
                                p.DateOfBirth = reader.GetDateTime(3).ToShortDateString();
                                p.PhoneNumber = reader.GetString(4);
                                p.Address = reader.GetString(5);
                                p.UserID = reader.GetInt32(6);
                                p.Class = new Class { ClassName = reader.GetString(7) };
                                pupils.Add(p);
                            }
                            return pupils;
                        }
                    }
                }
            }
           
            /// <summary>
            /// Send message from teacher to pupil's pearent and save wthis in db
            /// </summary>
            /// <param name="m">Message from teacher</param>
            /// <returns>true- if message sent successfully</returns>
            internal static bool SendMessageToServer(Message m)
            {
                using (SqlConnection conn = new SqlConnection(CONN_STRING))
                {
                    conn.CheckDBConnection();
                    conn.Open();
                    string sql = "INSERT INTO Messages (Date, TeacherID, PupilID, UserIDOfPearent, Message) VALUES " +
                        "(@date, @teacher, @pupil, @user, @mess)";
                    using (SqlCommand comm = new SqlCommand(sql, conn))
                    {

                        comm.Add("@date", m.Date);
                        comm.Add("@teacher", m.Teacher.TeacherID);
                        comm.Add("@pupil", m.PupilID);
                        comm.Add("@user", m.UserID);
                        comm.Add("@mess", m.MessageText);
                        return comm.ExecuteNonQuery() == 1;
                    }

                }
            }
           
            /// <summary>
            /// Get Teacher's Work Schedule by dates from db if variable forTeacher==true,
            /// or get Classe's lessons schedule bu dates from db if variable forTeacher==false
            /// </summary>
            /// <param name="from">From date </param>
            /// <param name="to"> to date</param>
            /// <param name="teacherID"> id of teacher who want get schedule</param>
            /// <param name="forTeacher">boolean variable</param>
            /// <returns>list Schedule</returns>
            internal static List<Schedule> GetSchedule(DateTime from, DateTime to, int teacherID, bool forTeacher)
            {
                using (SqlConnection conn = new SqlConnection(CONN_STRING))
                {
                    if (conn == null)
                        throw new Exception("Connection is null");
                    conn.Open();
                    string sql = "SELECT Date, [Number Of Lesson], TeachersComment,Lessons.LessonName, " +
                        "Classes.ClassName, Teachers.Name , Teachers.Surname FROM (SELECT * FROM Schedule WHERE Date >= @from and Date <= @to" +
                        " and TeacherID = @teacherID) t LEFT OUTER JOIN Lessons ON Lessons.LessonID = t.LessonID " +
                        "LEFT OUTER JOIN Classes ON Classes.ClassID = t.ClassID left outer join Teachers ON " +
                        "Teachers.TeacherID=@teacherID ORDER BY Date, [Number of Lesson]";
                    if (!forTeacher)
                    {
                        sql = "SELECT Date, [Number Of Lesson], TeachersComment, Lessons.LessonName, Teachers.Name," +
                            " Teachers.Surname FROM (SELECT * FROM Schedule WHERE Date>= @from and Date<= @to AND " +
                            "ClassID IN (SELECT ClassID FROM Teachers WHERE TeacherID = @teacherID)) t LEFT OUTER JOIN" +
                            " Lessons ON Lessons.LessonID = t.LessonID  LEFT OUTER JOIN Teachers ON" +
                            " Teachers.TeacherID = t.TeacherID  ORDER BY Date, [Number of Lesson]";
                    }
                    using (SqlCommand comm = new SqlCommand(sql, conn))
                    {
                        comm.Add("@from", from.ToShortDateString());
                        comm.Add("@to", to.ToShortDateString());
                        comm.Add("teacherID", teacherID);

                        using (SqlDataReader rd = comm.ExecuteReader())
                        {
                            List<Schedule> list = new List<Schedule>();

                            while (rd.Read())
                            {
                                Schedule s = new Schedule();
                                s.Date = rd.GetDateTime(0);
                                s.NumberOfLesson = rd.GetInt32(1);
                                s.TeachersComment = rd.CheckEmptyStringFromDB(2);
                                s.Lesson = new Lesson
                                {
                                    LessonName = rd.GetString(3)
                                };
                                if (forTeacher)
                                {
                                    s.Class = new Class
                                    {
                                        ClassName = rd.GetString(4)
                                    };
                                    s.Teacher = new Teacher
                                    {
                                        Name = rd.GetString(5),
                                        Surname = rd.GetString(6)
                                    };

                                }
                                else
                                {
                                    s.Teacher = new Teacher
                                    {
                                        Name = rd.GetString(4),
                                        Surname = rd.GetString(5)
                                    };

                                }


                                list.Add(s);
                            }
                            return list;
                        }
                    }
                }
            }
        }
    }

    static class SqlCommandExtensions
    {
        /// <summary>
        /// Add values in sql parameters collection
        /// </summary>
        /// <param name="cmd">Sql command</param>
        /// <param name="param">name of parameter</param>
        /// <param name="value">value of parameter</param>
        public static void Add(this SqlCommand cmd, string param, object value)
        {
            cmd.Parameters.AddWithValue(param, value == null ? DBNull.Value : value);
        }

        /// <summary>
        /// Check if int value from client side is correct
        /// </summary>
        /// <param name="value"></param>
        /// <param name="text"></param>
        public static void CheckValue(this int value, string text)
        {
            if (value <= 0)
                throw new ArgumentNullException("The "+text+" is not valid", nameof(value));
        }

        /// <summary>
        /// Return empty string if varchar value from data base is null
        /// </summary>
        /// <param name="rd"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        public static string CheckEmptyStringFromDB(this SqlDataReader rd, int num)
        {
            if (!rd.IsDBNull(num))
                return rd.GetString(num);
            return string.Empty;
        }

        /// <summary>
        /// check if DB connection object is not null
        /// </summary>
        /// <param name="conn"></param>
        public static void CheckDBConnection(this SqlConnection conn)
        {
            if (conn==null)
                throw new Exception("Connection is null");

        }
    }
}




