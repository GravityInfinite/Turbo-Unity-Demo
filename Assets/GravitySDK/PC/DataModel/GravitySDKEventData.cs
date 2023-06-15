using System;
using System.Collections.Generic;
using GravitySDK.PC.Constant;
using GravitySDK.PC.Time;
using GravitySDK.PC.Utils;
using UnityEngine;

namespace GravitySDK.PC.DataModel
{
    public class GravitySDKEventData : GravitySDKBaseData
    {
        private DateTime mEventTime;

        private TimeZoneInfo mTimeZone;

        //事件持续时长
        private float mDuration;

        public void SetEventTime(DateTime dateTime)
        {
            this.mEventTime = dateTime;
        }

        public void SetTimeZone(TimeZoneInfo timeZone)
        {
            this.mTimeZone = timeZone;
        }

        //public DateTime EventTime()
        //{
        //    return this.mEventTime;
        //}
        public DateTime Time()
        {
            return mEventTime;
        }

        public GravitySDKEventData(string eventName) : base(eventName)
        {
        }

        public GravitySDKEventData(GravitySDKTimeInter time, string eventName) : base(time, eventName)
        {
        }

        public GravitySDKEventData(GravitySDKTimeInter time, string eventName, Dictionary<string, object> properties) :
            base(time, eventName, properties)
        {
        }

        public void SetDuration(float duration)
        {
            this.mDuration = duration;
        }

        public override Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data[GravitySDKConstant.TYPE] = "track";
            // data[GravitySDKConstant.TIME] = GravitySDKUtil.GetTimeStamp();
            data[GravitySDKConstant.TIME] = this.EventTime().GetDateTimeUtcTimestamp();
            // Debug.Log("GravityTest event " + this.mTimeZone + " " + data[GravitySDKConstant.TIME] + " " + GravitySDKUtil.GetTimeStamp() + " " +
            //           (float) ((long) data[GravitySDKConstant.TIME] - GravitySDKUtil.GetTimeStamp()) / 1000 / 60);
            data[GravitySDKConstant.DISTINCT_ID] = this.DistinctID();
            if (!string.IsNullOrEmpty(this.EventName()))
            {
                data[GravitySDKConstant.EVENT_NAME] = this.EventName();
            }

            if (!string.IsNullOrEmpty(this.AccountID()))
            {
                data[GravitySDKConstant.ACCOUNT_ID] = this.AccountID();
            }

            data[GravitySDKConstant.UUID] = this.UUID();
            Dictionary<string, object> properties = this.Properties();
            properties[GravitySDKConstant.ZONE_OFFSET] = this.EventTime().GetZoneOffset(this.mTimeZone);
            if (mDuration != 0)
            {
                properties[GravitySDKConstant.DURATION] = mDuration;
            }

            data[GravitySDKConstant.PROPERTIES] = properties;

            return data;
        }
    }
}