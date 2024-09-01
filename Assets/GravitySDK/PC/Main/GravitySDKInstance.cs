using System;
using System.Collections;
using System.Collections.Generic;
using GravityEngine;
using GravitySDK.PC.AutoTrack;
using GravitySDK.PC.Config;
using GravitySDK.PC.Constant;
using GravitySDK.PC.DataModel;
using GravitySDK.PC.GravityTurbo;
using GravitySDK.PC.Request;
using GravitySDK.PC.Storage;
using GravitySDK.PC.TaskManager;
using GravitySDK.PC.Time;
using GravitySDK.PC.Utils;
using UnityEngine;

namespace GravitySDK.PC.Main
{
    public interface IDynamicSuperProperties_PC
    {
        Dictionary<string, object> GetDynamicSuperProperties_PC();
    }

    public interface IAutoTrackEventCallback_PC
    {
        Dictionary<string, object> AutoTrackEventCallback_PC(int type, Dictionary<string, object> properties);
    }

    public class GravitySDKInstance
    {
        private string mServer;
        protected string mDistinctID;
        protected string mAccountID;
        private bool mOptTracking = true;
        private Dictionary<string, object> mTimeEvents = new Dictionary<string, object>();
        private Dictionary<string, object> mTimeEventsBefore = new Dictionary<string, object>();
        private bool mEnableTracking = true;
        private bool mEventSaveOnly = false; //事件数据仅保存，不上报
        protected Dictionary<string, object> mSupperProperties = new Dictionary<string, object>();

        protected Dictionary<string, Dictionary<string, object>> mAutoTrackProperties =
            new Dictionary<string, Dictionary<string, object>>();

        private GravitySDKConfig mConfig;
        private GravitySDKBaseRequest mRequest;
        private GravitySDKTimeCalibration mTimeCalibration;
        private IDynamicSuperProperties_PC mDynamicProperties;

        private GravitySDKTask mTask
        {
            get { return GravitySDKTask.SingleTask(); }
            set { this.mTask = value; }
        }

        private static GravitySDKInstance mCurrentInstance;
        private MonoBehaviour mMono;
        private static MonoBehaviour sMono;
        private GravitySDKAutoTrack mAutoTrack;
#if GRAVITY_WECHAT_GAME_MODE || GRAVITY_BYTEDANCE_GAME_MODE || GRAVITY_KUAISHOU_GAME_MODE || GRAVITY_OPPO_GAME_MODE
        private WeChatGameAutoTrack mWechatGameAutoTrack;
#endif

        ResponseHandle mResponseHandle;

        public void SetTimeCalibratieton(GravitySDKTimeCalibration timeCalibration)
        {
            this.mTimeCalibration = timeCalibration;
        }

        private GravitySDKInstance()
        {
        }

        private void DefaultData()
        {
            DistinctId();
            AccountID();
            SuperProperties();
            DefaultTrackState();
        }

