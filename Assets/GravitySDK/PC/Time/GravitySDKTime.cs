using System;
using GravitySDK.PC.Time;
using GravitySDK.PC.Utils;
using UnityEngine;

namespace GravitySDK.PC.Time
{
    public class GravitySDKTime : GravitySDKTimeInter
    {
        private TimeZoneInfo mTimeZone;
        private DateTime mDate;

        public GravitySDKTime(TimeZoneInfo timezone, DateTime date)
        {
            this.mTimeZone = timezone;
            this.mDate = date; // 这个是没有经过校准的时间
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
        
        public long GetDateTimeUtcTimestamp()
        {
            return GravitySDKUtil.FormatDateTimeToUtcTimestamp(mDate);
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
