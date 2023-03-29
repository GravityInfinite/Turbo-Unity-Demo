using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace GravitySDK.PC.GravityTurbo
{
    public class UnityWebRequestMgr : MonoBehaviour
    {
        #region 单例

        private static UnityWebRequestMgr instance = null;

        private static readonly object locker = new object();

        private static bool bAppQuitting;

        public static UnityWebRequestMgr Instance
        {
            get
            {
                if (bAppQuitting)
                {
                    instance = null;
                    return instance;
                }

                lock (locker)
                {
                    if (instance == null)
                    {
                        instance = FindObjectOfType<UnityWebRequestMgr>();
                        if (FindObjectsOfType<UnityWebRequestMgr>().Length > 1)
                        {
                            Debug.LogError("不应该存在多个单例！");
                            return instance;
                        }

                        if (instance == null)
                        {
                            var singleton = new GameObject();
                            instance = singleton.AddComponent<UnityWebRequestMgr>();
                            singleton.name = "(singleton)" + typeof(UnityWebRequestMgr);
                            singleton.hideFlags = HideFlags.None;
                            DontDestroyOnLoad(singleton);
                        }
                        else
                            DontDestroyOnLoad(instance.gameObject);
                    }

                    instance.hideFlags = HideFlags.None;
                    return instance;
                }
            }
        }

        private void Awake()
        {
            bAppQuitting = false;
        }

        private void OnDestroy()
        {
            bAppQuitting = true;
        }

        #endregion

        public delegate void Callback();

        /// <summary>
        /// GET请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="actionResult"></param>
        public void Get(string url, Action<UnityWebRequest> actionResult)
        {
            StartCoroutine(_Get(url, actionResult));
        }

        /// <summary>
        /// 向服务器提交post请求
        /// </summary>
        /// <param name="serverURL">服务器请求目标地址</param>
        /// <param name="requestBody">请求的结构体</param>
        /// <param name="actionResult">处理返回结果的委托,处理请求对象</param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public void Post(string serverURL, object requestBody, Action<UnityWebRequest> actionResult, Callback callback=null)
        {
            StartCoroutine(_Post(serverURL, requestBody, actionResult, callback));
        }

        /// <summary>
        /// GET请求
        /// </summary>
        /// <param name="url">请求地址,like 'http://www.shijing720.com/ '</param>
        /// <param name="actionResult">请求发起后处理回调结果的委托</param>
        /// <returns></returns>
        IEnumerator _Get(string url, Action<UnityWebRequest> actionResult)
        {
            using (UnityWebRequest uwr = UnityWebRequest.Get(url))
            {
                yield return uwr.SendWebRequest();
                actionResult?.Invoke(uwr);
            }
        }

        /// <summary>
        /// 向服务器提交post请求
        /// </summary>
        /// <param name="serverURL">服务器请求目标地址</param>
        /// <param name="requestBody">发送的结构体</param>
        /// <param name="actionResult">处理返回结果的委托</param>
        /// <returns></returns>
        IEnumerator _Post(string serverURL, object requestBody, Action<UnityWebRequest> actionResult, Callback callback)
        {
            var uwr = new UnityWebRequest(serverURL, "POST");

            uwr.uploadHandler =
                (UploadHandler) new UploadHandlerRaw(
                    System.Text.Encoding.Default.GetBytes(JsonUtility.ToJson(requestBody)));
            uwr.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();

            uwr.SetRequestHeader("Content-Type", "application/json");
            yield return uwr.SendWebRequest();
            actionResult?.Invoke(uwr);
            callback?.Invoke();
        }
    }
}