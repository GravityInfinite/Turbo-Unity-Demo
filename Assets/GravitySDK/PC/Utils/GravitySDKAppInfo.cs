using System;
using GravitySDK.PC.Config;
using GravitySDK.PC.Constant;
using UnityEngine;

namespace GravitySDK.PC.Utils
{
    public class GravitySDKAppInfo
    {
        //SDK版本号
        public static string LibVersion()
        {
            if (GravitySDKUtil.DisPresetProperties.Contains(GravitySDKConstant.LIB_VERSION))
            {
                return "";
            }
            return GravitySDKPublicConfig.Version() ;
        }
        //SDK名称
        public static string LibName()
        {
            if (GravitySDKUtil.DisPresetProperties.Contains(GravitySDKConstant.LIB))
            {
                return "";
            }
            return GravitySDKPublicConfig.Name();
        }
        //app版本号
        public static string AppVersion()
        {
            if (GravitySDKUtil.DisPresetProperties.Contains(GravitySDKConstant.APP_VERSION))
            {
                return "";
            }
            return Application.version;
        }
        //app唯一标识 包名
        public static string AppIdentifier()
        {
            if (GravitySDKUtil.DisPresetProperties.Contains(GravitySDKConstant.APP_BUNDLEID))
            {
                return "";
            }
            return Application.identifier;
        }
     
    }
}