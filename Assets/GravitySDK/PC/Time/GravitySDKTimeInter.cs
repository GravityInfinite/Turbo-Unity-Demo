using System;

namespace GravitySDK.PC.Time
{
    public interface GravitySDKTimeInter
    {
        string GetTime(TimeZoneInfo timeZone);
        string GetTimeWithFormat(TimeZoneInfo timeZone, string format);
        long GetDateTimeUtcTimestamp();
        Double GetZoneOffset(TimeZoneInfo timeZone);
    }
}