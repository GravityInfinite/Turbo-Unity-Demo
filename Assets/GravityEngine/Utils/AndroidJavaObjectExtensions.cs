using System.Collections.Generic;
using UnityEngine;

namespace GravityEngine.Utils
{
    /// <summary>
    /// Conversion methods for common Java types wrapped by <see cref="AndroidJavaObject"/>
    /// </summary>
    internal static class AndroidJavaObjectExtensions {
        /// <summary>
        /// Converts from a Java class which implements a toJSONObject method which returns a JSONObject representation
        /// of that object to a Serializable class in Unity
        /// </summary>
        public static TModel ToSerializable<TModel>(this AndroidJavaObject source) {
            if (source == default)
                return default;

            var json = source.Call<AndroidJavaObject>("toJSONObject");
            var jsonStr = json.Call<string>("toString");
            var serialized = JsonUtility.FromJson<TModel>(jsonStr);
            return serialized;
        }
        
        /*
         * JSONObject
         */
        
        /// <summary>
        /// Converts from a Java org.json.JSONObject to a <see cref="Dictionary{TKey,TValue}"/>
        /// </summary>
        public static Dictionary<string, object> JSONObjectToDictionary(this AndroidJavaObject source)
            => source != null ? GE_MiniJson.Deserialize(source.Call<string>("toString")) as Dictionary<string, object> : null;

        /// <summary>
        /// Converts from a <see cref="Dictionary{TKey,TValue}"/> to a Java org.json.JSONObject
        /// </summary>
        public static AndroidJavaObject ToJSONObject<TKey, TValue>(this Dictionary<TKey, TValue> source)
            => new AndroidJavaObject("org.json.JSONObject", GE_MiniJson.Serialize(source));
        
        /*
         * Map
         */
        
        /// <summary>
        /// Converts from a Java java.util.Map to a <see cref="Dictionary{TKey,TValue}"/>
        /// </summary>
        public static Dictionary<string, string> MapToDictionary(this AndroidJavaObject source) {
            if (source == null)
                return null;
            
            var entries = source.Call<AndroidJavaObject>("entrySet");
            var iter = entries.Call<AndroidJavaObject>("iterator");

            var ret = new Dictionary<string, string>();

            do {
                var entry = iter.Call<AndroidJavaObject>("next");
                var key = entry.Call<string>("getKey");
                var valueJO = entry.Call<AndroidJavaObject>("getValue");
                
                ret[key] = valueJO.Call<string>("toString");
            } while (iter.Call<bool>("hasNext"));
            
            return ret;
        }

        /// <summary>
        /// Converts from a <see cref="Dictionary{TKey,TValue}"/> to a Java java.util.Map
        /// </summary>
        public static AndroidJavaObject ToMap(this Dictionary<string, string> source) {
            var map = new AndroidJavaObject("java.util.HashMap");
            var put = AndroidJNIHelper.GetMethodID(map.GetRawClass(), "put", "(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;");

            var entryArgs = new object[2];
            foreach (var kv in source) {
                using (var key = new AndroidJavaObject("java.lang.String", kv.Key))
                using (var value = new AndroidJavaObject("java.lang.String", kv.Value)) {
                    entryArgs[0] = key;
                    entryArgs[1] = value;
                    AndroidJNI.CallObjectMethod(map.GetRawObject(), put, AndroidJNIHelper.CreateJNIArgArray(entryArgs));
                }    
            }
            
            return map;
        }

        public static AndroidJavaObject ToArrayList(this string[] keys) {
            AndroidJavaObject arrayList = new AndroidJavaObject("java.util.ArrayList");

            foreach(string key in keys) {
                arrayList.Call<bool>("add", key);
            }

            return arrayList;
        }

    }
}