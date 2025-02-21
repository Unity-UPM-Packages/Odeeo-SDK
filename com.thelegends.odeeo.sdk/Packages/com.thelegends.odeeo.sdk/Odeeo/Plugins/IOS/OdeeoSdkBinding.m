#import <OdeeoSDK/OdeeoSDK-Swift.h>
#import "OdeeoSdkAdListener.h"
#import "OdeeoSdkManagerListener.h"
#import "UnityAppController.h"

#pragma mark - Helpers

// Converts C style string to NSString
#define GetStringParam(_x_) ((_x_) != NULL ? [NSString stringWithUTF8String:_x_] : [NSString stringWithUTF8String:""])
#define GetNullableStringParam(_x_) ((_x_) != NULL ? [NSString stringWithUTF8String:_x_] : nil)
#define UIColorFromRGB(rgbValue) [UIColor colorWithRed:((float)((rgbValue & 0xFF0000) >> 16))/255.0 green:((float)((rgbValue & 0xFF00) >> 8))/255.0 blue:((float)(rgbValue & 0xFF))/255.0 alpha:1.0]
static char* odeeoStringCopy(NSString* input){
    const char* string = [input UTF8String];
    return string ? strdup(string) : NULL;
}

UIViewController * unityViewController()
{
    return UnityGetGLViewController() ?: UnityGetMainWindow().rootViewController ?: [[UIApplication sharedApplication].keyWindow rootViewController];
}

#pragma mark - Initialization

void _odeeoSdkInitialize(const char* appKey){
    [Odeeo initialize:GetStringParam(appKey)];
}

OdeeoSdkManagerListener* _odeeoSdkSetOnInitializationListener(POTypeCallbackClientRef * listener,
                                                              OdeeoSdkNoArgsDelegateNative onInitializationSuccess,
                                                              OdeeoSdkInitializationErrorDelegateNative onInitializationFailed)
{
    OdeeoSdkManagerListener* newDelegate = [[OdeeoSdkManagerListener alloc]
                                            initWithListenersOnInitialization:listener
                                            with:onInitializationSuccess
                                            and:onInitializationFailed];
    CFBridgingRetain(newDelegate);
    [Odeeo setDelegate:newDelegate];
    return newDelegate;
}

bool _odeeoSdkIsInitialized(){
    return [Odeeo isInitialized];
}

#pragma mark - Regulation

void _odeeoSdkSetIsChildDirected(bool flag){
    [Odeeo setIsChildDirected:flag];
}

void _odeeoSdkRequestTrackingAuthorization(){
    [Odeeo requestTrackingAuthorization];
}

// Regulation Type
void _odeeoSdkForceRegulationType(int type){
    [Odeeo forceRegulationType:type];
}

void _odeeoSdkClearForceRegulationType(){
    [Odeeo clearForceRegulationType];
}

int _odeeoSdkGetRegulationType(){
    return (int)[Odeeo getRegulationType];
}

//DoNotSell
void _odeeoSdkSetDoNotSellPrivacyString(const char* privacyString){
    [Odeeo setDoNotSellPrivacyString:GetStringParam(privacyString)];
}

void _odeeoSdkSetDoNotSell(bool isApplied){
    [Odeeo setDoNotSell:isApplied];
}

void _odeeoSdkSetDoNotSellWithString(bool isApplied, const char* privacyString){
    [Odeeo setDoNotSell:isApplied withConsentString:GetStringParam(privacyString)];
}

#pragma mark - AdUnit

CFTypeRef _odeeoSdkCreateAudioAdUnit(int adType, const char* placementID, OdeeoSdkAdListener* listener){
    AdUnit* adUnit = [[AdUnit alloc] init:adType placementID:GetStringParam(placementID) delegate:listener];
        
    UIViewController *rootViewController = unityViewController();
    [rootViewController.view addSubview:adUnit];
    
    return CFBridgingRetain(adUnit);
}


void _odeeoSdkDestroyBridgeReference(CFTypeRef adUnit){
    CFRelease( adUnit );
}

void _odeeoSdkSetRewardedPopupType(AdUnit* ad, int type){
    [ad setRewardedPopupType:type];
}

void _odeeoSdkSetRewardedPopupBannerPosition(AdUnit* ad, int position){
    [ad setRewardedPopupBannerPosition:position];
}

