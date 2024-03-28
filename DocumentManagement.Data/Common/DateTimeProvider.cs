using System;
using System.Collections.Generic;
using System.Text;

namespace DocumentManagement.Data.Common
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime Now => DateTime.Now.Truncate(TimeSpan.FromSeconds(1));
    }
}