        public GravitySDKInstance(string server, string instanceName, GravitySDKConfig config,
            MonoBehaviour mono = null)
        {
            this.mMono = mono;
            sMono = mono;
            mResponseHandle = delegate(Dictionary<string, object> result) { mTask.Release(); };
            if (config == null)
            {
                this.mConfig = GravitySDKConfig.GetInstance(server, instanceName);
            }
            else
            {
                this.mConfig = config;
            }

            // 更新线上配置到客户端，暂时不需要
            // this.mConfig.UpdateConfig(mono, delegate (Dictionary<string, object> result) {
            //     if (this.mConfig.GetMode() == Mode.NORMAL)
            //     {
            //         sMono.StartCoroutine(WaitAndFlush());
            //     }
            // });
            // 启动之后，默认走一遍flush刷数据
            if (this.mConfig.GetMode() == Mode.NORMAL)
            {
                sMono.StartCoroutine(WaitAndFlush());
            }

            this.mServer = server;
            if (this.mConfig.GetMode() == Mode.NORMAL)
            {
                this.mRequest = new GravitySDKNormalRequest(this.mConfig.NormalURL());
            }
            else
            {
                this.mRequest = new GravitySDKDebugRequest(this.mConfig.NormalURL());
            }

            DefaultData();
            mCurrentInstance = this;
            // 动态加载 GravitySDKTask GravitySDKAutoTrack WeChatGameAutoTrack
            GameObject mGravitySDKTask = new GameObject("GravitySDKTask", typeof(GravitySDKTask));
            UnityEngine.Object.DontDestroyOnLoad(mGravitySDKTask);

            // 挂在采集器，开启App的自动采集
            GameObject mGravitySDKAutoTrack = new GameObject("GravitySDKAutoTrack", typeof(GravitySDKAutoTrack));
            this.mAutoTrack = (GravitySDKAutoTrack) mGravitySDKAutoTrack.GetComponent(typeof(GravitySDKAutoTrack));
            UnityEngine.Object.DontDestroyOnLoad(mGravitySDKAutoTrack);

#if GRAVITY_WECHAT_GAME_MODE || GRAVITY_BYTEDANCE_GAME_MODE || GRAVITY_KUAISHOU_GAME_MODE || GRAVITY_OPPO_GAME_MODE
            // 挂载采集器，以开启微信、抖音、快手小游戏、OPPO快游戏的自动采集
            GameObject mWechatGameAutoTrackObj = new GameObject("WechatGameAutoTrack", typeof(WeChatGameAutoTrack));
            mWechatGameAutoTrack =
                (WeChatGameAutoTrack) mWechatGameAutoTrackObj.GetComponent(typeof(WeChatGameAutoTrack));
            UnityEngine.Object.DontDestroyOnLoad(mWechatGameAutoTrackObj);
#endif
        }

        public GravitySDKTimeInter GetTime(DateTime dateTime)
        {
            GravitySDKTimeInter time = null;

            if (dateTime == DateTime.MinValue || dateTime == null)
            {
                if (mTimeCalibration == null) //判断是否有时间校准
                {
                    time = new GravitySDKTime(mConfig.TimeZone(), DateTime.Now);
                }
                else
                {
                    time = new GravitySDKCalibratedTime(mTimeCalibration, mConfig.TimeZone());
                }
            }
            else
            {
                time = new GravitySDKTime(mConfig.TimeZone(), dateTime);
            }

            return time;
        }

        //设置访客ID
        public virtual void Identifiy(string distinctID)
        {
            if (IsPaused())
            {
                return;
            }

            if (!string.IsNullOrEmpty(distinctID))
            {
                this.mDistinctID = distinctID;
                GravitySDKFile.SaveData(GravitySDKConstant.DISTINCT_ID, distinctID);
            }
        }

        public virtual string DistinctId()
        {
            this.mDistinctID =
                (string) GravitySDKFile.GetData(GravitySDKConstant.DISTINCT_ID, typeof(string));
            if (string.IsNullOrEmpty(this.mDistinctID))
            {
                this.mDistinctID = GravitySDKUtil.RandomID();
                GravitySDKFile.SaveData(GravitySDKConstant.DISTINCT_ID, this.mDistinctID);
            }

            return this.mDistinctID;
        }

        public virtual void Login(string accountID)
        {
            if (IsPaused())
            {
                return;
            }

            if (!string.IsNullOrEmpty(accountID))
            {
                this.mAccountID = accountID;
                GravitySDKFile.SaveData(GravitySDKConstant.ACCOUNT_ID, accountID);
                Turbo.SetClientId(accountID);
                
                Track("$MPLogin");
            }
        }

        public virtual string AccountID()
        {
            this.mAccountID =
                (string) GravitySDKFile.GetData(GravitySDKConstant.ACCOUNT_ID, typeof(string));
            return this.mAccountID;
        }

        public virtual void Logout(ILogoutCallback logoutCallback)
        {
            if (IsPaused())
            {
                return;
            }
            
            Track("$MPLogout", null, DateTime.MinValue, null, true);

            this.mAccountID = "";
            GravitySDKFile.DeleteData(GravitySDKConstant.ACCOUNT_ID);
            Turbo.SetClientId("");

            if (logoutCallback!=null)
            {
                logoutCallback.onCompleted();
            }
        }

