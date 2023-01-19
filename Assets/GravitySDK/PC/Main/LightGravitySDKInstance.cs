using GravitySDK.PC.Config;
using System.Collections.Generic;
using GravitySDK.PC.Constant;
using GravitySDK.PC.Utils;
using UnityEngine;

namespace GravitySDK.PC.Main
{
    public class LightGravitySDKInstance : GravitySDKInstance
    {
        public LightGravitySDKInstance(string appId, string server, GravitySDKConfig config, MonoBehaviour mono = null) : base(appId, server, null, config, mono)
        {
        }
        public override void Identifiy(string distinctID)
        {
            if (IsPaused())
            {
                return;
            }
            if (!string.IsNullOrEmpty(distinctID))
            {
                this.mDistinctID = distinctID;
            }
        }
        public override string DistinctId()
        {
            if (string.IsNullOrEmpty(this.mDistinctID))
            {
                this.mDistinctID = GravitySDKUtil.RandomID(false);
            }
            return this.mDistinctID;
        }
        public override void Login(string accountID)
        {
            if (IsPaused())
            {
                return;
            }
            if (!string.IsNullOrEmpty(accountID))
            {
                this.mAccountID = accountID;
            }
        }
        public override string AccountID()
        {
            return this.mAccountID;
        }
        public override void Logout()
        {
            if (IsPaused())
            {
                return;
            }
            this.mAccountID = "";
        }
        public override void SetSuperProperties(Dictionary<string, object> superProperties)
        {
            if (IsPaused())
            {
                return;
            }
            GravitySDKUtil.AddDictionary(this.mSupperProperties, superProperties);
        }
        public override void UnsetSuperProperty(string propertyKey)
        {
            if (IsPaused())
            {
                return;
            }
            if (this.mSupperProperties.ContainsKey(propertyKey))
            {
                this.mSupperProperties.Remove(propertyKey);
            }
        }
        public override Dictionary<string, object> SuperProperties()
        {
            return this.mSupperProperties;
        }
        public override void ClearSuperProperties()
        {
            if (IsPaused())
            {
                return;
            }
            this.mSupperProperties.Clear();
        }
        public override void EnableAutoTrack(AUTO_TRACK_EVENTS events, Dictionary<string, object> properties)
        {
        }
        public override void SetAutoTrackProperties(AUTO_TRACK_EVENTS events, Dictionary<string, object> properties)
        {
        }
        public override void Flush()
        {    
        }
    }

}