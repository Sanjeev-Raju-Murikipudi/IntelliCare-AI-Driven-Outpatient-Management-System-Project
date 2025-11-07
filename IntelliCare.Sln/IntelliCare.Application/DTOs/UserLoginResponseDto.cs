using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliCare.Application.DTOs
{
    public class UserLoginResponseDto
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public int RoleName { get; set; }
        public string MobileNumber { get; set; }
        public string ContactEmail { get; set; }
    }
}