void _odeeoSdkSetRewardedPopupIconPosition(AdUnit* ad, int position, int xOffset, int yOffset){
    [ad setRewardedPopupIconPosition:position xOffset:xOffset yOffset:yOffset];
}

bool _odeeoSdkIsAdAvailable(AdUnit* ad){
    return [ad isAdAvailable];
}

bool _odeeoSdkIsAdCached(AdUnit* ad){
    return [ad isAdCached];
}

void _odeeoSdkSetCustomTag(AdUnit* ad,  const char* tag){
    [ad setCustomTag:GetStringParam(tag)];
}

void _odeeoSdkRemoveAd(AdUnit* ad){
    [ad removeAd];
}

void _odeeoSdkShow(AdUnit* ad){
    if ( !ad.window.rootViewController )
    {
      [ad removeFromSuperview];
      UIViewController *rootViewController = unityViewController();
      [rootViewController.view addSubview:ad];
    }
    
    [ad showAd];
}

void _odeeoSdkSetIconPosition(AdUnit* ad, int position, int xOffset, int yOffset){
    [ad setIconPosition:position xOffset:xOffset yOffset:yOffset];
}

void _odeeoSdkSetIconSize(AdUnit* ad, int size){
    [ad setIconSize:size];
}

void _odeeoSdkSetBannerPosition(AdUnit* ad, int position){
    [ad setBannerPosition:position];
}

OdeeoSdkAdListener* _odeeoSdkSetListeners(AdUnit* ad,
                                      POTypeCallbackClientRef * adListener,
                                      OdeeoSdkStateDataDelegateNative onAvailabilityChange,
                                      OdeeoSdkNoArgsDelegateNative onShow,
                                      OdeeoSdkErrorDelegateNative onShowFailed,
                                      OdeeoSdkIntDelegateNative onClose,
                                      OdeeoSdkNoArgsDelegateNative onClick,
                                      OdeeoSdkFloatDelegateNative onReward,
                                      OdeeoSdkDataDelegateNative onImpression,
                                      OdeeoSdkNoArgsDelegateNative onRewardedPopupAppear,
                                      OdeeoSdkIntDelegateNative onRewardedPopupClosed,
                                      OdeeoSdkIntDelegateNative onPause,
                                      OdeeoSdkIntDelegateNative onResume,
                                      OdeeoSdkStateDelegateNative onMute)
{
    OdeeoSdkAdListener* newDelegate = [
        [OdeeoSdkAdListener alloc] initWithListeners:adListener
        onAvailabilityChange:onAvailabilityChange
        onShow:onShow
        onShowFailed:onShowFailed
        onClose:onClose
        onClick:onClick
        onReward:onReward
        onImpression:onImpression
        onRewardedPopupAppear:onRewardedPopupAppear
        onRewardedPopupClosed:onRewardedPopupClosed
        onPause:onPause
        onResume:onResume
        onMute:onMute
        ];
    
    CFBridgingRetain(newDelegate);
    return newDelegate;
}

void _odeeoSdkSetAudioOnlyAnimationColor(AdUnit* ad, const char* color){
    unsigned result = 0;
    NSScanner *scannerColor = [NSScanner scannerWithString:GetStringParam(color)];
    [scannerColor setScanLocation:1]; // bypass '#' character

    [scannerColor scanHexInt:&result];
    UIColor* resultColor = UIColorFromRGB(result);

    [ad setAudioOnlyAnimationColor:resultColor];
}

void _odeeoSdkSetAudioOnlyBackgroundColor(AdUnit* ad, const char* color){
    unsigned result = 0;
    NSScanner *scannerColor = [NSScanner scannerWithString:GetStringParam(color)];
    [scannerColor setScanLocation:1]; // bypass '#' character

    [scannerColor scanHexInt:&result];
    UIColor* resultColor = UIColorFromRGB(result);

    [ad setAudioOnlyBackgroundColor:resultColor];
}

void _odeeoSdkSetProgressBarColor(AdUnit* ad, const char* tint){
    unsigned result = 0;
    NSScanner *scannerTint = [NSScanner scannerWithString:GetStringParam(tint)];
    [scannerTint setScanLocation:1]; // bypass '#' character

    [scannerTint scanHexInt:&result];
    UIColor* tintColor = UIColorFromRGB(result);

    [ad setProgressBarColor:tintColor];
}

