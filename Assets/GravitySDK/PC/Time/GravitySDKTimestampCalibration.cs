using System;
namespace GravitySDK.PC.Time
{
    public class GravitySDKTimestampCalibration : GravitySDKTimeCalibration
    {

        public GravitySDKTimestampCalibration(long timestamp)
        {
            this.mStartTime = timestamp;
            this.mSystemElapsedRealtime = Environment.TickCount;
        } 
    }
}

