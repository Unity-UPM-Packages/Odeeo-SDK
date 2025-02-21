#if UNITY_ANDROID
using System;
using Odeeo.Data;
using Odeeo.Platforms.Serialization;
using Odeeo.Utils;
using UnityEngine;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local

namespace Odeeo.Proxy
{
    internal sealed class OdeeoAdProxyAndroid : AndroidJavaProxy, IOdeeoAdProxy
    {
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
        
        private readonly AndroidJavaObject _client;
        private readonly OdeeoAdUnitConfig _config;
        private readonly IOdeeoLogging _logging;

        internal OdeeoAdProxyAndroid(OdeeoAdUnitConfig config, IOdeeoLogging logging) 
            : base("io.odeeo.sdk.AdListener")
        {
            _config = config;
            _logging = logging;
            
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    using (AndroidJavaClass typeEnum = new AndroidJavaClass("io.odeeo.sdk.AdUnit$PlacementType"))
                    {
                        using (AndroidJavaObject curType = typeEnum.CallStatic<AndroidJavaObject>("valueOf", PlacementType.ToString()))
                        {
                            _client = new AndroidJavaObject("io.odeeo.sdk.AdUnit", activity, curType, this, PlacementId);
                        }
                    }
                }
            }
        }
        
        public void ShowAd()
        {
            SetPositionAndSize();
            
            _client.Call("showAd");
        }

        public void RemoveAd()
        {
            _client.Call("removeAd");
        }

        public bool IsAdAvailable()
        {
            return _client.Call<bool>("isAdAvailable");
        }

        public bool IsAdCached()
        {
            return _client.Call<bool>("isAdCached");
        }
        
        public void SetBannerPosition(OdeeoSdk.BannerPosition position)
        {
            using (AndroidJavaClass positionEnum = new AndroidJavaClass ("io.odeeo.sdk.AdPosition$BannerPosition"))
            {
                using (AndroidJavaObject currentPosition = positionEnum.CallStatic<AndroidJavaObject>("valueOf", position.ToString()))
                {
                    _client.Call("setBannerPosition", currentPosition);
                }
            }
        }

        public void SetRewardedPopupType(OdeeoAdUnit.PopUpType type)
        {
            using (AndroidJavaClass en = new AndroidJavaClass("io.odeeo.sdk.AdUnit$PopUpType"))
            {
                using (AndroidJavaObject curValue = en.CallStatic<AndroidJavaObject>("valueOf", type.ToString()))
                {
                    _client.Call("setRewardedPopUpType", curValue);
                }
            }
        }

        public void SetRewardedPopupBannerPosition(OdeeoSdk.BannerPosition position)
        {
            AndroidJavaClass en = new AndroidJavaClass("io.odeeo.sdk.AdPosition$BannerPosition");
            AndroidJavaObject curValue = en.CallStatic<AndroidJavaObject>("valueOf", position.ToString());
            _client.Call("setRewardedPopupBannerPosition", curValue);
        }

        public void SetRewardedPopupIconPosition(OdeeoSdk.IconPosition position, int xOffset, int yOffset)
        {
            AndroidJavaClass en = new AndroidJavaClass("io.odeeo.sdk.AdPosition$IconPosition");
            AndroidJavaObject curValue = en.CallStatic<AndroidJavaObject>("valueOf", position.ToString());
            _client.Call("setRewardedPopupIconPosition", curValue, xOffset, yOffset);
        }

        public void SetProgressBarColor(Color progressBarColor)
        {
            string hexProgressBarColor = ColorUtility.ToHtmlStringRGBA(progressBarColor);
            hexProgressBarColor = "#" + hexProgressBarColor.Substring(6) + hexProgressBarColor.Remove(6);

            _client.Call("setProgressBarColor", hexProgressBarColor);
        }

        public void SetAudioOnlyBackgroundColor(Color color)
        {
            string hexColor = ColorUtility.ToHtmlStringRGBA(color);
            hexColor = "#" + hexColor.Substring(6) + hexColor.Remove(6);
            
            _client.Call("setAudioOnlyBackgroundColor", hexColor);
        }

        public void SetAudioOnlyAnimationColor(Color color)
        {
            string hexColor = ColorUtility.ToHtmlStringRGBA(color);
            hexColor = "#" + hexColor.Substring(6) + hexColor.Remove(6);

            _client.Call("setAudioOnlyAnimationColor", hexColor);
        }

        public void SetIconActionButtonPosition(OdeeoAdUnit.ActionButtonPosition position)
        {
            AndroidJavaClass en = new AndroidJavaClass("io.odeeo.sdk.AdUnit$ActionButtonPosition");
            AndroidJavaObject curValue = en.CallStatic<AndroidJavaObject>("valueOf", position.ToString());
            _client.Call("setIconActionButtonPosition", curValue);
        }

        public void SetCustomTag(string tag)
        {
            _client.Call("setCustomTag", tag);
        }

        public void TrackRewardedOffer()
        {
            _client.Call("trackRewardedOffer");
        }

        public void TrackAdShowBlocked()
        {
            _client.Call("trackAdShowBlocked");
        }

        public void DispatchOnShowError(OdeeoAdUnit.ErrorShowReason reason, string customMessage)
        {
            string message = string.IsNullOrEmpty(customMessage) ? _config.ErrorMessageBy(reason) : customMessage;
            OnShowFailed?.Invoke(PlacementId, reason, message);
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
        
        private void SetPositionAndSize()
        {
            AndroidJavaClass posEnum;
            AndroidJavaObject curPos;
            
            if (OdeeoAdManager.IsBannerType(PlacementType))
            {
                posEnum = new AndroidJavaClass("io.odeeo.sdk.AdPosition$BannerPosition");
                curPos = posEnum.CallStatic<AndroidJavaObject>("valueOf", _config.Position.ToString());

                _client.Call("setBannerPosition", curPos);
                return;
            }
            
            posEnum = new AndroidJavaClass("io.odeeo.sdk.AdPosition$IconPosition");
            curPos = posEnum.CallStatic<AndroidJavaObject>("valueOf", _config.Position.ToString());

            _client.Call("setIconPosition", curPos, _config.Offset.x, _config.Offset.y);
            _client.Call("setIconSize", _config.Size.x);
        }
        
        // Android proxy methods. Do not rename unless native version is renamted.

        private void onAvailabilityChanged(bool availability, AndroidJavaObject dataObject)
        {
            bool availabilityStatus = availability;
            if (IsAdBlocked)
                availabilityStatus = IsAdBlocked;
            
            AndroidJavaObject type = dataObject.Get<AndroidJavaObject>("placementType");
            string stringType = type.Call<string>("toString");
            
            OdeeoSdk.PlacementType placementType = (OdeeoSdk.PlacementType)System.Enum.Parse(typeof(OdeeoSdk.PlacementType), stringType);
            string sessionID = dataObject.Get<string>("sessionID");
            string placementID = dataObject.Get<string>("placementID");
            string country = dataObject.Get<string>("country");
            double eCPM = dataObject.Get<double>("eCPM");
            string transactionID = dataObject.Get<string>("transactionID");
            string customTag = dataObject.Get<string>("customTag");
            
            OdeeoAdDataDto dto = new OdeeoAdDataDto()
            {
                sessionID = sessionID,
                placementType = (int)placementType, placementID = placementID, 
                country = country, eCPM = eCPM, transactionID = transactionID, customTag = customTag
            };
            
            OdeeoMainThreadDispatcher.Instance.Enqueue(() => OnAvailabilityChanged?.Invoke(availabilityStatus, new OdeeoAdData(dto)));
        }

        private void onShow()
        {
            OdeeoMainThreadDispatcher.Instance.Enqueue(() => OnShow?.Invoke());
        }

        private void onClose(AndroidJavaObject adResult)
        {
            int typeIndex = adResult.Call<int> ("ordinal");
            OdeeoAdUnit.CloseReason result = (OdeeoAdUnit.CloseReason)typeIndex;
            
            OdeeoMainThreadDispatcher.Instance.Enqueue(() => OnClose?.Invoke(result));
        }

        private void onClick()
        {
            OdeeoMainThreadDispatcher.Instance.Enqueue(() =>
            {
                OnClick?.Invoke();
            });
        }

        private void onReward(float amount)
        {
            OdeeoMainThreadDispatcher.Instance.Enqueue(() => OnReward?.Invoke(amount));
        }

        private void onImpression(AndroidJavaObject dataObject)
        {
            AndroidJavaObject type = dataObject.Get<AndroidJavaObject>("placementType");
            string stringType = type.Call<string>("toString");
            
            OdeeoSdk.PlacementType placementType = (OdeeoSdk.PlacementType)System.Enum.Parse(typeof(OdeeoSdk.PlacementType), stringType);
            
            string placementID = dataObject.Get<string>("placementID");
            string sessionID = dataObject.Get<string>("sessionID");
            string country = dataObject.Get<string>("country");
            string transactionID = dataObject.Get<string>("transactionID");
            double payableAmount = dataObject.Get<double>("payableAmount");
            string customTag = dataObject.Get<string>("customTag");

            OdeeoImpressionDataDto dto = new OdeeoImpressionDataDto()
            {
                sessionID = sessionID,
                placementType = (int)placementType, placementID = placementID, 
                country = country, payableAmount = payableAmount, transactionID = transactionID, customTag = customTag
            };
            
            OdeeoMainThreadDispatcher.Instance.Enqueue(() => OnImpression?.Invoke(new OdeeoImpressionData(dto)));
        }

        private void onRewardedPopupAppear()
        {
            OdeeoMainThreadDispatcher.Instance.Enqueue(() => OnRewardedPopupAppear?.Invoke());
        }

        private void onRewardedPopupClosed(AndroidJavaObject reason)
        {
            int reasonIndex = reason.Call<int> ("ordinal");
            OdeeoAdUnit.CloseReason closeReason = (OdeeoAdUnit.CloseReason)reasonIndex;
            
            OdeeoMainThreadDispatcher.Instance.Enqueue(() => OnRewardedPopupClosed?.Invoke(closeReason));
        }

        private void onPause(AndroidJavaObject reason)
        {
            int reasonIndex = reason.Call<int> ("ordinal");
            OdeeoAdUnit.StateChangeReason stateChangeReason = (OdeeoAdUnit.StateChangeReason)reasonIndex;
            
            OdeeoMainThreadDispatcher.Instance.Enqueue(() => OnPause?.Invoke(stateChangeReason));
        }

        private void onResume(AndroidJavaObject reason)
        {
            int reasonIndex = reason.Call<int> ("ordinal");
            OdeeoAdUnit.StateChangeReason stateChangeReason = (OdeeoAdUnit.StateChangeReason)reasonIndex;
            
            OdeeoMainThreadDispatcher.Instance.Enqueue(() => OnResume?.Invoke(stateChangeReason));
        }

        private void onMute(bool isMuted)
        {
            OdeeoMainThreadDispatcher.Instance.Enqueue(() => OnMute?.Invoke(isMuted));
        }

        private void onShowFailed(string placementId, AndroidJavaObject reason, string description)
        {
            int reasonIndex = reason.Call<int>("ordinal");
            OdeeoAdUnit.ErrorShowReason errorShowReason = (OdeeoAdUnit.ErrorShowReason)reasonIndex;
            
            OdeeoMainThreadDispatcher.Instance.Enqueue(() => OnShowFailed?.Invoke(placementId, errorShowReason, description));
        }
    }
}
#endif