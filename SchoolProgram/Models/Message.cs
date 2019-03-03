using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolProgram.Models
{
    public class Message
    {
        public DateTime Date { get; set; }
        public Teacher Teacher { get; set; }
        public int PupilID { get; set; }
        public int UserID { get; set; }
        public string MessageText { get; set; }

    }
}
