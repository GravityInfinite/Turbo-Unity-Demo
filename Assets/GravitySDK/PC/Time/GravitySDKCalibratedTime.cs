using System;
using GravitySDK.PC.Utils;

namespace GravitySDK.PC.Time
{
    public class GravitySDKCalibratedTime : GravitySDKTimeInter
    {
        private GravitySDKTimeCalibration mCalibratedTime;
        private long mSystemElapsedRealtime;
        private TimeZoneInfo mTimeZone;
        private DateTime mDate;
        public GravitySDKCalibratedTime(GravitySDKTimeCalibration calibrateTimeInter,TimeZoneInfo timeZoneInfo)
        {
            this.mCalibratedTime = calibrateTimeInter;
            this.mTimeZone = timeZoneInfo;
            this.mDate = mCalibratedTime.NowDate();
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
