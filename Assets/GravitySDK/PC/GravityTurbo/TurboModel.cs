using System;
using System.Collections.Generic;

namespace GravitySDK.PC.GravityTurbo
{
    [Serializable]
    public class AdData
    {
        // 头条
        public string clue_token;
        public string ad_id;
        public string creative_id;
        public string request_id;
        public string req_id;
        public string advertiser_id;
        
        public string project_id;
        public string promotion_id;
        public string mid1;
        public string mid2;
        public string mid3;
        public string mid4;
        public string mid5;

        // 快手
        public string callback;
        public string ksChannel;
        public string ksCampaignId;
        public string ksUnitId;
        public string ksCreativeId;
        
        // 腾讯 click_id?
        public string gdt_vid;

        // 百度
        public string bd_vid;

        // 引力换量
        public string turbo_vid;
    }

    [Serializable]
    public class RegisterRequestBody
    {
        public string client_id;
        public string name;
        public string channel;
        public int version;
        public string media_type;
        public string wx_openid;
        public string wx_unionid;
        public AdData ad_data;
        public string promoted_object_id;
        public Dictionary<string, string > query_object;
    }

    [Serializable]
    public class Properties
    {
        public long amount;
        public long real_amount;
    }

    [Serializable]
    public class HandleEventBody
    {
        public string event_type;
        public Properties properties;
        public bool use_client_time;
        public long timestamp;
        public string trace_id;
    }

    [Serializable]
    public class QueryUserBody
    {
        public string[] user_list;
    }
}