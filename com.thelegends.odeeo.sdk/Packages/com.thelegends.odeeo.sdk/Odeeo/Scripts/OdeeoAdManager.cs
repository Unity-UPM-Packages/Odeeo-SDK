using System.Collections.Generic;
using Odeeo.Logging;
using Odeeo.Scripts.Proxy;
using UnityEngine;

namespace Odeeo
{
    public class OdeeoAdManager : MonoBehaviour
    {
        private static readonly Dictionary<string, OdeeoAdUnit> s_adUnits = new Dictionary<string, OdeeoAdUnit>();
        private static readonly IOdeeoLogging _logging = new OdeeoLogging();

        #region Creation

        public static void CreateAudioBannerAd(string placementID)
        {
            CreateAd(OdeeoSdk.PlacementType.AudioBannerAd, placementID);
        }

        public static void CreateAudioIconAd(string placementID)
        {
            CreateAd(OdeeoSdk.PlacementType.AudioIconAd, placementID);
        }

        public static void CreateRewardedAudioBannerAd(string placementID)
        {
            CreateAd(OdeeoSdk.PlacementType.RewardedAudioBannerAd, placementID);
        }

        public static void CreateRewardedAudioIconAd(string placementID)
        {
            CreateAd(OdeeoSdk.PlacementType.RewardedAudioIconAd, placementID);
        }

        private static void CreateAd(OdeeoSdk.PlacementType adType, string placementID)
        {
            if (!OdeeoSdk.IsInitialized())
            {
                _logging.Error("Creation Audio Ad failed. OdeeoSDK is not Initialized.");
                return;
            }
            
            if (s_adUnits.ContainsKey(placementID))
            {
                _logging.Warning("Creation Audio Ad failed, this placement already exists.");
                return;
            }

            var adUnit = new OdeeoAdUnit(adType, placementID);
            s_adUnits.Add(placementID, adUnit);
        }

        #endregion

        #region General
        
        public static IOdeeoAdUnitCallbacks AdUnitCallbacks(string placementID)
        {
            if (s_adUnits.TryGetValue(placementID, out var unit)) 
                return unit.AdCallbacks;
            
            _logging.Error("AdUnitCallbacks failed, placement doesn't exists");
            return null;
        }

        public static void ShowAd(string placementID)
        {
            if (!s_adUnits.TryGetValue(placementID, out var unit))
            {
                _logging.Error("ShowAd failed, placement doesn't exists");
                return;
            }

            unit.ShowAd();
        }

        public static void RemoveAd(string placementID)
        {
            if (!s_adUnits.TryGetValue(placementID, out var unit))
                return;

            unit.RemoveAd();
        }
        
        public static void SetCustomTag(string placementID, string tag)
        {
            if (!s_adUnits.TryGetValue(placementID, out var unit))
            {
                _logging.Error("SetCustomTag failed, placement doesn't exists");
                return;
            }

            unit.SetCustomTag(tag);
        }

        public static void TrackRewardedOffer(string placementID)
        {
            if (!s_adUnits.TryGetValue(placementID, out var unit))
            {
                _logging.Error("TrackRewardedOffer failed, placement doesn't exists");
                return;
            }

            unit.TrackRewardedOffer();
        }

        #endregion

        #region Positioning
        
        public static bool IsRectTransformBlocked(string placementId)
        {
            if (!IsPlacementExist(placementId))
                return false;

            return s_adUnits[placementId].IsRectTransformBlocked();
        }
        
        public static void SetIconPosition(string placementID, OdeeoSdk.IconPosition iconPosition, int xOffset, int yOffset)
        {
            if (!s_adUnits.TryGetValue(placementID, out var unit))
            {
                _logging.Error("SetIconPosition failed, placement doesn't exists");
                return;
            }

            unit.SetIconPosition(iconPosition, xOffset, yOffset);
        }

        public static void SetBannerPosition(string placementID, OdeeoSdk.BannerPosition position)
        {
            if (!s_adUnits.TryGetValue(placementID, out var unit))
            {
                _logging.Error("SetBannerPosition failed, placement doesn't exists");
                return;
            }

            unit.SetBannerPosition(position);
        }

        public static void LinkToIconAnchor(string placementID, OdeeoIconAnchor iconAnchor)
        {
            if (!s_adUnits.TryGetValue(placementID, out var unit))
            {
                _logging.Error("LinkToIconAnchor failed, placement doesn't exists");
                return;
            }

            unit.LinkToIconAnchor(iconAnchor);
        }

        public static void LinkIconToRectTransform(string placementID, OdeeoSdk.IconPosition iconPosition,
            RectTransform rectTransform, Canvas canvas,
            OdeeoSdk.AdSizingMethod sizingMethod = OdeeoSdk.AdSizingMethod.Flexible)
        {
            if (!s_adUnits.TryGetValue(placementID, out var unit))
            {
                _logging.Error("LinkIconToRectTransform failed, placement doesn't exists");
                return;
            }

            unit.LinkIconToRectTransform(iconPosition, rectTransform, canvas, sizingMethod);
        }

        #endregion

        #region VisualSettings
        
        public static void SetIconSize(string placementID, int size)
        {
            if (!s_adUnits.TryGetValue(placementID, out var unit))
            {
                _logging.Error("SetIconSize failed, placement doesn't exists");
                return;
            }

            unit.SetIconSize(size);
        }

