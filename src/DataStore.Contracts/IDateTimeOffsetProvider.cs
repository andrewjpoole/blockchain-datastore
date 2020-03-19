using System;

namespace AsyncInternals
{
    public interface IDateTimeOffsetProvider
    {
        DateTimeOffset GetDateTimeOffset();
    }

    public class DateTimeOffsetProvider : IDateTimeOffsetProvider
    {
        public DateTimeOffset GetDateTimeOffset()
        {
            return DateTimeOffset.UtcNow;
        }
    }
}