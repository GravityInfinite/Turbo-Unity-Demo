using System;
using System.Collections.Generic;
using GravitySDK.PC.Time;

namespace GravitySDK.PC.DataModel
{
    public abstract class GravitySDKBaseData
    {
        //事件发生时间
        private GravitySDKTimeInter mTime;
        //访客ID
        private string mDistinctID;
        //事件名称
        private string mEventName;
        //账号ID
        private string mAccountID;
      
        //该条数据唯一标识
        private string mUUID;
        private Dictionary<string, object> mProperties = new  Dictionary<string, object>();
        public Dictionary<string, object> Properties()
        {
            return mProperties;
        }
        public void SetEventName(string eventName)
        {
            this.mEventName = eventName;
        }
        public string EventName()
        {
            return this.mEventName;
        }
        public void SetTime(GravitySDKTimeInter time)
        {
            this.mTime = time;
        }
        public GravitySDKTimeInter EventTime()
        {
            return this.mTime;
        }
        public string AccountID()
        {
            return this.mAccountID;
        }
        public string DistinctID()
        {
            return this.mDistinctID;
        }
        public void SetAccountID(string accuntID)
        {
            this.mAccountID = accuntID;
        }
        public void SetDistinctID(string distinctID)
        {
            this.mDistinctID = distinctID;
        }
        public string UUID()
        {
            return this.mUUID;
        }
        public GravitySDKBaseData() { }
        public GravitySDKBaseData(GravitySDKTimeInter time,string eventName)
        {
            this.SetBaseData(eventName);
            this.SetTime(time);
        }
        public GravitySDKBaseData(string eventName)
        {
            this.SetBaseData(eventName);
        }
        public void SetBaseData(string eventName)
        {
            this.mEventName = eventName;
            this.mUUID = System.Guid.NewGuid().ToString();
        }
        
        public GravitySDKBaseData(GravitySDKTimeInter time, string eventName, Dictionary<string, object> properties):this(time,eventName)
        {
            if (properties != null)
            {
                this.SetProperties(properties);
            }
        }

        abstract public Dictionary<string, object> ToDictionary();
        public void SetProperties(Dictionary<string, object> properties,bool isOverwrite = true)
        {
            if (isOverwrite)
            {
                foreach (KeyValuePair<string, object> kv in properties)
                {
                    mProperties[kv.Key] = kv.Value;

                }
            }
            else
            {
                foreach (KeyValuePair<string, object> kv in properties)
                {
                    if (!mProperties.ContainsKey(kv.Key))
                    {
                        mProperties[kv.Key] = kv.Value;
                    } 

                }
            }
            
        }
       
    }
}
