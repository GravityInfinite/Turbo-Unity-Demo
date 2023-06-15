using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GravitySDK.PC.Main;
using GravitySDK.PC.Storage;
using GravitySDK.PC.Utils;
using GravitySDK.PC.Constant;

namespace GravitySDK.PC.AutoTrack
{
    public class GravitySDKAutoTrack : MonoBehaviour
    {
        private AUTO_TRACK_EVENTS mAutoTrackEvents = AUTO_TRACK_EVENTS.NONE;

        private Dictionary<string, Dictionary<string, object>> mAutoTrackProperties =
            new Dictionary<string, Dictionary<string, object>>();

        private bool mStarted = false;
        private IAutoTrackEventCallback_PC mEventCallback_PC;

        void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                if ((mAutoTrackEvents & AUTO_TRACK_EVENTS.APP_START) != 0)
                {
                    Dictionary<string, object> properties = new Dictionary<string, object>();
                    if (mAutoTrackProperties.ContainsKey(AUTO_TRACK_EVENTS.APP_START.ToString()))
                    {
                        GravitySDKUtil.AddDictionary(properties,
                            mAutoTrackProperties[AUTO_TRACK_EVENTS.APP_START.ToString()]);
                    }

                    if (mEventCallback_PC != null)
                    {
                        GravitySDKUtil.AddDictionary(properties,
                            mEventCallback_PC.AutoTrackEventCallback_PC((int) AUTO_TRACK_EVENTS.APP_START, properties));
                    }

                    GravityPCSDK.Track(GravitySDKConstant.START_EVENT, properties);
                }

                if ((mAutoTrackEvents & AUTO_TRACK_EVENTS.APP_END) != 0)
                {
                    // 开始记录end事件
                    GravityPCSDK.TimeEvent(GravitySDKConstant.END_EVENT);
                }