        public static void SetAudioOnlyBackground(string placementID, Color color)
        {
            if (!s_adUnits.TryGetValue(placementID, out var unit))
            {
                _logging.Error("SetAudioOnlyBackgroundColor failed, placement doesn't exists");
                return;
            }

            unit.SetAudioOnlyBackgroundColor(color);
        }

        public static void SetAudioOnlyAnimation(string placementID, Color color)
        {
            if (!s_adUnits.TryGetValue(placementID, out var unit))
            {
                _logging.Error("SetAudioOnlyAnimationColor failed, placement doesn't exists");
                return;
            }

            unit.SetAudioOnlyAnimationColor(color);
        }

        public static void SetProgressBar(string placementID, Color progressBarColor)
        {
            if (!s_adUnits.TryGetValue(placementID, out var unit))
            {
                _logging.Error("SetVisualisationColors failed, placement doesn't exists");
                return;
            }

            unit.SetProgressBarColor(progressBarColor);
        }

        #endregion

        #region RewardSettings

        public static void SetRewardedPopupType(string placementID, OdeeoAdUnit.PopUpType type)
        {
            if (!s_adUnits.TryGetValue(placementID, out var unit))
            {
                _logging.Error("SetRewardedPopupType failed, placement doesn't exists");
                return;
            }

            unit.SetRewardedPopupType(type);
        }

        public static void SetRewardedPopupBannerPosition(string placementID, OdeeoSdk.BannerPosition bannerPosition)
        {
            if (!s_adUnits.TryGetValue(placementID, out var unit))
            {
                _logging.Error("SetRewardedPopupBannerPosition failed, placement doesn't exists");
                return;
            }
            
            unit.SetRewardedPopupBannerPosition(bannerPosition);
        }
        
        public static void SetRewardedPopupIconPosition(string placementID, OdeeoSdk.IconPosition iconPosition, int xOffset,
            int yOffset)
        {
            if (!s_adUnits.TryGetValue(placementID, out var unit))
            {
                _logging.Error("SetRewardedPopupIconPosition failed, placement doesn't exists");
                return;
            }

            unit.SetRewardedPopupIconPosition(iconPosition, xOffset, yOffset);
        }

        #endregion

        #region ActionButtonSettings

        public static void SetIconActionButtonPosition(string placementID, OdeeoAdUnit.ActionButtonPosition position)
        {
            if (!s_adUnits.TryGetValue(placementID, out var unit))
            {
                _logging.Error("SetIconActionButtonPosition failed, placement doesn't exists");
                return;
            }
            
            unit.SetIconActionButtonPosition(position);
        }

        #endregion

        #region Verification

        public static bool IsPlacementExist(string placementId)
        {
            return s_adUnits.ContainsKey(placementId);
        }

        public static bool IsAdAvailable(string placementId)
        {
            if (!IsPlacementExist(placementId))
                return false;

            return s_adUnits[placementId].IsAdAvailable();
        }

        public static bool IsAdCached(string placementId)
        {
            if (!IsPlacementExist(placementId))
                return false;

            return s_adUnits[placementId].IsAdCached();
        }

        public static bool IsAdPlaying(string placementId)
        {
            if (!IsPlacementExist(placementId))
                return false;

            return s_adUnits[placementId].IsPlaying;
        }

        public static bool IsAnyAdPlaying()
        {
            foreach (KeyValuePair<string, OdeeoAdUnit> entry in s_adUnits)
            {
                if (entry.Value.IsPlaying)
                    return true;
            }

            return false;
        }

        public static bool IsIconType(OdeeoSdk.PlacementType adType)
        {
            return adType == OdeeoSdk.PlacementType.AudioIconAd || adType == OdeeoSdk.PlacementType.RewardedAudioIconAd;
        }

        public static bool IsBannerType(OdeeoSdk.PlacementType adType)
        {
            return adType == OdeeoSdk.PlacementType.AudioBannerAd ||
                   adType == OdeeoSdk.PlacementType.RewardedAudioBannerAd;
        }

        public static bool IsBannerType(OdeeoAdUnit.PopUpType popUpType)
        {
            return popUpType == OdeeoAdUnit.PopUpType.BannerPopUp;
        }

        public static bool IsAdRewardedType(OdeeoSdk.PlacementType adType)
        {
            return adType == OdeeoSdk.PlacementType.RewardedAudioIconAd ||
                   adType == OdeeoSdk.PlacementType.RewardedAudioBannerAd;
        }

        #endregion

        #region Internal

        internal static OdeeoAdUnit GetCurrentPlayingAd()
        {
            foreach (KeyValuePair<string, OdeeoAdUnit> entry in s_adUnits)
            {
                if (entry.Value.IsPlaying)
                    return entry.Value;
            }

            return null;
        }

        internal static bool TryGetCurrentAdBy(string placementId, out OdeeoAdUnit unit)
        {
            return s_adUnits.TryGetValue(placementId, out unit);
        }

        internal static int GetCurrentPlayingAdCount()
        {
            int count = 0;
            
            foreach (KeyValuePair<string, OdeeoAdUnit> entry in s_adUnits)
            {
                if (entry.Value.IsPlaying)
                    count++;
            }
            
            return count;
        }

        #endregion
    }
}