using System;
using UnityEngine;
using UnityEngine.Networking;

namespace StarkSDKSpace
{
    /**
     * 只是用来承载 asset bundle 的获取，
     * TTAssetBundle.load 触发下载后，这里并不会获取到真正的 Asset Bundle 数据
     */
    public class DownloadHandlerTTAssetBundle : DownloadHandlerScript
    {
        public bool isDone;
        
        private string _uri;
        private uint _crc;
        private AssetBundle _assetBundle;
        private byte[] _contentBytes;
        private int _contentLength;
        
        private static bool _isFallbackNoticed = false;
        
        public DownloadHandlerTTAssetBundle(string uri, uint crc)
        {
            _uri = uri;
            _crc = crc;
        }

        public AssetBundle assetBundle
        {
            get
            {
                if (_assetBundle == null)
                {
                    if (TTAssetBundle.isAbfsReady)
                    {
                        if (_contentLength != 0)
                        {
                            Debug.LogError($"DownloadHandlerTTAssetBundle contentLength not 0!");
                            return null;
                        }
                        _assetBundle = AssetBundle.LoadFromFile(_uri, _crc);
                        TTAssetBundle.bundle2path.Add(_assetBundle, _uri);
                    }
                    else
                    {
                        _assetBundle = AssetBundle.LoadFromMemory(_contentBytes);
                    }
                }
                return _assetBundle;
            }
        }

        protected override byte[] GetData() => _contentBytes;

        protected override bool ReceiveData(byte[] data, int dataLength)
        {
            if (data == null || dataLength < 1)
                return false;
            
#if !(UNITY_WEBGL && !UNITY_EDITOR)
            if (!_isFallbackNoticed)
            {
                _isFallbackNoticed = true;
                Debug.LogWarning("TTAssetBundle 仅在 WebGL 方案有优化效果，当前环境下回滚到 UnityWebRequestAssetBundle 加载实现。");
            }
#endif
            
            _contentBytes = _contentBytes != null ? _contentBytes : Array.Empty<byte>();
            Array.Resize(ref _contentBytes, _contentBytes.Length + dataLength);
            data.CopyTo(_contentBytes, _contentLength);
            _contentLength += dataLength;
            return true;
        }

        protected override void CompleteContent() => isDone = true;
        
    }
}