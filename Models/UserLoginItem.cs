using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASPNETAOP_WebServer.Models
{
    public class UserLoginItem
    {
        public long Id { get; set; }
        public int UserID { get; set; }
        public string Username { get; set; }
        public string Usermail { get; set; }
        public string Userpassword { get; set; }
        public int UserRole { get; set; }

        public DateTime LoginDate { get; set; }

        //0 - Request has been send
        //4 - User register
        public int isUserLoggedIn { get; set; }
    }
}