        //TODO
        public virtual void EnableAutoTrack(AUTO_TRACK_EVENTS events, Dictionary<string, object> properties)
        {
            this.mAutoTrack.EnableAutoTrack(events, properties);
#if GRAVITY_WECHAT_GAME_MODE || GRAVITY_BYTEDANCE_GAME_MODE || GRAVITY_KUAISHOU_GAME_MODE || GRAVITY_OPPO_GAME_MODE
            mWechatGameAutoTrack.EnableAutoTrack(events, properties);
#endif
        }

        public virtual void EnableAutoTrack(AUTO_TRACK_EVENTS events, IAutoTrackEventCallback_PC eventCallback)
        {
            this.mAutoTrack.EnableAutoTrack(events, eventCallback);
#if GRAVITY_WECHAT_GAME_MODE || GRAVITY_BYTEDANCE_GAME_MODE || GRAVITY_KUAISHOU_GAME_MODE || GRAVITY_OPPO_GAME_MODE
            mWechatGameAutoTrack.EnableAutoTrack(events, null);
#endif
        }

        // 设置自动采集事件的自定义属性
        public virtual void SetAutoTrackProperties(AUTO_TRACK_EVENTS events, Dictionary<string, object> properties)
        {
            this.mAutoTrack.SetAutoTrackProperties(events, properties);
        }

        public void Track(string eventName)
        {
            Track(eventName, null, DateTime.MinValue);
        }

        public void Track(string eventName, Dictionary<string, object> properties)
        {
            Track(eventName, properties, DateTime.MinValue);
        }

        public void Track(string eventName, Dictionary<string, object> properties, DateTime date)
        {
            Track(eventName, properties, date, null, false);
        }

        public void Track(string eventName, Dictionary<string, object> properties, DateTime date, TimeZoneInfo timeZone)
        {
            Track(eventName, properties, date, timeZone, false);
        }

        public void Track(string eventName, Dictionary<string, object> properties, DateTime date, TimeZoneInfo timeZone,
            bool immediately)
        {
            GravitySDKTimeInter time = GetTime(date);
            GravitySDKEventData data = new GravitySDKEventData(time, eventName, properties);
            if (timeZone != null)
            {
                data.SetTimeZone(timeZone);
            }

            SendData(data, immediately);
        }

        private void SendData(GravitySDKEventData data)
        {
            SendData(data, false);
        }

        private void SendData(GravitySDKEventData data, bool immediately)
        {
            if (this.mDynamicProperties != null)
            {
                data.SetProperties(this.mDynamicProperties.GetDynamicSuperProperties_PC(), false);
            }

            if (this.mSupperProperties != null && this.mSupperProperties.Count > 0)
            {
                data.SetProperties(this.mSupperProperties, false);
            }

            // 移除设备信息中部分的禁用属性
            Dictionary<string, object> deviceInfo = GravitySDKUtil.DeviceInfo();
            foreach (string item in GravitySDKUtil.DisPresetProperties)
            {
                if (deviceInfo.ContainsKey(item))
                {
                    deviceInfo.Remove(item);
                }
            }

            data.SetProperties(deviceInfo, false);

            // 处理TimerEvent事件，自动记录事件时长
            float duration = 0;
            if (mTimeEvents.ContainsKey(data.EventName()))
            {
                int beginTime = (int) mTimeEvents[data.EventName()];
                int nowTime = Environment.TickCount;
                duration = (float) ((nowTime - beginTime) / 1000.0);
                mTimeEvents.Remove(data.EventName());
                if (mTimeEventsBefore.ContainsKey(data.EventName()))
                {
                    int beforeTime = (int) mTimeEventsBefore[data.EventName()];
                    duration = duration + (float) (beforeTime / 1000.0);
                    mTimeEventsBefore.Remove(data.EventName());
                }
            }

            if (duration != 0)
            {
                data.SetDuration(duration);
            }

            SendData((GravitySDKBaseData) data, immediately);
        }

        private void SendData(GravitySDKBaseData data)
        {
            SendData(data, false);
        }

