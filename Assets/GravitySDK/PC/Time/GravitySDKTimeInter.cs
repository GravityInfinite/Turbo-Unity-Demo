using System;
namespace GravitySDK.PC.Time
{
    public interface GravitySDKTimeInter
    {
        string GetTime(TimeZoneInfo timeZone);
        long GetTimeLong(TimeZoneInfo timeZone);
        Double GetZoneOffset(TimeZoneInfo timeZone);
    }
}
