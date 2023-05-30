using System;
using System.Collections.Generic;
using GravitySDK.PC.Constant;
using GravitySDK.PC.Time;
using GravitySDK.PC.Utils;
using UnityEngine;

namespace GravitySDK.PC.DataModel
{
    public class GravitySDKUserData:GravitySDKBaseData
    {
        
        private TimeZoneInfo mTimeZone;
        public void SetTimeZone(TimeZoneInfo timeZone)
        {
            this.mTimeZone = timeZone;
        }
        public GravitySDKUserData(GravitySDKTimeInter time,string eventName, Dictionary<string,object> properties)
        {
            this.SetTime(time);
            this.SetBaseData(eventName);
            this.SetProperties(properties);
        }
        override public Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data[GravitySDKConstant.TYPE] = "profile";
            // data[GravitySDKConstant.TIME] = GravitySDKUtil.GetTimeStamp();
            data[GravitySDKConstant.TIME] = this.EventTime().GetTimeLong(this.mTimeZone);// 使用默认的time zone
            Debug.Log("diff time " + data[GravitySDKConstant.TIME] + " " + GravitySDKUtil.GetTimeStamp());
            data[GravitySDKConstant.DISTINCT_ID] = DistinctID();
            if (!string.IsNullOrEmpty(this.EventName()))
            {
                data[GravitySDKConstant.EVENT_NAME] = this.EventName();
            }
            if (!string.IsNullOrEmpty(AccountID()))
            {
                data[GravitySDKConstant.ACCOUNT_ID] = AccountID();
            }
            data[GravitySDKConstant.UUID] = UUID();
            data[GravitySDKConstant.PROPERTIES] = Properties();
            return data;
        }
    }
}
