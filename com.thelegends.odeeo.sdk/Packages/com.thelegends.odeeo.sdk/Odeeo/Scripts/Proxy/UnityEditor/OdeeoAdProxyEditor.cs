#if UNITY_EDITOR
using System;
using System.Collections;
using System.Threading.Tasks;
using Odeeo.Data;
using Odeeo.Proxy.AdUnit;
using Odeeo.Utils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Odeeo.Proxy
{
    public class OdeeoAdProxyEditor : IOdeeoAdProxy<OdeeoAdUnitSimulation>
    {
        public bool IsAdBlocked { get; set; }
        public string PlacementId => _config.PlacementId;
        public OdeeoSdk.PlacementType PlacementType => _config.PlacementType;

        OdeeoAdUnitSimulation IOdeeoAdProxy<OdeeoAdUnitSimulation>.AdoptedObject => _adUnit;

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
        
        private const string ADPrefabFilename = "OdeeoAd.prefab";

        private readonly OdeeoAdUnitConfig _config;
        private readonly IOdeeoLogging _logging;
        
        private OdeeoAdUnitSimulation _adUnit;

        internal OdeeoAdProxyEditor(OdeeoAdUnitConfig config, IOdeeoLogging logging)
        {
            _config = config;
            _logging = logging;
            
            CreateGameObject();
        }

        public void ShowAd()
        {
            _adUnit.SetPositionAndSize();
            
            if (OdeeoAdManager.IsAnyAdPlaying())
            {
                if (!OdeeoAdManager.TryGetCurrentAdBy(PlacementId, out var currentAddUnit))
                {
                    _logging.Error("Trying to show an add that wasn't created");
                    return;
                }
                
                if (OdeeoAdManager.GetCurrentPlayingAdCount() > 1)
                {
                    currentAddUnit.DispatchOnShowError(OdeeoAdUnit.ErrorShowReason.AnotherAdPlaying);
                    return;
                }
                
                IOdeeoAdProxy<OdeeoAdUnitSimulation> playingAd = (IOdeeoAdProxy<OdeeoAdUnitSimulation>) OdeeoAdManager.GetCurrentPlayingAd().AdCallbacks;
                bool isPlayAllowed = !OdeeoAdManager.IsAdRewardedType(playingAd.PlacementType) &&
                                     OdeeoAdManager.IsAdRewardedType(((IOdeeoAdProxy) currentAddUnit.AdCallbacks).PlacementType);

                if (!isPlayAllowed)
                {
                    currentAddUnit.DispatchOnShowError(currentAddUnit.IsPlaying
                        ? OdeeoAdUnit.ErrorShowReason.CurrentAdPlaying
                        : OdeeoAdUnit.ErrorShowReason.AnotherAdPlaying);
                    return;
                }
                
                playingAd.AdoptedObject.SetPause(true, OdeeoAdUnit.StateChangeReason.OtherOdeeoPlacementStart);

                void Unpause(OdeeoAdUnit.CloseReason reason)
                {
                    if(playingAd != null)
                        playingAd.AdoptedObject.SetPause(false, OdeeoAdUnit.StateChangeReason.OtherOdeeoPlacementEnd);
                    
                    currentAddUnit.AdCallbacks.OnClose -= Unpause;
                }

                currentAddUnit.AdCallbacks.OnClose += Unpause;
            }
            
            _adUnit.ShowAd();
        }

        public void RemoveAd()
        {
            if (!OdeeoAdManager.IsAdPlaying(_config.PlacementId))
                return;
            
            _adUnit.DestroyAd(OdeeoAdUnit.CloseReason.AdRemovedByDev);
        }

        public bool IsAdAvailable()
        {
            return _adUnit.IsAdAvailable();
        }

        public bool IsAdCached()
        {
            return _adUnit.IsAdCached();
        }

        public void SetBannerPosition(OdeeoSdk.BannerPosition position)
        {
            _adUnit.SetBannerPosition(position);
        }
        
        public void SetRewardedPopupBannerPosition(OdeeoSdk.BannerPosition position)
        {
            _adUnit.SetRewardedPopupBannerPosition(position);
        }

        public void SetRewardedPopupIconPosition(OdeeoSdk.IconPosition position, int xOffset, int yOffset)
        {
            _adUnit.SetRewardedPopupIconPosition(position, xOffset, yOffset);
        }

        public void SetRewardedPopupType(OdeeoAdUnit.PopUpType type)
        {
            _adUnit.SetRewardedPopupType(type);
        }

        public void SetProgressBarColor(Color progressBarColor)
        {
            _adUnit.SetProgressBarColor(progressBarColor);
        }

        public void SetAudioOnlyBackgroundColor(Color color)
        {
            _adUnit.SetAudioOnlyBackgroundColor(color);
        }

        public void SetAudioOnlyAnimationColor(Color color)
        {
            _adUnit.SetAudioOnlyAnimationColor(color);
        }

        public void SetIconActionButtonPosition(OdeeoAdUnit.ActionButtonPosition position)
        {
            _adUnit.SetIconActionButtonPosition(position);
        }

        public void SetCustomTag(string tag)
        {
            _adUnit.SetCustomTag(tag);
        }

        public void TrackRewardedOffer()
        {
            _adUnit.TrackRewardedOffer();
        }

        public void TrackAdShowBlocked()
        {
            _adUnit.TrackAdShowBlocked();
        }

        public void DispatchOnShowError(OdeeoAdUnit.ErrorShowReason reason, string customMessage)
        {
            _adUnit.DispatchOnShowError(reason, customMessage);
        }
        
        private void CreateGameObject()
        {
            string adPrefabPath = OdeeoEditorHelper.GetAssetBasedPath(ADPrefabFilename);
            if (string.IsNullOrEmpty(adPrefabPath))
            {
                _logging.Error($"Can't find {ADPrefabFilename} asset");
                return;
            }
            
            OdeeoAdUnitSimulation prefab = AssetDatabase.LoadAssetAtPath<OdeeoAdUnitSimulation>(adPrefabPath);
            prefab.gameObject.SetActive(false);
            _adUnit = Object.Instantiate(prefab, Vector3.zero, Quaternion.identity);
            _adUnit.gameObject.name = $"{prefab.name}_{_config.PlacementId}";
            
            Object.DontDestroyOnLoad(_adUnit.gameObject);
            
            _adUnit.OnAvailabilityChanged += OnAvailabilityChangedHandler;
            _adUnit.OnShow += OnShowHandler;
            _adUnit.OnClose += OnCloseHandler;
            _adUnit.OnClick += OnClickHandler;
            _adUnit.OnReward += OnRewardHandler;
            _adUnit.OnImpression += OnImpressionHandler;
            _adUnit.OnRewardedPopupAppear += OnRewardedPopupAppearHandler;
            _adUnit.OnRewardedPopupClosed += OnRewardedPopupClosedHandler;
            _adUnit.OnPause += OnPauseHandler;
            _adUnit.OnResume += OnResumeHandler;
            _adUnit.OnMute += OnMuteHandler;
            _adUnit.OnShowFailed += OnShowFailedHandler;
            
            _adUnit.Init(_config);
        }
        
        private void OnShowFailedHandler(string error, OdeeoAdUnit.ErrorShowReason reason, string tag)
        {
            OnShowFailed?.Invoke(error, reason, tag);
        }

        private void OnMuteHandler(bool mute)
        {
            OnMute?.Invoke(mute);
        }

        private void OnResumeHandler(OdeeoAdUnit.StateChangeReason reason)
        {
            OnResume?.Invoke(reason);
        }

        private void OnPauseHandler(OdeeoAdUnit.StateChangeReason reason)
        {
            OnPause?.Invoke(reason);
        }

        private void OnRewardedPopupClosedHandler(OdeeoAdUnit.CloseReason reason)
        {
            OnRewardedPopupClosed?.Invoke(reason);
        }

        private void OnRewardedPopupAppearHandler()
        {
            OnRewardedPopupAppear?.Invoke();
        }

        private void OnImpressionHandler(OdeeoImpressionData data)
        {
            OnImpression?.Invoke(data);
        }

        private void OnRewardHandler(float amount)
        {
            OnReward?.Invoke(amount);
        }

        private void OnClickHandler()
        {
            OnClick?.Invoke();
        }

        private void OnCloseHandler(OdeeoAdUnit.CloseReason reason)
        {
            OnClose?.Invoke(reason);
        }

        private void OnShowHandler()
        {
            OnShow?.Invoke();
        }

        private void OnAvailabilityChangedHandler(bool available, OdeeoAdData data)
        {
            OnAvailabilityChanged?.Invoke(available, data);
        }

        public void Dispose()
        {
            _logging.Info("Disposing...");
        }
    }
}
#endif