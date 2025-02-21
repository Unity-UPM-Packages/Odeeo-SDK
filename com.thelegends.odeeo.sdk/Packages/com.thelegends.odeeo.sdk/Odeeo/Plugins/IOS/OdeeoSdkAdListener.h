#import <OdeeoSDK/OdeeoSDK-Swift.h>

typedef const void *POTypeCallbackClientRef;
typedef const void *POTypeCallbackImpressionDataRef;

typedef void (*OdeeoSdkNoArgsDelegateNative) (POTypeCallbackClientRef* callback);
typedef void (*OdeeoSdkInitializationErrorDelegateNative)(POTypeCallbackClientRef* callback, NSInteger, const char *);
typedef void (*OdeeoSdkStateDelegateNative) (POTypeCallbackClientRef* callback, BOOL flag);
typedef void (*OdeeoSdkFloatDelegateNative) (POTypeCallbackClientRef* callback, float value);
typedef void (*OdeeoSdkDataDelegateNative) (POTypeCallbackClientRef* callback, CFTypeRef data);
typedef void (*OdeeoSdkStateDataDelegateNative) (POTypeCallbackClientRef* callback, BOOL flag, CFTypeRef data);
typedef void (*OdeeoSdkIntDelegateNative) (POTypeCallbackClientRef* callback, int value);
typedef void (*OdeeoSdkErrorDelegateNative) (POTypeCallbackClientRef* callback, char* placementId, int value, char* description);

@interface OdeeoSdkAdListener: NSObject <AdUnitDelegate> {
    OdeeoSdkNoArgsDelegateNative _onShowCallback;
    OdeeoSdkErrorDelegateNative _onShowFailedCallback;
    OdeeoSdkIntDelegateNative _onCloseCallback;
    OdeeoSdkNoArgsDelegateNative _onClickCallback;
    OdeeoSdkStateDataDelegateNative _onAvailabilityChangedCallback;
    OdeeoSdkFloatDelegateNative _onRewardCallback;
    OdeeoSdkDataDelegateNative _onImpressionCallback;
    OdeeoSdkNoArgsDelegateNative _onRewardedPopupAppearCallback;
    OdeeoSdkIntDelegateNative _onRewardedPopupClosedCallback;
    OdeeoSdkIntDelegateNative _onPauseCallback;
    OdeeoSdkIntDelegateNative _onResumeCallback;
    OdeeoSdkStateDelegateNative _onMuteCallback;
    
    POTypeCallbackClientRef* _clientRef;
    POTypeCallbackImpressionDataRef* _impressionDataRef;
}

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
                           onMute:(OdeeoSdkStateDelegateNative)onMuteRef;

- (void)onAvailabilityChangedWithState:(BOOL)flag data:(AdData *)adData;
- (void)onShow;
- (void)onShowFailedWithPlacementID:(NSString *)placementID :(int)reason description:(NSString *)description;
- (void)onClose:(int)resultData;
- (void)onClick;
- (void)onReward:(float)amount;
- (void)onImpression:(ImpressionData *)impressionData;
- (void)onRewardedPopupAppear;
- (void)onRewardedPopupClosed:(int)resultData;
- (void)onPause:(int)resultData;
- (void)onResume:(int)resultData;
- (void)onMute:(BOOL)flag;

@end
