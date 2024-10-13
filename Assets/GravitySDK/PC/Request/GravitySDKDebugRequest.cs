using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using GravitySDK.PC.Config;
using GravitySDK.PC.Constant;
using GravitySDK.PC.Utils;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using GravitySDK.PC.GravityTurbo;

namespace GravitySDK.PC.Request
{
    public class GravitySDKDebugRequest : GravitySDKBaseRequest
    {
        public GravitySDKDebugRequest(string url, IList<Dictionary<string, object>> data) : base(url, data)
        {
        }

        public GravitySDKDebugRequest(string url) : base(url)
        {
        }

        public override IEnumerator SendData_2(ResponseHandle responseHandle, IList<Dictionary<string, object>> data)
        {
            this.SetData(data);
            string uri = this.URL();

            Dictionary<string, object> param = new Dictionary<string, object>();
            param["event_list"] = this.Data();
            param["client_id"] = GravityHelper.GetClientId();
            param["flush_time"] = GravitySDKUtil.GetTimeStamp();
            string content = GravitySDKJSON.Serialize(param);
            byte[] contentCompressed = Encoding.UTF8.GetBytes(content);

            using (UnityWebRequest webRequest = new UnityWebRequest(uri, "POST"))
            {
                webRequest.timeout = 30;
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Turbo-Debug-Mode", "1");
                webRequest.uploadHandler = (UploadHandler) new UploadHandlerRaw(contentCompressed);
                webRequest.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();

                GravitySDKLogger.Print("Post event: " + content + "\n  Request URL: " + uri);

                // Request and wait for the desired page.
                yield return webRequest.SendWebRequest();

                Dictionary<string, object> resultDict = null;
#if UNITY_2020_1_OR_NEWER
                switch (webRequest.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.DataProcessingError:
                    case UnityWebRequest.Result.ProtocolError:
                        GravitySDKLogger.Print("Error response : " + webRequest.error);
                        break;
                    case UnityWebRequest.Result.Success:
                        GravitySDKLogger.Print("Response : " + webRequest.downloadHandler.text);
                        if (!string.IsNullOrEmpty(webRequest.downloadHandler.text))
                        {
                            resultDict = GravitySDKJSON.Deserialize(webRequest.downloadHandler.text);
                        }

                        break;
                }
#else
                if (webRequest.isHttpError || webRequest.isNetworkError)
                {
                    GravitySDKLogger.Print("Error response : " + webRequest.error);
                }
                else
                {
                    GravitySDKLogger.Print("Response : " + webRequest.downloadHandler.text);
                    if (!string.IsNullOrEmpty(webRequest.downloadHandler.text)) 
                    {
                        resultDict = GravitySDKJSON.Deserialize(webRequest.downloadHandler.text);
                    } 
                }
#endif
                if (responseHandle != null)
                {
                    // 用户还没注册成功时，返回2000，此时不能删除本地事件
                    if (resultDict != null)
                    {
                        resultDict.Add("flush_count", data.Count);
                        int code = Convert.ToInt32(resultDict["code"]);
                        resultDict.Add("is_response_error",
                            resultDict.ContainsKey("code") && (code == 2000 || code != 0));
                    }

                    responseHandle(resultDict);
                }
            }
        }
    }
}