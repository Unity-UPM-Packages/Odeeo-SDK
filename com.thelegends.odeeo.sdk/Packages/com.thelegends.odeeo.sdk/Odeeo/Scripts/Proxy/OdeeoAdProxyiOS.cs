#if UNITY_IOS
using System;
using System.Runtime.InteropServices;
using AOT;
using Odeeo.Data;
using Odeeo.Platforms.Serialization;
using Odeeo.Utils;
using UnityEngine;

namespace Odeeo.Proxy
{
    internal sealed class OdeeoAdProxyiOS : IOdeeoAdProxy
    {
        private delegate void OdeeoSdkDelegateNative(IntPtr client);
        private delegate void OdeeoSdkDelegateNative<in T>(IntPtr client, T flag);
        private delegate void OdeeoSdkDelegateNative<in T1, in T2>(IntPtr client, T1 flag, T2 data);
        private delegate void OdeeoSdkDelegateNative<in T1, in T2, in T3>(IntPtr client, T1 data1, T2 data2, T3 data3);
        
        public bool IsAdBlocked { get; set; }
        public string PlacementId => _config.PlacementId;
        public OdeeoSdk.PlacementType PlacementType => _config.PlacementType;
        
        public event Action<bool, OdeeoAdData> OnAvailabilityChanged;
        public event Action OnShow;
        public event Action<OdeeoAdUnit.CloseReason> OnClose;
        public event Action OnClick;
        public event Action<float> OnReward;
        public event Action<OdeeoImpressionData> OnImpression;
        public event Action OnRewardedPopupAppear;
        public event Action<OdeeoAdUnit.CloseReason> OnRewardedPopupClosed;
        public event Action<OdeeoAdUnit.StateChangeReason> OnPause;
        public event Action<OdeeoAdUnit.StateChangeReason> OnResume;
        public event Action<bool> OnMute;
        public event Action<string, OdeeoAdUnit.ErrorShowReason, string> OnShowFailed;
        
        private readonly IntPtr _adNativeListenerRef;
        private readonly IntPtr _client;
        
        private readonly OdeeoAdUnitConfig _config;
        private readonly IOdeeoLogging _logging;

        internal OdeeoAdProxyiOS(OdeeoAdUnitConfig config, IOdeeoLogging logging)
        {
            _config = config;
            _logging = logging;
            
            _adNativeListenerRef = _odeeoSdkSetListeners(
                _client, 
                (IntPtr)GCHandle.Alloc(this),
                OnAvailabilityChangedNative,
                OnShowNative,
                OnShowFailedNative,
                OnCloseNative,
                OnClickNative,
                OnRewardNative,
                OnImpressionNative,
                OnRewardedPopupAppearNative,
                OnRewardedPopupClosedNative,
                OnPauseNative,
                OnResumeNative,
                OnMuteNative
            );
            
            _client = _odeeoSdkCreateAudioAdUnit((int)PlacementType, PlacementId, _adNativeListenerRef);
        }
        
        public void ShowAd()
        {
            SetPositionAndSize();
            
            _odeeoSdkShow(_client);
        }

        public void RemoveAd()
        {
            _odeeoSdkRemoveAd(_client);
        }

        public bool IsAdAvailable()
        {
            return _odeeoSdkIsAdAvailable(_client);
        }

        public bool IsAdCached()
        {
            return _odeeoSdkIsAdCached(_client);
        }

        public void SetBannerPosition(OdeeoSdk.BannerPosition position)
        {
            _odeeoSdkSetBannerPosition(_client, (int)position);
        }

        public void SetRewardedPopupType(OdeeoAdUnit.PopUpType type)
        {
            _odeeoSdkSetRewardedPopupType(_client, (int)type);
        }

        public void SetRewardedPopupBannerPosition(OdeeoSdk.BannerPosition position)
        {
            _odeeoSdkSetRewardedPopupBannerPosition(_client, (int)position);
        }