void _odeeoSdkSetIconActionButtonPosition(AdUnit* ad, int position){
    [ad setIconActionButtonPosition:position];
}

void _odeeoSdkTrackRewardedOffer(AdUnit* ad){
    [ad trackRewardedOffer];
}

void _odeeoSdkTrackAdShowBlocked(AdUnit* ad){
    [ad trackAdShowBlocked];
}

#pragma mark - Data

// ImpressionData
const char* _odeeoSdkImpressionDataGetString(ImpressionData *data){
    NSError* emptyError = [NSError alloc];
    NSData *jsonData = [NSJSONSerialization dataWithJSONObject:[data dictionaryWithValuesForKeys:@[@"sessionID", @"placementType", @"placementID", @"country", @"payableAmount", @"transactionID", @"customTag"]]
                                                       options:NSJSONWritingPrettyPrinted
                                                         error:&emptyError];
    
    NSString *jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
    
    return odeeoStringCopy(jsonString);
}

// AdData
const char* _odeeoSdkAdDataGetString(AdData *data){
    NSError* emptyError = [NSError alloc];
    NSData *jsonData = [NSJSONSerialization dataWithJSONObject:[data dictionaryWithValuesForKeys:@[@"sessionID", @"placementType", @"placementID", @"country", @"eCPM", @"transactionID", @"customTag"]]
                                                       options:NSJSONWritingPrettyPrinted
                                                         error:&emptyError];
    
    NSString *jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
    
    return odeeoStringCopy(jsonString);
}

#pragma mark - General

void _odeeoSdkSetLogLevel(int level){
    [Odeeo setLogLevel:level];
}

void _odeeoSdkAddToMutableArray(NSMutableArray* ar, int item){
    NSNumber* num = [NSNumber numberWithInt:item];
    [ar addObject:num];
}

NSMutableArray* _odeeoSdkCreateMutableArray(){
    return [[NSMutableArray alloc] init];
}

void _odeeoSdkSetEngineInfo(const char* engineName, const char* engineVersion){
    [Odeeo setEngineInfo:GetStringParam(engineName) withVersion:GetStringParam(engineVersion)];
}

void _odeeoSdkAddCustomAttribute(const char* key, const char* value){
    [Odeeo addCustomAttribute:GetStringParam(key) withValue:GetStringParam(value)];
}

void _odeeoSdkClearCustomAttributes(){
    [Odeeo clearCustomAttributes];
}

void _odeeoSdkRemoveCustomAttribute(const char*  key){
    [Odeeo removeCustomAttributeWithKey:GetStringParam(key)];
}

const char* _odeeoSdkGetCustomAttributes(){
    NSError* emptyError = [NSError alloc];
    NSArray* cAttrs = [Odeeo getCustomAttributes];
    NSData *jsonData = [NSJSONSerialization dataWithJSONObject:cAttrs options:0 error:&emptyError];
    NSString *jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
    
    return odeeoStringCopy(jsonString);
}

const char* _odeeoSdkGetCustomAttributesWithKey(const char* key){
    NSError* emptyError = [NSError alloc];
    NSArray* cAttrs = [Odeeo getCustomAttributes:GetStringParam(key)];
    NSData *jsonData = [NSJSONSerialization dataWithJSONObject:cAttrs options:0 error:&emptyError];
    NSString *jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
    
    return odeeoStringCopy(jsonString);
}

const char* _odeeoSdkGetPublisherUserID(){
    return odeeoStringCopy([Odeeo getPublisherUserID]);
}

void _odeeoSdkSetPublisherUserID(const char* value){
    [Odeeo setPublisherUserID:GetStringParam(value)];
}

void _odeeoSdkSetExtendedUserID(const char* partner, const char* value){
    [Odeeo setExtendedUserID:GetStringParam(partner) :GetStringParam(value)];
}

float _odeeoSdkGetDeviceVolumeLevel(){
    return [[Odeeo getDeviceVolumeLevel] floatValue];
}

float _odeeoSdkGetDeviceScale(){
    return [UIScreen mainScreen].nativeScale;
}

void _odeeoSdkPause(){
    [Odeeo onPause];
}

void _odeeoSdkResume(){
    [Odeeo onResume];
}
