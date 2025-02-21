#import "OdeeoSdkManagerListener.h"

@implementation OdeeoSdkManagerListener 

static char* odeeoStringCopy(NSString* input){
    const char* string = [input UTF8String];
    return string ? strdup(string) : NULL;
}

-(instancetype) initWithListenersOnInitialization:(POTypeCallbackClientRef* )client with:(OdeeoSdkNoArgsDelegateNative)onInitializationSuccessRef and:(OdeeoSdkInitializationErrorDelegateNative)onInitializationFailedRef
{
    self = [super init];
    _clientRef = client;
    _onInitializationSuccessCallback = onInitializationSuccessRef;
    _onInitializationFailedCallback = onInitializationFailedRef;
    return self;
}

#pragma mark - PlayOnManagerListener

- (void)onInitializationFailedWithErrorCode:(NSInteger)errorCode errorMessage:(NSString * _Nonnull)errorMessage {
    const char * message = odeeoStringCopy(errorMessage);
    _onInitializationFailedCallback(_clientRef, errorCode, message);
}

- (void)onInitializationSucceed { 
  _onInitializationSuccessCallback(_clientRef);
}


@end