        public void SetRewardedPopupIconPosition(OdeeoSdk.IconPosition position, int xOffset, int yOffset)
        {
            _odeeoSdkSetRewardedPopupIconPosition(_client, (int)position, xOffset, yOffset);
        }

        public void SetProgressBarColor(Color progressBarColor)
        {
            _odeeoSdkSetProgressBarColor(_client, "#"+ColorUtility.ToHtmlStringRGB(progressBarColor));
        }

        public void SetAudioOnlyBackgroundColor(Color color)
        {
            _odeeoSdkSetAudioOnlyBackgroundColor(_client, "#"+ColorUtility.ToHtmlStringRGB(color));
        }

        public void SetAudioOnlyAnimationColor(Color color)
        {
            _odeeoSdkSetAudioOnlyAnimationColor(_client, "#"+ColorUtility.ToHtmlStringRGB(color));
        }

        public void SetIconActionButtonPosition(OdeeoAdUnit.ActionButtonPosition position)
        {
            _odeeoSdkSetIconActionButtonPosition(_client, (int)position);
        }
        
        public void SetCustomTag(string tag)
        {
            _odeeoSdkSetCustomTag(_client, tag);
        }

        public void TrackRewardedOffer()
        {
            _odeeoSdkTrackRewardedOffer(_client);
        }

        public void TrackAdShowBlocked()
        {
            _odeeoSdkTrackAdShowBlocked(_client);
        }

        public void DispatchOnShowError(OdeeoAdUnit.ErrorShowReason reason, string customMessage)
        {
            string message = string.IsNullOrEmpty(customMessage) ? _config.ErrorMessageBy(reason) : customMessage;
            OnShowFailed?.Invoke(PlacementId, reason, message);
        }

        public void Dispose()
        {
            _odeeoSdkDestroyBridgeReference(_client);
            _odeeoSdkDestroyBridgeReference(_adNativeListenerRef);
        }
        
        private void SetPositionAndSize()
        {
            if (OdeeoAdManager.IsBannerType(PlacementType))
            {
                _odeeoSdkSetBannerPosition(_client, (int)_config.Position);
                return;
            }
            
            _odeeoSdkSetIconPosition(_client, (int)_config.Position, _config.Offset.x, _config.Offset.y);

            int iconSize = (_config.Size.x + _config.Size.y) / 2;
            _odeeoSdkSetIconSize(_client, iconSize);
        }
        
        [MonoPInvokeCallback(typeof(OdeeoSdkDelegateNative<bool, IntPtr>))]
        private static void OnAvailabilityChangedNative(IntPtr client, bool flag, IntPtr data)
        {
            OdeeoAdProxyiOS listener = IntPtrToClient(client);

            bool availabilityStatus = flag;
            if (listener.IsAdBlocked)
                availabilityStatus = listener.IsAdBlocked;

            OdeeoAdData adData = IntPtrToAdData(data);

            OdeeoMainThreadDispatcher.Instance.Enqueue(() => listener.OnAvailabilityChanged?.Invoke(availabilityStatus, adData));
        }

        [MonoPInvokeCallback(typeof(OdeeoSdkDelegateNative))]
        private static void OnShowNative(IntPtr client)
        {
            OdeeoAdProxyiOS listener = IntPtrToClient(client);
            OdeeoMainThreadDispatcher.Instance.Enqueue(() => listener.OnShow?.Invoke());
        }

        [MonoPInvokeCallback(typeof(OdeeoSdkDelegateNative<string, int, string>))]
        private static void OnShowFailedNative(IntPtr client, string placementId, int reason, string description)
        {
            OdeeoAdProxyiOS listener = IntPtrToClient(client);
            OdeeoMainThreadDispatcher.Instance.Enqueue(() => listener.OnShowFailed?.Invoke(placementId, (OdeeoAdUnit.ErrorShowReason)reason, description));
        }

        [MonoPInvokeCallback(typeof(OdeeoSdkDelegateNative<int>))]
        private static void OnCloseNative(IntPtr client, int result)
        {
            OdeeoAdProxyiOS listener = IntPtrToClient(client);
            OdeeoMainThreadDispatcher.Instance.Enqueue(() => listener.OnClose?.Invoke((OdeeoAdUnit.CloseReason)result));
        }
        