        private void SendData(GravitySDKBaseData data, bool immediately)
        {
            if (IsPaused())
            {
                return;
            }

            if (!string.IsNullOrEmpty(this.mAccountID))
            {
                data.SetAccountID(this.mAccountID);
            }

            if (string.IsNullOrEmpty(this.mDistinctID))
            {
                DistinctId();
            }

            data.SetDistinctID(this.mDistinctID);

            if (this.mConfig.IsDisabledEvent(data.EventName()))
            {
                GravitySDKLogger.Print("disabled Event: " + data.EventName());
                return;
            }

            IList<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
            list.Add(data.ToDictionary());
            if (this.mConfig.GetMode() == Mode.NORMAL && this.mRequest.GetType() != typeof(GravitySDKNormalRequest))
            {
                this.mRequest = new GravitySDKNormalRequest(this.mConfig.NormalURL());
            }

            if (immediately)
            {
                mRequest.SendData_2(null, list);
            }
            else
            {
                Dictionary<string, object> dataDic = data.ToDictionary();
                int count = 0;
                if (!string.IsNullOrEmpty(this.mConfig.InstanceName()))
                {
                    GravitySDKLogger.Print("Save event: " + GravitySDKJSON.Serialize(dataDic));
                    count = GravitySDKFileJson.EnqueueTrackingData(dataDic, this.mConfig.InstanceName());
                }
                else
                {
                    GravitySDKLogger.Print("Save event: " + GravitySDKJSON.Serialize(dataDic));
                    count = GravitySDKFileJson.EnqueueTrackingData(dataDic);
                }

                if (this.mConfig.GetMode() != Mode.NORMAL || count >= this.mConfig.mUploadSize)
                {
                    Flush();
                }
            }
        }

