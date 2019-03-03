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
    {
        public static string CONN_STRING;

        public static UserLogAndPas Login(string login, string password)
        {
            using (SqlConnection conn = new SqlConnection(CONN_STRING))
            {
                conn.Open();

                return ValidateUser(login, password, conn);
            }
        }
        public static UserLogAndPas ValidateUser(string login, string password, SqlConnection conn)
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
                    {
                        return null;
                    }
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
        {
            internal static List<User> GetUsers()
            {
                List<User> users = new List<User>();
                using (SqlConnection conn = new SqlConnection(CONN_STRING))
                {
                    if (conn == null)
                        throw new Exception("Connection is null");
                    conn.Open();
                    string sql = "SELECT UserID, Email, LastLogin FROM Users";
                    using (SqlCommand command = new SqlCommand(sql, conn))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                User user = new User
                                {
                                    UserID = reader.GetInt32(0),
                                    EMail = reader["EMail"].ToString(),
                                    LastLogin = reader.GetDateTime(reader.GetOrdinal("LastLogin"))
                                };
                                users.Add(user);
                            }
                        }
                    }

                }
                return users;
            }

            internal static bool InsertUser(User user)
            {

                using (SqlConnection conn = new SqlConnection(CONN_STRING))
                {
                    if (conn == null)
                        throw new Exception("Connection is null");
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
            internal static List<Attendance> GetPupilAttendanceForUser(int userID, DateTime date)
            {
                using (SqlConnection conn = new SqlConnection(CONN_STRING))
                {
                    if (conn == null)
                        throw new Exception("Connection is null");
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

            internal static List<Message> GetMessages(int userID)
            {
                using (SqlConnection conn = new SqlConnection(CONN_STRING))
                {
                    if (conn == null)
                        throw new Exception("Connection is null");
                    conn.Open();
                    string sql = "SELECT Messages.Date, Teachers.Name, Teachers.Surname, Messages.Message " +
                        "FROM Messages INNER JOIN Teachers ON Messages.UserID = 6 AND Teachers.TeacherID IN " +
                        "(SELECT TeacherID FROM Messages WHERE UserID = @userID)";
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

            internal static  List<Schedule> GetSchedule(int userID, DateTime from, DateTime to)
            {
                using (SqlConnection conn = new SqlConnection(CONN_STRING))
                {
                    if (conn == null)
                        throw new Exception("Connection is null");
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
        {
            public static int GetTeacherID(int userID)
            {
                using (SqlConnection conn = new SqlConnection(CONN_STRING))
                {
                    if (conn == null)
                        throw new Exception("Connection is null");
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
            public static List<Class> GetClasses()
            {
                List<Class> classes = new List<Class>();
                using (SqlConnection conn = new SqlConnection(CONN_STRING))
                {
                    if (conn == null)
                        throw new Exception("Connection is null");
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
            public static List<Pupil> GetPupils(int classId, DateTime selectedDate)
            {
                using (SqlConnection conn = new SqlConnection(CONN_STRING))
                {
                    if (conn == null)
                        throw new Exception("Connection is null");
                    conn.Open();
                    List<Pupil> pupils = new List<Pupil>();
                    string sql = "SELECT PupilID, Name, Surname, UserID FROM Pupils WHERE ClassID=@classId";
                    using (SqlCommand comm = new SqlCommand(sql, conn))
                    {
                        comm.Add("@classId", classId);
                        using (SqlDataReader reader = comm.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Pupil p = new Pupil
                                {
                                    PupilID = reader.GetInt32(0),
                                    Name = reader.GetString(1),
                                    Surname = reader.GetString(2),
                                    UserID = reader.GetInt32(3)
                                };
                                pupils.Add(p);
                            }
                            return pupils;
                        }
                    }
                }
            }
            public static bool AttendanceHasData(DateTime date, int classId, int lessonId, int numberOfLesson, SqlConnection conn)
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


            public static List<Lesson> GetLessons(DateTime date, int classId)
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
            public static AttendanceWithTeacherComment GetPupilsListFromAttendance(DateTime date, int classId, int lessonId, int numberOfLesson)
            {
                using (SqlConnection conn = new SqlConnection(CONN_STRING))
                {
                    if (conn == null)
                        throw new Exception("Connection is null");
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
            public static List<Pupil> GetPupilsToAttendance(int classID, SqlConnection conn)
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
            public static bool InsertDataToAttendanceTable(AttendanceWithTeacherComment list)
            {
                using (SqlConnection conn = new SqlConnection(CONN_STRING))
                {
                    if (conn == null)
                        throw new Exception("Connection is null");
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
            public static Teacher GetNameOfTeacherById(int id)
            {
                using (SqlConnection conn = new SqlConnection(CONN_STRING))
                {
                    if (conn == null)
                        throw new Exception("Connection is null");
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

            public static List<Pupil> GetPupilsFromMyClass(int teacherId)
            {
                using (SqlConnection conn = new SqlConnection(CONN_STRING))
                {
                    if (conn == null)
                        throw new Exception("Connection is null");
                    conn.Open();
                    string sql = "SELECT PupilID, Pupils.Name, Surname, DateOfBirth, PhoneNumber,Address,UserID," +
                        "Classes.ClassName FROM Pupils, Classes Where Pupils.TeacherID = @teacherId AND " +
                        "Classes.TeacherID = @teacherId";

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

            public static bool SendMessageToServer(Message m)
            {
                using (SqlConnection conn = new SqlConnection(CONN_STRING))
                {
                    if (conn == null)
                        throw new Exception("Connection is null");
                    conn.Open();
                    string sql = "INSERT INTO Messages (Date, TeacherID, PupilID, UserID, Message) VALUES " +
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
                        "Teachers.TeacherID=@teacherID ORDER BY Date";
                    if (!forTeacher)
                    {
                        sql = "SELECT Date, [Number Of Lesson], TeachersComment, Lessons.LessonName, Teachers.Name," +
                            " Teachers.Surname FROM (SELECT * FROM Schedule WHERE Date>= @from and Date<= @to AND " +
                            "ClassID IN (SELECT ClassID FROM Teachers WHERE TeacherID = @teacherID)) t LEFT OUTER JOIN" +
                            " Lessons ON Lessons.LessonID = t.LessonID  LEFT OUTER JOIN Teachers ON" +
                            " Teachers.TeacherID = t.TeacherID  ORDER BY Date";
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
        public static void Add(this SqlCommand cmd, string param, object value)
        {
            cmd.Parameters.AddWithValue(param, value == null ? DBNull.Value : value);
        }

        public static string CheckEmptyStringFromDB(this SqlDataReader rd, int num)
        {
            if (!rd.IsDBNull(num))
                return rd.GetString(num);
            return string.Empty;
        }
    }
}




