using System;
using System.IO;
using GravitySDK.PC.Constant;
using GravitySDK.PC.Config;
using UnityEngine;

namespace GravitySDK.PC.Utils
{
    public class GravitySDKLogger
    {
        public GravitySDKLogger()
        {
        }

        public static void Print(string str)
        {
            if (GravitySDKPublicConfig.IsPrintLog())
            {
                Debug.Log("[Turbo Unity_PC_V" + GravitySDKAppInfo.LibVersion() + "] " + str);
            }
        }
    }
}