        private IEnumerator WaitAndFlush()
        {
            while (true)
            {
                yield return new WaitForSeconds(mConfig.mUploadInterval);
                GravitySDKLogger.Print("Flush Data");
                Flush();
                break;
            }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        public virtual void Flush()
        {
            if (mEventSaveOnly == false)
            {
                mTask.SyncInvokeAllTask();

                int batchSize = (this.mConfig.GetMode() != Mode.NORMAL) ? 1 : mConfig.mUploadSize;
                ResponseHandle responseHandle = delegate(Dictionary<string, object> result)
                {
                    int eventCount = 0;
                    if (result != null && !(result.ContainsKey("is_response_error") &&
                                            Convert.ToBoolean(result["is_response_error"])))
                    {
                        int flushCount = 0;
                        if (result.ContainsKey("flush_count"))
                        {
                            flushCount = (int) result["flush_count"];
                        }

                        if (!string.IsNullOrEmpty(this.mConfig.InstanceName()))
                        {
                            eventCount =
                                GravitySDKFileJson.DeleteBatchTrackingData(flushCount, this.mConfig.InstanceName());
                        }
                        else
                        {
                            eventCount = GravitySDKFileJson.DeleteBatchTrackingData(flushCount);
                        }
                    }

                    mTask.Release();
                    if (eventCount > 0)
                    {
                        Flush();
                    }
                };
                mTask.StartRequest(mRequest, responseHandle, batchSize);
            }
        }

        public void FlushImmediately()
        {
            if (mEventSaveOnly == false)
            {
                mTask.SyncInvokeAllTask();

                int batchSize = (this.mConfig.GetMode() != Mode.NORMAL) ? 1 : mConfig.mUploadSize;
                ResponseHandle responseHandle = delegate(Dictionary<string, object> result)
                {
                    int eventCount = 0;
                    // 如果发生网络错误，这里result会为null，导致eventCount为0，进而不再继续发送数据，这是合理的
                    // 如果发生未授权2000情况，is_response_error为true，进而不再继续发送数据，这也是合理的
                    if (result != null && !(result.ContainsKey("is_response_error") &&
                                            Convert.ToBoolean(result["is_response_error"])))
                    {
                        if (!string.IsNullOrEmpty(this.mConfig.InstanceName()))
                        {
                            eventCount =
                                GravitySDKFileJson.DeleteBatchTrackingData(batchSize, this.mConfig.InstanceName());
                        }
                        else
                        {
                            eventCount = GravitySDKFileJson.DeleteBatchTrackingData(batchSize);
                        }
                    }

                    mTask.Release();
                    if (eventCount > 0)
                    {
                        Flush();
                    }
                };
                IList<Dictionary<string, object>> list = GravitySDKFileJson.DequeueBatchTrackingData(batchSize);
                if (!string.IsNullOrEmpty(this.mConfig.InstanceName()))
                {
                    list = GravitySDKFileJson.DequeueBatchTrackingData(batchSize, this.mConfig.InstanceName());
                }
                else
                {
                    list = GravitySDKFileJson.DequeueBatchTrackingData(batchSize);
                }

                if (list.Count > 0)
                {
                    this.mMono.StartCoroutine(mRequest.SendData_2(responseHandle, list));
                }
            }
        }

        public void Track(GravitySDKEventData analyticsEvent)
        {
            GravitySDKTimeInter time = GetTime(analyticsEvent.Time());
            analyticsEvent.SetTime(time);
            SendData(analyticsEvent);
        }

        public virtual void SetSuperProperties(Dictionary<string, object> superProperties)
        {
            if (IsPaused())
            {
                return;
            }

            Dictionary<string, object> properties = new Dictionary<string, object>();
            string propertiesStr =
                (string) GravitySDKFile.GetData(GravitySDKConstant.SUPER_PROPERTY, typeof(string));
            if (!string.IsNullOrEmpty(propertiesStr))
            {
                properties = GravitySDKJSON.Deserialize(propertiesStr);
            }

            GravitySDKUtil.AddDictionary(properties, superProperties);
            this.mSupperProperties = properties;
            GravitySDKFile.SaveData(GravitySDKConstant.SUPER_PROPERTY,
                GravitySDKJSON.Serialize(this.mSupperProperties));
        }

        public virtual void UnsetSuperProperty(string propertyKey)
        {
            if (IsPaused())
            {
                return;
            }

            Dictionary<string, object> properties = new Dictionary<string, object>();
            string propertiesStr =
                (string) GravitySDKFile.GetData(GravitySDKConstant.SUPER_PROPERTY, typeof(string));
            if (!string.IsNullOrEmpty(propertiesStr))
            {
                properties = GravitySDKJSON.Deserialize(propertiesStr);
            }

            if (properties.ContainsKey(propertyKey))
            {
                properties.Remove(propertyKey);
            }

            this.mSupperProperties = properties;
            GravitySDKFile.SaveData(GravitySDKConstant.SUPER_PROPERTY,
                GravitySDKJSON.Serialize(this.mSupperProperties));
        }

        public virtual Dictionary<string, object> SuperProperties()
        {
            string propertiesStr =
                (string) GravitySDKFile.GetData(GravitySDKConstant.SUPER_PROPERTY, typeof(string));
            if (!string.IsNullOrEmpty(propertiesStr))
            {
                this.mSupperProperties = GravitySDKJSON.Deserialize(propertiesStr);
            }

            return this.mSupperProperties;
        }

        public virtual void ClearSuperProperties()
        {
            if (IsPaused())
            {
                return;
            }

            this.mSupperProperties.Clear();
            GravitySDKFile.DeleteData(GravitySDKConstant.SUPER_PROPERTY);
        }

        public void TimeEvent(string eventName)
        {
            if (!mTimeEvents.ContainsKey(eventName))
            {
                mTimeEvents.Add(eventName, Environment.TickCount);
            }
        }

        /// <summary>
        /// 暂停事件的计时
        /// </summary>
        /// <param name="status">暂停状态，ture：暂停计时，false：取消暂停计时</param>
        /// <param name="eventName">事件名称，有值：暂停指定事件计时，无值：暂停全部事件计时</param>
        public void PauseTimeEvent(bool status, string eventName = "")
        {
            if (string.IsNullOrEmpty(eventName))
            {
                string[] eventNames = new string[mTimeEvents.Keys.Count];
                mTimeEvents.Keys.CopyTo(eventNames, 0);
                for (int i = 0; i < eventNames.Length; i++)
                {
                    string key = eventNames[i];
                    if (status == true)
                    {
                        int startTime = int.Parse(mTimeEvents[key].ToString());
                        int pauseTime = Environment.TickCount;
                        int duration = pauseTime - startTime;
                        if (mTimeEventsBefore.ContainsKey(key))
                        {
                            duration = duration + int.Parse(mTimeEventsBefore[key].ToString());
                        }

                        mTimeEventsBefore[key] = duration;
                    }
                    else
                    {
                        mTimeEvents[key] = Environment.TickCount;
                    }
                }
            }
            else
            {
                if (status == true)
                {
                    int startTime = int.Parse(mTimeEvents[eventName].ToString());
                    int pauseTime = Environment.TickCount;
                    int duration = pauseTime - startTime;
                    mTimeEventsBefore[eventName] = duration;
                }
                else
                {
                    mTimeEvents[eventName] = Environment.TickCount;
                }
            }
        }

        public void UserSet(Dictionary<string, object> properties)
        {
            UserSet(properties, DateTime.MinValue);
        }

        public void UserSet(Dictionary<string, object> properties, DateTime dateTime)
        {
            GravitySDKTimeInter time = GetTime(dateTime);
            GravitySDKUserData data = new GravitySDKUserData(time, GravitySDKConstant.USER_SET, properties);
            SendData(data);
        }

        public void UserUnset(string propertyKey)
        {
            UserUnset(propertyKey, DateTime.MinValue);
        }

        public void UserUnset(string propertyKey, DateTime dateTime)
        {
            GravitySDKTimeInter time = GetTime(dateTime);
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties[propertyKey] = 0;
            GravitySDKUserData data = new GravitySDKUserData(time, GravitySDKConstant.USER_UNSET, properties);
            SendData(data);
        }

        public void UserUnset(List<string> propertyKeys)
        {
            UserUnset(propertyKeys, DateTime.MinValue);
        }

        public void UserUnset(List<string> propertyKeys, DateTime dateTime)
        {
            GravitySDKTimeInter time = GetTime(dateTime);
            Dictionary<string, object> properties = new Dictionary<string, object>();
            foreach (string key in propertyKeys)
            {
                properties[key] = null;
            }

            GravitySDKUserData data = new GravitySDKUserData(time, GravitySDKConstant.USER_UNSET, properties);
            SendData(data);
        }

        public void UserSetOnce(Dictionary<string, object> properties)
        {
            UserSetOnce(properties, DateTime.MinValue);
        }

        public void UserSetOnce(Dictionary<string, object> properties, DateTime dateTime)
        {
            GravitySDKTimeInter time = GetTime(dateTime);
            GravitySDKUserData data = new GravitySDKUserData(time, GravitySDKConstant.USER_SETONCE, properties);
            SendData(data);
        }

        public void UserAdd(Dictionary<string, object> properties)
        {
            UserAdd(properties, DateTime.MinValue);
        }

        public void UserAdd(Dictionary<string, object> properties, DateTime dateTime)
        {
            GravitySDKTimeInter time = GetTime(dateTime);
            GravitySDKUserData data = new GravitySDKUserData(time, GravitySDKConstant.USER_ADD, properties);
            SendData(data);
        }

        public void UserNumberMin(Dictionary<string, object> properties)
        {
            UserNumberMin(properties, DateTime.MinValue);
        }

        public void UserNumberMin(Dictionary<string, object> properties, DateTime dateTime)
        {
            GravitySDKTimeInter time = GetTime(dateTime);
            GravitySDKUserData data = new GravitySDKUserData(time, GravitySDKConstant.USER_NUMBER_MIN, properties);
            SendData(data);
        }

        public void UserNumberMax(Dictionary<string, object> properties)
        {
            UserNumberMax(properties, DateTime.MinValue);
        }

        public void UserNumberMax(Dictionary<string, object> properties, DateTime dateTime)
        {
            GravitySDKTimeInter time = GetTime(dateTime);
            GravitySDKUserData data = new GravitySDKUserData(time, GravitySDKConstant.USER_NUMBER_MAX, properties);
            SendData(data);
        }

        public void UserAppend(Dictionary<string, object> properties)
        {
            UserAppend(properties, DateTime.MinValue);
        }

        public void UserAppend(Dictionary<string, object> properties, DateTime dateTime)
        {
            GravitySDKTimeInter time = GetTime(dateTime);
            GravitySDKUserData data = new GravitySDKUserData(time, GravitySDKConstant.USER_APPEND, properties);
            SendData(data);
        }

        public void UserUniqAppend(Dictionary<string, object> properties)
        {
            UserUniqAppend(properties, DateTime.MinValue);
        }

        public void UserUniqAppend(Dictionary<string, object> properties, DateTime dateTime)
        {
            GravitySDKTimeInter time = GetTime(dateTime);
            GravitySDKUserData data = new GravitySDKUserData(time, GravitySDKConstant.USER_UNIQ_APPEND, properties);
            SendData(data);
        }

        public void UserDelete()
        {
            UserDelete(DateTime.MinValue);
        }

        public void UserDelete(DateTime dateTime)
        {
            GravitySDKTimeInter time = GetTime(dateTime);
            Dictionary<string, object> properties = new Dictionary<string, object>();
            GravitySDKUserData data = new GravitySDKUserData(time, GravitySDKConstant.USER_DEL, properties);
            SendData(data);
        }

        public void SetDynamicSuperProperties(IDynamicSuperProperties_PC dynamicSuperProperties)
        {
            if (IsPaused())
            {
                return;
            }

            this.mDynamicProperties = dynamicSuperProperties;
        }

        protected bool IsPaused()
        {
            bool mIsPaused = !mEnableTracking || !mOptTracking;
            if (mIsPaused)
            {
                GravitySDKLogger.Print("Track status is Pause or Stop");
            }

            return mIsPaused;
        }

        public void SetTrackStatus(GE_TRACK_STATUS status)
        {
            GravitySDKLogger.Print("SetTrackStatus: " + status);
            switch (status)
            {
                case GE_TRACK_STATUS.PAUSE:
                    mEventSaveOnly = false;
                    OptTracking(true);
                    EnableTracking(false);
                    break;
                case GE_TRACK_STATUS.STOP:
                    mEventSaveOnly = false;
                    EnableTracking(true);
                    OptTracking(false);
                    break;
                case GE_TRACK_STATUS.SAVE_ONLY:
                    mEventSaveOnly = true;
                    EnableTracking(true);
                    OptTracking(true);
                    break;
                case GE_TRACK_STATUS.NORMAL:
                default:
                    mEventSaveOnly = false;
                    OptTracking(true);
                    EnableTracking(true);
                    Flush();
                    break;
            }
        }

        /*
        停止或开启数据上报,默认是开启状态,设置为停止时还会清空本地的访客ID,账号ID,静态公共属性
        其中true表示可以上报数据,false表示停止数据上报
        **/
        public void OptTracking(bool optTracking)
        {
            mOptTracking = optTracking;
            int opt = optTracking ? 1 : 0;
            GravitySDKFile.SaveData(GravitySDKConstant.OPT_TRACK, opt);
            if (!optTracking)
            {
                GravitySDKFile.DeleteData(GravitySDKConstant.ACCOUNT_ID);
                GravitySDKFile.DeleteData(GravitySDKConstant.DISTINCT_ID);
                GravitySDKFile.DeleteData(GravitySDKConstant.SUPER_PROPERTY);
                this.mAccountID = null;
                this.mDistinctID = null;
                this.mSupperProperties = new Dictionary<string, object>();
                GravitySDKFileJson.DeleteAllTrackingData();
            }
        }

        //是否暂停数据上报,默认是正常上报状态,其中true表示可以上报数据,false表示暂停数据上报
        public void EnableTracking(bool isEnable)
        {
            mEnableTracking = isEnable;
            int enable = isEnable ? 1 : 0;
            GravitySDKFile.SaveData(GravitySDKConstant.ENABLE_TRACK, enable);
        }

        private void DefaultTrackState()
        {
            object enableTrack = GravitySDKFile.GetData(GravitySDKConstant.ENABLE_TRACK, typeof(int));
            object optTrack = GravitySDKFile.GetData(GravitySDKConstant.OPT_TRACK, typeof(int));
            if (enableTrack != null)
            {
                this.mEnableTracking = ((int) enableTrack) == 1;
            }
            else
            {
                this.mEnableTracking = true;
            }

            if (optTrack != null)
            {
                this.mOptTracking = ((int) optTrack) == 1;
            }
            else
            {
                this.mOptTracking = true;
            }
        }

        //停止数据上报
        public void OptTrackingAndDeleteUser()
        {
            UserDelete();
            OptTracking(false);
        }

        // 这个不支持校准时间
        public string TimeString(DateTime dateTime)
        {
            return GravitySDKUtil.FormatDateTime(dateTime, mConfig.TimeZone());
        }
    }
}