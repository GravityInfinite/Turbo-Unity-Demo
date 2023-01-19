using System;
using GravitySDK.PC.Time;
using GravitySDK.PC.Utils;

namespace GravitySDK.PC.Time
{
    public class GravitySDKTime : GravitySDKTimeInter
    {
        private TimeZoneInfo mTimeZone;
        private DateTime mDate;

        public GravitySDKTime(TimeZoneInfo timezone, DateTime date)
        {
            this.mTimeZone = timezone;
            this.mDate = date;
        }

        public string GetTime(TimeZoneInfo timeZone)
        {
            if (timeZone == null)
            {
                return GravitySDKUtil.FormatDateTime(mDate, mTimeZone);
            }
            else
            {
                return GravitySDKUtil.FormatDateTime(mDate, timeZone);
            }
        }
        
        public long GetTimeLong(TimeZoneInfo timeZone)
        {
            if (timeZone == null)
            {
                return GravitySDKUtil.FormatDateTimeToLong(mDate, mTimeZone);
            }
            else
            {
                return GravitySDKUtil.FormatDateTimeToLong(mDate, timeZone);
            }
        }

        public double GetZoneOffset(TimeZoneInfo timeZone)
        {
            if (timeZone == null)
            {
                return GravitySDKUtil.ZoneOffset(mDate, mTimeZone);
            }
            else
            {
                return GravitySDKUtil.ZoneOffset(mDate, timeZone);
            }
        }
    }

}
