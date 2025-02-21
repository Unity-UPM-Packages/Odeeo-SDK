using System;
using Odeeo.Data;

namespace Odeeo.Scripts.Proxy
{
    public interface IOdeeoAdUnitCallbacks
    {
        event Action<bool, OdeeoAdData> OnAvailabilityChanged;
        event Action<string, OdeeoAdUnit.ErrorShowReason, string> OnShowFailed;
        event Action<OdeeoImpressionData> OnImpression;
        event Action OnShow;
        event Action OnClick;
        event Action<bool> OnMute;
        
        event Action OnRewardedPopupAppear;
        event Action<OdeeoAdUnit.CloseReason> OnRewardedPopupClosed;
        event Action<float> OnReward;
        
        event Action<OdeeoAdUnit.CloseReason> OnClose;
        event Action<OdeeoAdUnit.StateChangeReason> OnPause;
        event Action<OdeeoAdUnit.StateChangeReason> OnResume;
    }
}