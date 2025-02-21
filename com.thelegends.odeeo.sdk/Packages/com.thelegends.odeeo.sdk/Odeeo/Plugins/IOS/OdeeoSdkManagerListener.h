#import <OdeeoSDK/OdeeoSDK-Swift.h>
#import "OdeeoSdkAdListener.h"

@interface OdeeoSdkManagerListener: NSObject <OdeeoDelegate> {
    OdeeoSdkNoArgsDelegateNative _onInitializationSuccessCallback;
    OdeeoSdkInitializationErrorDelegateNative _onInitializationFailedCallback;
    POTypeCallbackClientRef* _clientRef;
}

-(instancetype) initWithListenersOnInitialization:(POTypeCallbackClientRef* )client with:(OdeeoSdkNoArgsDelegateNative)onInitializationSuccessRef and:(OdeeoSdkInitializationErrorDelegateNative)onInitializationFailedRef;

- (void)onInitializationSucceed;
- (void)onInitializationFailedWithErrorCode:(NSInteger)errorCode errorMessage:(NSString * _Nonnull)errorMessage;


@end
