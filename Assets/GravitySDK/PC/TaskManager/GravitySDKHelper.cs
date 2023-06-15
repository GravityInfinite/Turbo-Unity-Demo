using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GravitySDK.PC.Request;
using GravitySDK.PC.Constant;
using GravitySDK.PC.Storage;
using GravitySDK.PC.Utils;

namespace GravitySDK.PC.TaskManager
{
    [DisallowMultipleComponent]
    public class GravitySDKHelper : MonoBehaviour
    {
        private static GravitySDKHelper mSingleTask;

        public static GravitySDKHelper SingleTask()
        {
            return mSingleTask;
        }

        private void Awake()
        {
            mSingleTask = this;
        }

        private void Start()
        {
        }

        private void Update()
        {
        }


        public delegate void Callback(int a);

        public void t()
        {
            StartCoroutine(WaitAndReportRegister(10000));
        }

        public IEnumerator WaitAndReportRegister(long waitMills)
        {
            while (true)
            {
                GravitySDKLogger.Print("wait and run for " + waitMills);
                yield return new WaitForSeconds(waitMills);
                GravitySDKLogger.Print("wait and run");
            }
        }
    }
}