        [MonoPInvokeCallback(typeof(OdeeoSdkDelegateNative))]
        private static void OnClickNative(IntPtr client)
        {
            OdeeoAdProxyiOS listener = IntPtrToClient(client);
            OdeeoMainThreadDispatcher.Instance.Enqueue( () => listener.OnClick?.Invoke() );
        }
        
        [MonoPInvokeCallback(typeof(OdeeoSdkDelegateNative<float>))]
        private static void OnRewardNative(IntPtr client, float amount)
        {
            OdeeoAdProxyiOS listener = IntPtrToClient(client);
            OdeeoMainThreadDispatcher.Instance.Enqueue( () => listener.OnReward?.Invoke(amount) );
        }
        
        [MonoPInvokeCallback(typeof(OdeeoSdkDelegateNative<IntPtr>))]
        private static void OnImpressionNative(IntPtr client, IntPtr data)
        {
            OdeeoAdProxyiOS listener = IntPtrToClient(client);
            OdeeoImpressionData impressionData = IntPtrToImpressionData(data);
            OdeeoMainThreadDispatcher.Instance.Enqueue( () => listener.OnImpression?.Invoke(impressionData) );
        }
        
        [MonoPInvokeCallback(typeof(OdeeoSdkDelegateNative))]
        private static void OnRewardedPopupAppearNative(IntPtr client)
        {
            OdeeoAdProxyiOS listener = IntPtrToClient(client);
            OdeeoMainThreadDispatcher.Instance.Enqueue( () => listener.OnRewardedPopupAppear?.Invoke() );
        }
        
        [MonoPInvokeCallback(typeof(OdeeoSdkDelegateNative<int>))]
        private static void OnRewardedPopupClosedNative(IntPtr client, int result)
        {
            OdeeoAdProxyiOS listener = IntPtrToClient(client);
            OdeeoMainThreadDispatcher.Instance.Enqueue(() => listener.OnRewardedPopupClosed?.Invoke((OdeeoAdUnit.CloseReason)result));
        }
        
        [MonoPInvokeCallback(typeof(OdeeoSdkDelegateNative<int>))]
        private static void OnPauseNative(IntPtr client, int result)
        {
            OdeeoAdProxyiOS listener = IntPtrToClient(client);
            OdeeoMainThreadDispatcher.Instance.Enqueue(() => listener.OnPause?.Invoke((OdeeoAdUnit.StateChangeReason)result));
        }
        
        [MonoPInvokeCallback(typeof(OdeeoSdkDelegateNative<int>))]
        private static void OnResumeNative(IntPtr client, int result)
        {
            OdeeoAdProxyiOS listener = IntPtrToClient(client);
            OdeeoMainThreadDispatcher.Instance.Enqueue(() => listener.OnResume?.Invoke((OdeeoAdUnit.StateChangeReason)result));
        }
        
        [MonoPInvokeCallback(typeof(OdeeoSdkDelegateNative))]
        private static void OnMuteNative(IntPtr client, bool flag)
        {
            OdeeoAdProxyiOS listener = IntPtrToClient(client);
            OdeeoMainThreadDispatcher.Instance.Enqueue( () => listener.OnMute?.Invoke(flag) );
        }
        
        private static OdeeoAdProxyiOS IntPtrToClient(IntPtr cl)
        {
            GCHandle handle = (GCHandle)cl;
            return handle.Target as OdeeoAdProxyiOS;
        }
        
        private static OdeeoAdData IntPtrToAdData(IntPtr ptr)
        {
            string dataString = _odeeoSdkAdDataGetString(ptr);
            OdeeoAdDataDto data = JsonUtility.FromJson<OdeeoAdDataDto>(dataString);
            
            _odeeoSdkDestroyBridgeReference(ptr);

            return new OdeeoAdData(data);
        }
        
