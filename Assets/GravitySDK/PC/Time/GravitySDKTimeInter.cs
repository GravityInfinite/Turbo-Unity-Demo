using System;
namespace GravitySDK.PC.Time
{
    public interface GravitySDKTimeInter
    {
        string GetTime(TimeZoneInfo timeZone);
        long GetDateTimeUtcTimestamp();
        Double GetZoneOffset(TimeZoneInfo timeZone);
    }
}
