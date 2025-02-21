using System;
using Odeeo.Scripts.Proxy;
using UnityEngine;

namespace Odeeo.Proxy
{
    public interface IOdeeoAdProxy : IOdeeoAdUnitCallbacks, IDisposable
    {
        bool IsAdBlocked { get; set; }
        string PlacementId { get; }
        OdeeoSdk.PlacementType PlacementType { get; }
        
        void ShowAd();
        void RemoveAd();
        bool IsAdAvailable();
        bool IsAdCached();
        void SetBannerPosition(OdeeoSdk.BannerPosition position);
        void SetRewardedPopupType(OdeeoAdUnit.PopUpType type);
        void SetRewardedPopupBannerPosition(OdeeoSdk.BannerPosition position);
        void SetRewardedPopupIconPosition(OdeeoSdk.IconPosition position, int xOffset, int yOffset);
        void SetProgressBarColor(Color progressBarColor);
        void SetAudioOnlyBackgroundColor(Color color);
        void SetAudioOnlyAnimationColor(Color color);
        void SetIconActionButtonPosition(OdeeoAdUnit.ActionButtonPosition position);
        void SetCustomTag(string tag);
        void TrackRewardedOffer();
        void TrackAdShowBlocked();
        void DispatchOnShowError(OdeeoAdUnit.ErrorShowReason reason, string customMessage);
    }
    
    internal interface IOdeeoAdProxy<out T> : IOdeeoAdProxy
    {
        T AdoptedObject { get; }
    }
}