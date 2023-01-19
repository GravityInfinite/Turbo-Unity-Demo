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

namespace GravitySDK.PC.Request
{
    public class GravitySDKDebugRequest:GravitySDKBaseRequest
    {
        private int mDryRun = 0;
        private string mDeviceID = GravitySDKDeviceInfo.DeviceID();
        public void SetDryRun(int dryRun)
        {
            mDryRun = dryRun;
        }
        public GravitySDKDebugRequest(string appId, string url, IList<Dictionary<string, object>> data):base(appId,url,data)
        {
            
        }
        public GravitySDKDebugRequest(string appId, string url) : base(appId, url)
        {
        }

        public override IEnumerator SendData_2(ResponseHandle responseHandle, IList<Dictionary<string, object>> data)
        {
            this.SetData(data);
            string uri = this.URL();
            string content = GravitySDKJSON.Serialize(this.Data()[0]);

            WWWForm form = new WWWForm();
            form.AddField("appid", this.APPID());
            form.AddField("source", "client");
            form.AddField("dryRun", mDryRun);
            form.AddField("deviceId", mDeviceID);
            form.AddField("data", content);

            using (UnityWebRequest webRequest = UnityWebRequest.Post(uri, form))
            {
                webRequest.timeout = 30;
                webRequest.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

                GravitySDKLogger.Print("Post event: " + content + "\n  Request URL: " + uri);

                // Request and wait for the desired page.
                yield return webRequest.SendWebRequest();

                Dictionary<string,object> resultDict = null;
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
                    if (resultDict != null)
                    {
                        resultDict.Add("flush_count", data.Count);
                    }
                    responseHandle(resultDict);
                }
            }
        }
    }
}
