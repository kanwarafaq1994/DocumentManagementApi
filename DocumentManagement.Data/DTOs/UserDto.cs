using System.Collections.Generic;

namespace DocumentManagement.Data.DTOs
{
    public class UserDto: UserBaseDto
    {
        public List<DocumentDto> Documents { get; set; }
    }

}
