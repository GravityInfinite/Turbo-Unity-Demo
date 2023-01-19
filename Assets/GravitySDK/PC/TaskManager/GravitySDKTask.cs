using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GravitySDK.PC.Request;
using GravitySDK.PC.Constant;
using GravitySDK.PC.Storage;

namespace GravitySDK.PC.TaskManager
{
    [DisallowMultipleComponent]
    public class GravitySDKTask : MonoBehaviour
    {
        private readonly static object _locker = new object();

        private List<GravitySDKBaseRequest> requestList = new List<GravitySDKBaseRequest>();
        private List<ResponseHandle> responseHandleList = new List<ResponseHandle>();
        private List<int> batchSizeList = new List<int>();
        private List<string> appIdList = new List<string>();


        private static GravitySDKTask mSingleTask;

        private bool isWaiting = false;

        private int mBatchSize = 30;

        public static GravitySDKTask SingleTask()
        {
            return mSingleTask;
        }

        private void Awake() {
            mSingleTask = this;
        }

        private void Start() {
        }

        private void Update() {
            if (requestList.Count > 0 && !isWaiting)
            {
                WaitOne();
                StartRequestSendData();
            }
        }

        /// <summary>
        /// 持有信号
        /// </summary>
        public void WaitOne()
        {
            isWaiting = true;
        }
        /// <summary>
        /// 释放信号
        /// </summary>
        public void Release()
        {
            isWaiting = false;
        }
        public void SyncInvokeAllTask()
        {

        }

        public void StartRequest(GravitySDKBaseRequest mRequest, ResponseHandle responseHandle, int batchSize, string appId)
        {
            lock(_locker)
            {
                requestList.Add(mRequest);
                responseHandleList.Add(responseHandle);
                batchSizeList.Add(batchSize);
                appIdList.Add(appId);
            }
        }

        private void StartRequestSendData() 
        {
            if (requestList.Count > 0)
            {
                GravitySDKBaseRequest mRequest;
                ResponseHandle responseHandle;
                IList<Dictionary<string, object>> list;
                lock(_locker)
                {
                    mRequest = requestList[0];
                    responseHandle = responseHandleList[0];
                    list = GravitySDKFileJson.DequeueBatchTrackingData(batchSizeList[0], appIdList[0]);
                }
                if (mRequest != null) 
                {
                    if (list.Count>0)
                    {
                        this.StartCoroutine(this.SendData(mRequest, responseHandle, list));                        
                    }
                    else
                    {
                        if (responseHandle != null) 
                        {
                            responseHandle(null);
                        }
                    }
                    lock(_locker)
                    {
                        requestList.RemoveAt(0);
                        responseHandleList.RemoveAt(0);
                        batchSizeList.RemoveAt(0);
                        appIdList.RemoveAt(0);
                    }
                }

            }
        }
        private IEnumerator SendData(GravitySDKBaseRequest mRequest, ResponseHandle responseHandle, IList<Dictionary<string, object>> list) {
            yield return mRequest.SendData_2(responseHandle, list);
        }
    }
}

