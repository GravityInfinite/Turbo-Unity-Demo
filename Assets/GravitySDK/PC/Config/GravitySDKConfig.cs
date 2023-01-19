﻿using System;
using System.Collections.Generic;
using GravitySDK.PC.Utils;
using GravitySDK.PC.Request;
using GravitySDK.PC.Constant;
using UnityEngine;
using System.Collections;
using GravitySDK.PC.GravityTurbo;

namespace GravitySDK.PC.Config
{
    public enum Mode
    {
        /* 正常模式，数据会存入缓存，并依据一定的缓存策略上报 */
        NORMAL,
        /* Debug 模式，数据逐条上报。当出现问题时会以日志和异常的方式提示用户 */
        DEBUG,
        /* Debug Only 模式，只对数据做校验，不会入库 */
        DEBUG_ONLY
    }
    public class GravitySDKConfig
    {
        private string mToken;
        private string mServerUrl;
        private string mNormalUrl;
        private string mDebugUrl;
        private string mConfigUrl;
        private string mInstanceName;
        private Mode mMode = Mode.NORMAL;
        private TimeZoneInfo mTimeZone;
        public int mUploadInterval = 30; // 默认30s刷新一次
        public int mUploadSize = 30;
        private List<string> mDisableEvents = new List<string>();
        private static Dictionary<string, GravitySDKConfig> sInstances = new Dictionary<string, GravitySDKConfig>();
        private GravitySDKConfig(string token,string serverUrl, string instanceName)
        {
            //校验 server url
            serverUrl = this.VerifyUrl(serverUrl);
            this.mServerUrl = serverUrl;
            this.mNormalUrl = serverUrl + "/event_center/api/v1/eventv2/collect/?access_token=" +
                              Turbo.GetAccessToken();
            // 这两个接口都没有启用
            this.mDebugUrl = serverUrl + "/data_debug";
            this.mConfigUrl = serverUrl + "/config";
            this.mToken = token;
            this.mInstanceName = instanceName;
            try
            {
                this.mTimeZone = TimeZoneInfo.Local;
            }
            catch (Exception e)
            {
                //GravitySDKLogger.Print("TimeZoneInfo initial failed :" + e.Message);
            }
        }
        private string VerifyUrl(string serverUrl)
        {
            Uri uri = new Uri(serverUrl);
            serverUrl = uri.Scheme + "://" + uri.Host + ":" + uri.Port;
            return serverUrl;
        }
        public void SetMode(Mode mode)
        {
            this.mMode = mode;
        }
        public Mode GetMode()
        {
            return this.mMode;
        }
        public string DebugURL()
        {
            return this.mDebugUrl;
        }
        public string NormalURL()
        {
            return this.mNormalUrl;
        }
        public string ConfigURL()
        {
            return this.mConfigUrl;
        }
        public string Server()
        {
            return this.mServerUrl;
        }
        public string InstanceName()
        {
            return this.mInstanceName;
        }
        public static GravitySDKConfig GetInstance(string token, string server, string instanceName)
        {
            GravitySDKConfig config = null;
            if (!string.IsNullOrEmpty(instanceName))
            {
                if (sInstances.ContainsKey(instanceName))
                {
                    config = sInstances[instanceName];
                }
                else
                {
                    config = new GravitySDKConfig(token, server, instanceName);
                    sInstances.Add(instanceName, config);
                }
            }
            else
            {
                if (sInstances.ContainsKey(token))
                {
                    config = sInstances[token];
                }
                else
                {
                    config = new GravitySDKConfig(token, server, null);
                    sInstances.Add(token, config);
                }
            }
            return config;
        }
        public void SetTimeZone(TimeZoneInfo timeZoneInfo)
        {
            this.mTimeZone = timeZoneInfo;
        }
        public TimeZoneInfo TimeZone()
        {
            return this.mTimeZone;
        }
        public List<string> DisableEvents() {
            return this.mDisableEvents;
        }
        public bool IsDisabledEvent(string eventName) 
        {
            if (this.mDisableEvents == null)
            {
                return false;
            } 
            else 
            {
                return this.mDisableEvents.Contains(eventName);
            }
        }
        public void UpdateConfig(MonoBehaviour mono, ResponseHandle callback = null)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            ResponseHandle responseHandle = delegate (Dictionary<string, object> result) {
                try
                {
                    int code = int.Parse(result["code"].ToString());
                    if (result!=null && code==0)
                    {
                        Dictionary<string, object> data = (Dictionary<string, object>)result["data"];
                        foreach(KeyValuePair<string, object> kv in data) 
                        {
                            if (kv.Key == "sync_interval")
                            {
                                this.mUploadInterval = int.Parse(kv.Value.ToString());
                            } 
                            else if (kv.Key == "sync_batch_size")
                            {
                                this.mUploadSize = int.Parse(kv.Value.ToString());
                            }
                            else if (kv.Key == "disable_event_list")
                            {
                                foreach (var item in (List<object>)kv.Value)
                                {
                                    this.mDisableEvents.Add((string)item);
                                }
                            } 
                        }
                    }
                }
                catch (Exception ex)
                {
                    GravitySDKLogger.Print("Get config failed: " + ex.Message);
                }
                if (callback != null)
                {
                    callback();
                }
            };
            mono.StartCoroutine(this.GetWithFORM(this.mConfigUrl,this.mToken,null,responseHandle));
        }

        private IEnumerator GetWithFORM (string url, string appId, Dictionary<string, object> param, ResponseHandle responseHandle) {
            yield return GravitySDKBaseRequest.GetWithFORM_2(this.mConfigUrl,this.mToken,param,responseHandle);
        }
    }
}