                GravityPCSDK.PauseTimeEvent(false);
            }
            else
            {
                if ((mAutoTrackEvents & AUTO_TRACK_EVENTS.APP_END) != 0)
                {
                    Dictionary<string, object> properties = new Dictionary<string, object>();
                    if (mAutoTrackProperties.ContainsKey(AUTO_TRACK_EVENTS.APP_END.ToString()))
                    {
                        GravitySDKUtil.AddDictionary(properties,
                            mAutoTrackProperties[AUTO_TRACK_EVENTS.APP_END.ToString()]);
                    }

                    if (mEventCallback_PC != null)
                    {
                        GravitySDKUtil.AddDictionary(properties,
                            mEventCallback_PC.AutoTrackEventCallback_PC((int) AUTO_TRACK_EVENTS.APP_END, properties));
                    }

                    // 上报end事件
                    GravityPCSDK.Track(GravitySDKConstant.END_EVENT, properties);
                }

                GravityPCSDK.Flush();

                GravityPCSDK.PauseTimeEvent(true);
            }
        }

        void OnApplicationQuit()
        {
            if (Application.isFocused == true)
            {
                OnApplicationFocus(false);
            }

            GravityPCSDK.FlushImmediately();
        }

        public void EnableAutoTrack(AUTO_TRACK_EVENTS events, Dictionary<string, object> properties)
        {
            SetAutoTrackProperties(events, properties);
            if ((events & AUTO_TRACK_EVENTS.APP_INSTALL) != 0)
            {
                object result = GravitySDKFile.GetData(GravitySDKConstant.IS_INSTALL, typeof(int));
                if (result == null)
                {
                    Dictionary<string, object> mProperties = new Dictionary<string, object>(properties);
                    GravitySDKFile.SaveData(GravitySDKConstant.IS_INSTALL, 1);
                    if (mAutoTrackProperties.ContainsKey(AUTO_TRACK_EVENTS.APP_INSTALL.ToString()))
                    {
                        GravitySDKUtil.AddDictionary(mProperties,
                            mAutoTrackProperties[AUTO_TRACK_EVENTS.APP_INSTALL.ToString()]);
                    }

                    GravityPCSDK.Track(GravitySDKConstant.INSTALL_EVENT, mProperties);
                    GravityPCSDK.Flush();
                }
            }

            if ((events & AUTO_TRACK_EVENTS.APP_START) != 0 && mStarted == false)
            {
                Dictionary<string, object> mProperties = new Dictionary<string, object>(properties);
                if (mAutoTrackProperties.ContainsKey(AUTO_TRACK_EVENTS.APP_START.ToString()))
                {
                    GravitySDKUtil.AddDictionary(mProperties,
                        mAutoTrackProperties[AUTO_TRACK_EVENTS.APP_START.ToString()]);
                }

                GravityPCSDK.Track(GravitySDKConstant.START_EVENT, mProperties);
                GravityPCSDK.Flush();
            }

            if ((events & AUTO_TRACK_EVENTS.APP_END) != 0 && mStarted == false)
            {
                GravityPCSDK.TimeEvent(GravitySDKConstant.END_EVENT);
            }

            mStarted = true;
        }

        public void EnableAutoTrack(AUTO_TRACK_EVENTS events, IAutoTrackEventCallback_PC eventCallback)
        {
            mAutoTrackEvents = events;
            mEventCallback_PC = eventCallback;
            if ((events & AUTO_TRACK_EVENTS.APP_INSTALL) != 0)
            {
                object result = GravitySDKFile.GetData(GravitySDKConstant.IS_INSTALL, typeof(int));
                if (result == null)
                {
                    GravitySDKFile.SaveData(GravitySDKConstant.IS_INSTALL, 1);
                    Dictionary<string, object> properties = null;
                    if (mAutoTrackProperties.ContainsKey(AUTO_TRACK_EVENTS.APP_INSTALL.ToString()))
                    {
                        properties = mAutoTrackProperties[AUTO_TRACK_EVENTS.APP_INSTALL.ToString()];
                    }
                    else
                    {
                        properties = new Dictionary<string, object>();
                    }

                    if (mEventCallback_PC != null)
                    {
                        GravitySDKUtil.AddDictionary(properties,
                            mEventCallback_PC.AutoTrackEventCallback_PC((int) AUTO_TRACK_EVENTS.APP_INSTALL,
                                properties));
                    }

                    GravityPCSDK.Track(GravitySDKConstant.INSTALL_EVENT, properties);
                    GravityPCSDK.Flush();
                }
            }

            if ((events & AUTO_TRACK_EVENTS.APP_START) != 0 && mStarted == false)
            {
                Dictionary<string, object> properties = null;
                if (mAutoTrackProperties.ContainsKey(AUTO_TRACK_EVENTS.APP_START.ToString()))
                {
                    properties = mAutoTrackProperties[AUTO_TRACK_EVENTS.APP_START.ToString()];
                }
                else
                {
                    properties = new Dictionary<string, object>();
                }

                if (mEventCallback_PC != null)
                {
                    GravitySDKUtil.AddDictionary(properties,
                        mEventCallback_PC.AutoTrackEventCallback_PC((int) AUTO_TRACK_EVENTS.APP_START, properties));
                }

                GravityPCSDK.Track(GravitySDKConstant.START_EVENT, properties);
                GravityPCSDK.Flush();
            }

            if ((events & AUTO_TRACK_EVENTS.APP_END) != 0 && mStarted == false)
            {
                GravityPCSDK.TimeEvent(GravitySDKConstant.END_EVENT);
            }

            mStarted = true;
        }

        public void SetAutoTrackProperties(AUTO_TRACK_EVENTS events, Dictionary<string, object> properties)
        {
            mAutoTrackEvents = events;
            if ((events & AUTO_TRACK_EVENTS.APP_INSTALL) != 0)
            {
                if (mAutoTrackProperties.ContainsKey(AUTO_TRACK_EVENTS.APP_INSTALL.ToString()))
                {
                    GravitySDKUtil.AddDictionary(mAutoTrackProperties[AUTO_TRACK_EVENTS.APP_INSTALL.ToString()],
                        properties);
                }

                mAutoTrackProperties[AUTO_TRACK_EVENTS.APP_INSTALL.ToString()] = properties;
            }

            if ((events & AUTO_TRACK_EVENTS.APP_START) != 0)
            {
                if (mAutoTrackProperties.ContainsKey(AUTO_TRACK_EVENTS.APP_START.ToString()))
                {
                    GravitySDKUtil.AddDictionary(mAutoTrackProperties[AUTO_TRACK_EVENTS.APP_START.ToString()],
                        properties);
                }

                mAutoTrackProperties[AUTO_TRACK_EVENTS.APP_START.ToString()] = properties;
            }

            if ((events & AUTO_TRACK_EVENTS.APP_END) != 0)
            {
                if (mAutoTrackProperties.ContainsKey(AUTO_TRACK_EVENTS.APP_END.ToString()))
                {
                    GravitySDKUtil.AddDictionary(mAutoTrackProperties[AUTO_TRACK_EVENTS.APP_END.ToString()],
                        properties);
                }

                mAutoTrackProperties[AUTO_TRACK_EVENTS.APP_END.ToString()] = properties;
            }

            if ((events & AUTO_TRACK_EVENTS.APP_CRASH) != 0)
            {
                if (mAutoTrackProperties.ContainsKey(AUTO_TRACK_EVENTS.APP_CRASH.ToString()))
                {
                    GravitySDKUtil.AddDictionary(mAutoTrackProperties[AUTO_TRACK_EVENTS.APP_CRASH.ToString()],
                        properties);
                }

                mAutoTrackProperties[AUTO_TRACK_EVENTS.APP_CRASH.ToString()] = properties;
            }
        }
    }
}