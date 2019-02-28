using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolProgram.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string EMail { get; set; }
        public DateTime LastLogin { get; set; }
        public string NewPassword { get; set; }
       
    }

    

   
}
