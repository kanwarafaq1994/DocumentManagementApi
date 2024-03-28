using System;
using System.Collections.Generic;
using System.Text;

namespace DocumentManagement.Data.DTOs
{
    public class LoginResponseDto
    {

        public int UserId { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
    }
}
