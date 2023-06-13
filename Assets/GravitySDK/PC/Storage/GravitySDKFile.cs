using System;

#if GRAVITY_BYTEDANCE_GAME_MODE
using StarkSDKSpace;
#endif

namespace GravitySDK.PC.Storage
{
    public class GravitySDKFile
    {
        public static string GetKey(string prefix,string key)
        {
            return prefix +"_"+ key;
        }
        public static void SaveData(string prefix, string key, object value)
        {
            SaveData(GetKey(prefix, key), value);
        }
        public static void SaveData(string key, object value)
        {
            if (!string.IsNullOrEmpty(key))
            {
#if GRAVITY_WECHAT_GAME_MODE
                if (value.GetType() == typeof(int))
                {
                    PlayerPrefs.SetInt(key, (int)value);
                }
                else if (value.GetType() == typeof(float))
                {
                    PlayerPrefs.SetFloat(key, (float)value);
                }
                else if (value.GetType() == typeof(string))
                {
                    PlayerPrefs.SetString(key, (string)value);
                }
                PlayerPrefs.Save();  
#elif GRAVITY_BYTEDANCE_GAME_MODE
                if (value.GetType() == typeof(int))
                {
                    StarkSDK.API.PlayerPrefs.SetInt(key, (int)value);
                }
                else if (value.GetType() == typeof(float))
                {
                    StarkSDK.API.PlayerPrefs.SetFloat(key, (float)value);
                }
                else if (value.GetType() == typeof(string))
                {
                    StarkSDK.API.PlayerPrefs.SetString(key, (string)value);
                }
                StarkSDK.API.PlayerPrefs.Save();
#else
                if (value.GetType() == typeof(int))
                {
                    UnityEngine.PlayerPrefs.SetInt(key, (int)value);
                }
                else if (value.GetType() == typeof(float))
                {
                    UnityEngine.PlayerPrefs.SetFloat(key, (float)value);
                }
                else if (value.GetType() == typeof(string))
                {
                    UnityEngine.PlayerPrefs.SetString(key, (string)value);
                }
                UnityEngine.PlayerPrefs.Save();
#endif
            }
        }
        public static object GetData(string key, Type type)
        {
#if GRAVITY_WECHAT_GAME_MODE
            if (!string.IsNullOrEmpty(key) && PlayerPrefs.HasKey(key))
            {
                if (type == typeof(int))
                {
                    return PlayerPrefs.GetInt(key);
                }
                else if (type == typeof(float))
                {
                    return PlayerPrefs.GetFloat(key);
                }
                else if (type == typeof(string))
                {
                    return PlayerPrefs.GetString(key);
                }
                PlayerPrefs.Save();
            }
#elif GRAVITY_BYTEDANCE_GAME_MODE
            if (!string.IsNullOrEmpty(key) && StarkSDK.API.PlayerPrefs.HasKey(key))
            {
                if (type == typeof(int))
                {
                    return StarkSDK.API.PlayerPrefs.GetInt(key);
                }
                else if (type == typeof(float))
                {
                    return StarkSDK.API.PlayerPrefs.GetFloat(key);
                }
                else if (type == typeof(string))
                {
                    return StarkSDK.API.PlayerPrefs.GetString(key);
                }
                StarkSDK.API.PlayerPrefs.Save();
            }    
#else
            if (!string.IsNullOrEmpty(key) && UnityEngine.PlayerPrefs.HasKey(key))
            {
                if (type == typeof(int))
                {
                    return UnityEngine.PlayerPrefs.GetInt(key);
                }
                else if (type == typeof(float))
                {
                    return UnityEngine.PlayerPrefs.GetFloat(key);
                }
                else if (type == typeof(string))
                {
                    return UnityEngine.PlayerPrefs.GetString(key);
                }
                UnityEngine.PlayerPrefs.Save();
            }
#endif
            return null;

        }
        public static object GetData(string prefix,string key, Type type)
        {
            key = GetKey(prefix, key);
            return GetData(key, type);
        }

        public static void DeleteData(string key)
        {
            if (!string.IsNullOrEmpty(key))
            {
#if GRAVITY_WECHAT_GAME_MODE
                if (PlayerPrefs.HasKey(key))
                {
                    PlayerPrefs.DeleteKey(key);
                }   
#elif GRAVITY_BYTEDANCE_GAME_MODE
                if (StarkSDK.API.PlayerPrefs.HasKey(key))
                {
                    StarkSDK.API.PlayerPrefs.DeleteKey(key);
                }
#else
                if (UnityEngine.PlayerPrefs.HasKey(key))
                {
                    UnityEngine.PlayerPrefs.DeleteKey(key);
                }
#endif
            }
        }
        public static void DeleteData(string prefix,string key)
        {
            key = GetKey(prefix, key);
            DeleteData(key);
        }
    }
}

