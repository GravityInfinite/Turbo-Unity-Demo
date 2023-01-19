using UnityEngine;

namespace GravityEngine.Utils
{
    public class GE_Log
    {
        private static bool enableLog;
        public static void EnableLog(bool enabled)
        {
            enableLog = enabled;
        }

        public static void d(string message)
        {
            if (enableLog)
            {
                Debug.Log("[Turbo Unity_PC_V"+ GE_PublicConfig.LIB_VERSION + "] " + message);
            }
        }

        public static void e(string message)
        {
            if (enableLog)
            {
                Debug.LogError("[Turbo Unity_PC_V"+ GE_PublicConfig.LIB_VERSION + "] " + message);
            }
        }

        public static void w(string message)
        {
            if (enableLog)
            {
                Debug.LogWarning("[Turbo Unity_PC_V"+ GE_PublicConfig.LIB_VERSION + "] " + message);
            }
        }
    }
}