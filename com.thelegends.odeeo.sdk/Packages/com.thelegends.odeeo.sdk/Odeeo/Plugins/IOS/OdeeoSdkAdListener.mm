#import "OdeeoSdkAdListener.h"

@implementation OdeeoSdkAdListener

-(instancetype) initWithListeners:(POTypeCallbackClientRef* )client
             onAvailabilityChange:(OdeeoSdkStateDataDelegateNative)onAvailabilityChangeRef
                           onShow:(OdeeoSdkNoArgsDelegateNative)onShowRef
                     onShowFailed:(OdeeoSdkErrorDelegateNative)onShowFailedRef
                          onClose:(OdeeoSdkIntDelegateNative)onCloseRef
                          onClick:(OdeeoSdkNoArgsDelegateNative)onClickRef
                         onReward:(OdeeoSdkFloatDelegateNative)onRewardRef
                     onImpression:(OdeeoSdkDataDelegateNative)onImpressionRef
            onRewardedPopupAppear:(OdeeoSdkNoArgsDelegateNative)onRewardedPopupAppearRef
            onRewardedPopupClosed:(OdeeoSdkIntDelegateNative)onRewardedPopupClosedRef
                          onPause:(OdeeoSdkIntDelegateNative)onPauseRef
                         onResume:(OdeeoSdkIntDelegateNative)onResumeRef
                           onMute:(OdeeoSdkStateDelegateNative)onMuteRef
                     
{
    self = [super init];
    
    _clientRef = client;
    _onAvailabilityChangedCallback = onAvailabilityChangeRef;
    _onShowCallback = onShowRef;
    _onShowFailedCallback = onShowFailedRef;
    _onCloseCallback = onCloseRef;
    _onClickCallback = onClickRef;
    _onRewardCallback = onRewardRef;
    _onImpressionCallback = onImpressionRef;
    _onRewardedPopupAppearCallback = onRewardedPopupAppearRef;
    _onRewardedPopupClosedCallback = onRewardedPopupClosedRef;
    _onPauseCallback = onPauseRef;
    _onResumeCallback = onResumeRef;
    _onMuteCallback = onMuteRef;
    
    return self;
}

#pragma mark - AdListener

static char* odeeoStringCopy(NSString* input){
    const char* string = [input UTF8String];
    return string ? strdup(string) : NULL;
}

- (void)onAvailabilityChangedWithState:(BOOL)flag data:(AdData * _Nullable)adData {
    _onAvailabilityChangedCallback(_clientRef, flag, CFBridgingRetain(adData));
}

-(void) onShow{
    _onShowCallback(_clientRef);
}

-(void) onShowFailedWithPlacementID:(NSString *)placementID :(int)reason description:(NSString *)description {
    _onShowFailedCallback(_clientRef, odeeoStringCopy(placementID), reason, odeeoStringCopy(description));
}

-(void) onClose:(int)resultData {
    _onCloseCallback(_clientRef, resultData);
}

-(void) onClick {
    _onClickCallback(_clientRef);
}

- (void)onReward:(float)amount {
    _onRewardCallback(_clientRef, amount);
}

- (void)onImpression:(ImpressionData *)impressionData {
    _onImpressionCallback(_clientRef, CFBridgingRetain(impressionData));
}

-(void) onRewardedPopupAppear {
    _onRewardedPopupAppearCallback(_clientRef);
}

-(void) onRewardedPopupClosed:(int)resultData {
    _onRewardedPopupClosedCallback(_clientRef, resultData);
}

-(void) onPause:(int)resultData {
    _onPauseCallback(_clientRef, resultData);
}

-(void) onResume:(int)resultData {
    _onResumeCallback(_clientRef, resultData);
}

-(void) onMute:(BOOL)flag {
    _onMuteCallback(_clientRef, flag);
}

@end
