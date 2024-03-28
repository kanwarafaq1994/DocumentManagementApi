using System.Collections.Generic;
using System.Text.Json;

namespace DocumentManagement.Data.Common
{
    public class InfoDto
    {

        public List<string> Message { get; set; } = new List<string>();

        public InfoDto()
        {
            this.Message = new List<string>();
        }

        public InfoDto(string message)
        {
            this.Message.Add(message);
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(Message);
        }
    }
}
