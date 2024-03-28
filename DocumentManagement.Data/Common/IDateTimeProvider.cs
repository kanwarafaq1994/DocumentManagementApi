using System;

namespace DocumentManagement.Data.Common
{
    public interface IDateTimeProvider
    {
        DateTime Now { get; }
    }
}
