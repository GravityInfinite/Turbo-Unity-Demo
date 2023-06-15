using System;
using GravitySDK.PC.Utils;
using UnityEngine;

namespace GravitySDK.PC.Time
{
    public class GravitySDKCalibratedTime : GravitySDKTimeInter
    {
        private GravitySDKTimeCalibration mCalibratedTime;
        private long mSystemElapsedRealtime;
        private TimeZoneInfo mTimeZone;
        private DateTime mDate;

        public GravitySDKCalibratedTime(GravitySDKTimeCalibration calibrateTimeInter, TimeZoneInfo timeZoneInfo)
        {
            this.mCalibratedTime = calibrateTimeInter;
            this.mTimeZone = timeZoneInfo;
            this.mDate = mCalibratedTime.NowDate(); // 这个时间，是已经校准过的时间，事件发生的时间
            GravitySDKLogger.Print("CurrentDate = " + this.mDate.ToString("UTC yyyy-MM-dd HH:mm:ss.fff"));
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

        public string GetTimeWithFormat(TimeZoneInfo timeZone, string format)
        {
            if (timeZone == null)
            {
                return GravitySDKUtil.FormatDateTimeWithFormat(mDate, mTimeZone, format);
            }
            else
            {
                return GravitySDKUtil.FormatDateTimeWithFormat(mDate, timeZone, format);
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