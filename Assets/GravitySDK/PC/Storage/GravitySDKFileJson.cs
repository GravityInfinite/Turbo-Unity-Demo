using System;
using System.Collections.Generic;
using UnityEngine;
using GravitySDK.PC.Utils;

namespace GravitySDK.PC.Storage
{
    public class GravitySDKFileJson
    {
        // 保存事件，返回缓存事件数量
        internal static int EnqueueTrackingData(Dictionary<string, object> data, string prefix = "gravity")
        {
            int eventId = EventAutoIncrementingID(prefix);
            String trackingKey = prefix + "Event" + eventId.ToString();
            data["id"] = trackingKey;
            PlayerPrefs.SetString(trackingKey, GravitySDKJSON.Serialize(data));
            IncreaseTrackingDataID(prefix);
            int eventCount = EventAutoIncrementingID(prefix) - EventIndexID(prefix);
            return eventCount;
        }

        // 获取事件结束ID
        internal static int EventAutoIncrementingID(string prefix = "gravity")
        {
            string mEventAutoIncrementingID = prefix + "EventAutoIncrementingID";
            return PlayerPrefs.HasKey(mEventAutoIncrementingID) ? PlayerPrefs.GetInt(mEventAutoIncrementingID) : 0;
        }

        // 自动增加事件结束ID
        private static void IncreaseTrackingDataID(string prefix = "gravity")
        {
            int id = EventAutoIncrementingID(prefix);
            id += 1;
            String trackingIdKey = prefix + "EventAutoIncrementingID";
            PlayerPrefs.SetInt(trackingIdKey, id);
        }

        // 获取事件起始ID
        internal static int EventIndexID(string prefix = "gravity")
        {
            string mEventIndexID = prefix + "EventIndexID";
            return PlayerPrefs.HasKey(mEventIndexID) ? PlayerPrefs.GetInt(mEventIndexID) : 0;
        }

        // 保存时间起始ID
        private static void SaveEventIndexID(int indexID, string prefix = "gravity")
        {
            String trackingIdKey = prefix + "EventIndexID";
            PlayerPrefs.SetInt(trackingIdKey, indexID);
        }

        // 批量取出指定数量的事件
        internal static List<Dictionary<string, object>> DequeueBatchTrackingData(int batchSize, string prefix = "gravity")
        {
            List<Dictionary<string, object>> batch = new List<Dictionary<string, object>>();
            int dataIndex = EventIndexID(prefix);
            int maxIndex = EventAutoIncrementingID(prefix) - 1;
            while (batch.Count < batchSize && dataIndex <= maxIndex)
            {
                String trackingKey = prefix + "Event" + dataIndex.ToString();
                if (PlayerPrefs.HasKey(trackingKey))
                {
                    try
                    {
                        Dictionary<string, object>
                            data = GravitySDKJSON.Deserialize(PlayerPrefs.GetString(trackingKey));
                        data.Remove("id");
                        batch.Add(data);
                    }
                    catch (Exception e)
                    {
                        GravitySDKLogger.Print("There was an error processing " + trackingKey +
                                               " from the internal object pool: " + e);
                        PlayerPrefs.DeleteKey(trackingKey);
                    }
                }

                dataIndex++;
            }

            return batch;
        }

        // 批量删除指定数量的事件，返回剩余事件数量
        internal static int DeleteBatchTrackingData(int batchSize, string prefix = "gravity")
        {
            int deletedCount = 0;
            int dataIndex = EventIndexID(prefix);
            int maxIndex = EventAutoIncrementingID(prefix) - 1;
            while (deletedCount < batchSize && dataIndex <= maxIndex)
            {
                String trackingKey = prefix + "Event" + dataIndex.ToString();
                if (PlayerPrefs.HasKey(trackingKey))
                {
                    PlayerPrefs.DeleteKey(trackingKey);
                    deletedCount++;
                }

                dataIndex++;
            }

            SaveEventIndexID(dataIndex, prefix);

            int eventCount = EventAutoIncrementingID(prefix) - EventIndexID(prefix);
            return eventCount;
        }

        // 批量删除指定事件
        // internal static void DeleteBatchTrackingData(List<Dictionary<string, object>> batch, string prefix = "gravity") {
        //     foreach(Dictionary<string, object> data in batch) {
        //         String id = data["id"].ToString();
        //         if (id != null && PlayerPrefs.HasKey(id)) {
        //             PlayerPrefs.DeleteKey(id);
        //         }
        //     }
        // }

        // 批量删除全部事件
        internal static int DeleteAllTrackingData(string prefix = "gravity")
        {
            DeleteBatchTrackingData(int.MaxValue, prefix);
            SaveEventIndexID(0, prefix);
            return 0;
        }
    }
}