        private static OdeeoImpressionData IntPtrToImpressionData(IntPtr ptr)
        {
            string dataString = _odeeoSdkImpressionDataGetString(ptr);
            OdeeoImpressionDataDto data = JsonUtility.FromJson<OdeeoImpressionDataDto>(dataString);
        
            _odeeoSdkDestroyBridgeReference(ptr);

            return new OdeeoImpressionData(data);
        }
        
        [DllImport("__Internal")]
        private static extern string _odeeoSdkImpressionDataGetString(IntPtr obj);
        [DllImport("__Internal")]
        private static extern string _odeeoSdkAdDataGetString(IntPtr obj);
        [DllImport("__Internal")]
        private static extern IntPtr _odeeoSdkCreateAudioAdUnit(int adType, string placementID, IntPtr listener);
        [DllImport("__Internal")]
        private static extern void _odeeoSdkShow(IntPtr client);
        [DllImport("__Internal")]
        private static extern void _odeeoSdkRemoveAd(IntPtr client);
        [DllImport("__Internal")]
        private static extern void _odeeoSdkSetIconPosition(IntPtr client, int position, int xOffset, int yOffset);
        [DllImport("__Internal")]
        private static extern void _odeeoSdkSetIconSize(IntPtr client, int size);
        [DllImport("__Internal")]
        private static extern void _odeeoSdkSetBannerPosition(IntPtr client, int position);
        [DllImport("__Internal")]
        private static extern bool _odeeoSdkIsAdAvailable(IntPtr client);
        [DllImport("__Internal")]
        private static extern bool _odeeoSdkIsAdCached(IntPtr client);
        
        [DllImport("__Internal")]
        private static extern IntPtr _odeeoSdkSetListeners(
            IntPtr client,
            IntPtr callbackRef,
            OdeeoSdkDelegateNative<bool, IntPtr> onAvailabilityChange,
            OdeeoSdkDelegateNative onShow,
            OdeeoSdkDelegateNative<string, int, string> onShowFailed,
            OdeeoSdkDelegateNative<int> onClose,
            OdeeoSdkDelegateNative onClick,
            OdeeoSdkDelegateNative<float> onReward,
            OdeeoSdkDelegateNative<IntPtr> onImpression,
            OdeeoSdkDelegateNative onRewardedPopupAppear,
            OdeeoSdkDelegateNative<int> onRewardedPopupClosed,
            OdeeoSdkDelegateNative<int> onPause,
            OdeeoSdkDelegateNative<int> onResume,
            OdeeoSdkDelegateNative<bool> onMute
            );
        
        [DllImport("__Internal")]
        private static extern void _odeeoSdkSetRewardedPopupType(IntPtr client, int type);
        [DllImport("__Internal")]
        private static extern void _odeeoSdkSetRewardedPopupBannerPosition(IntPtr client, int position);
        [DllImport("__Internal")]
        private static extern void _odeeoSdkSetRewardedPopupIconPosition(IntPtr client, int position, int xOffset, int yOffset);
        [DllImport("__Internal")]
        private static extern void _odeeoSdkSetAudioOnlyAnimationColor(IntPtr client, string color);
        [DllImport("__Internal")]
        private static extern void _odeeoSdkSetAudioOnlyBackgroundColor(IntPtr client, string color);
        [DllImport("__Internal")]
        private static extern void _odeeoSdkSetProgressBarColor(IntPtr client, string tint);
        [DllImport("__Internal")]
        private static extern void _odeeoSdkSetIconActionButtonPosition(IntPtr client, int position);
        [DllImport("__Internal")]
        private static extern void _odeeoSdkSetCustomTag(IntPtr client, string tag);
        [DllImport("__Internal")]
        private static extern void _odeeoSdkDestroyBridgeReference(IntPtr obj);
        [DllImport("__Internal")]
        private static extern void _odeeoSdkTrackRewardedOffer(IntPtr obj);
        [DllImport("__Internal")]
        private static extern void _odeeoSdkTrackAdShowBlocked(IntPtr obj);
    }
}
#endif