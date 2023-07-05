//
//  GEPresetProperties+GEDisProperties.m
//  GravityEngineSDK
//
//

#import "GEPresetProperties+GEDisProperties.h"

static BOOL _ge_disableOpsReceiptProperties;
static BOOL _ge_disableStartReason;
static BOOL _ge_disableDisk;
static BOOL _ge_disableRAM;
static BOOL _ge_disableFPS;
static BOOL _ge_disableSimulator;
static BOOL _ge_disableAppVersion;
static BOOL _ge_disableOsVersion;
static BOOL _ge_disableManufacturer;
static BOOL _ge_disableDeviceModel;
static BOOL _ge_disableScreenHeight;
static BOOL _ge_disableScreenWidth;
static BOOL _ge_disableCarrier;
static BOOL _ge_disableDeviceId;
static BOOL _ge_disableSystemLanguage;
static BOOL _ge_disableLib;
static BOOL _ge_disableLibVersion;
static BOOL _ge_disableBundleId;
static BOOL _ge_disableOs;
static BOOL _ge_disableInstallTime;
static BOOL _ge_disableDeviceType;
static BOOL _ge_disableSessionID;
static BOOL _ge_disableCalibratedTime;

static BOOL _ge_disableNetworkType;
static BOOL _ge_disableZoneOffset;
static BOOL _ge_disableDuration;
static BOOL _ge_disableBackgroundDuration;
static BOOL _ge_disableAppCrashedReason;
static BOOL _ge_disableResumeFromBackground;
static BOOL _ge_disableElementId;
static BOOL _ge_disableElementType;
static BOOL _ge_disableElementContent;
static BOOL _ge_disableElementPosition;
static BOOL _ge_disableElementSelector;
static BOOL _ge_disableScreenName;
static BOOL _ge_disableTitle;
static BOOL _ge_disableUrl;
static BOOL _ge_disableReferrer;


// 推送事件名
static const NSString *kTDPushInfo  = @"ops_push_click";

// - 禁用功能并过滤字段拼接
static const NSString *kTDStartReason  = @"$start_reason";
static const NSString *kGEPerformanceRAM  = @"$ram";
static const NSString *kGEPerformanceDISK = @"$disk";
static const NSString *kGEPerformanceSIM  = @"$simulator";
static const NSString *kGEPerformanceFPS  = @"$fps";
static const NSString *kTDPresentAppVersion  = @"$app_version";
static const NSString *kTDPresentOsVersion = @"$os_version";
static const NSString *kTDPresentManufacturer  = @"$manufacturer";
static const NSString *kTDPresentDeviceModel  = @"$device_model";
static const NSString *kTDPresentScreenHeight  = @"$screen_height";
static const NSString *kTDPresentScreenWidth  = @"$screen_width";
static const NSString *kTDPresentCarrier  = @"$carrier";
static const NSString *kTDPresentDeviceId  = @"$device_id";
static const NSString *kTDPresentSystemLanguage  = @"$system_language";
static const NSString *kTDPresentLib  = @"$lib";
static const NSString *kTDPresentLibVersion  = @"$lib_version";
static const NSString *kTDPresentOs  = @"$os";
static const NSString *kTDPresentBundleId  = @"$bundle_id";
static const NSString *kTDPresentInstallTime  = @"$install_time";
static const NSString *kTDPresentDeviceType = @"$device_type";
static const NSString *kTDPresentSessionID  = @"$session_id";
static const NSString *kTDPresentCalibratedTime = @"$time_calibration";


// - 只过滤字段
static const NSString *kTDPresentNETWORKTYPE = @"$network_type";
static const NSString *kTDPresentZONEOFFSET = @"$zone_offset";
static const NSString *kTDPresentDURATION = @"$event_duration";
static const NSString *kTDPresentBACKGROUNDDURATION = @"$background_duration";
static const NSString *kTDPresentCRASHREASON = @"$app_crashed_reason";
static const NSString *kTDPresentRESUMEFROMBACKGROUND = @"$resume_from_background";
static const NSString *kTDPresentELEMENTID = @"$element_id";
static const NSString *kTDPresentELEMENTTYPE = @"$element_type";
static const NSString *kTDPresentELEMENTCONTENT = @"$element_content";
static const NSString *kTDPresentELEMENTPOSITION = @"$element_position";
static const NSString *kTDPresentELEMENTSELECTOR = @"$element_selector";
static const NSString *kTDPresentSCREENNAME = @"$screen_name";
static const NSString *kTDPresentTITLE = @"$title";
static const NSString *kTDPresentURL = @"$url";
static const NSString *kTDPresentREFERRER = @"$referrer";
static const NSString *kTDPresentOpsReceiptProperties = @"ops_receipt_properties";



#define GE_MAIM_INFO_PLIST_DISPRESTPRO_KEY @"GEDisPresetProperties"

@implementation GEPresetProperties (GEDisProperties)

static NSMutableArray *__ge_disPresetProperties;

+ (NSArray*)disPresetProperties {
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        
        NSArray *disPresetProperties = (NSArray *)[[[NSBundle mainBundle] infoDictionary] objectForKey:GE_MAIM_INFO_PLIST_DISPRESTPRO_KEY];

        if (disPresetProperties && disPresetProperties.count) {
            __ge_disPresetProperties = [NSMutableArray arrayWithArray:disPresetProperties];
            
            if ([__ge_disPresetProperties containsObject:kTDPresentZONEOFFSET]) {
                [__ge_disPresetProperties removeObject:kTDPresentZONEOFFSET];
            }

            _ge_disableStartReason = [__ge_disPresetProperties containsObject:kTDStartReason];
//            _ge_disableDisk        = [__ge_disPresetProperties containsObject:kGEPerformanceDISK];
//            _ge_disableRAM         = [__ge_disPresetProperties containsObject:kGEPerformanceRAM];
//            _ge_disableFPS         = [__ge_disPresetProperties containsObject:kGEPerformanceFPS];
            _ge_disableSimulator   = [__ge_disPresetProperties containsObject:kGEPerformanceSIM];
            
            _ge_disableAppVersion  = [__ge_disPresetProperties containsObject:kTDPresentAppVersion];
            _ge_disableOsVersion   = [__ge_disPresetProperties containsObject:kTDPresentOsVersion];
            _ge_disableManufacturer = [__ge_disPresetProperties containsObject:kTDPresentManufacturer];
            _ge_disableDeviceModel = [__ge_disPresetProperties containsObject:kTDPresentDeviceModel];
            _ge_disableScreenHeight = [__ge_disPresetProperties containsObject:kTDPresentScreenHeight];
            _ge_disableScreenWidth = [__ge_disPresetProperties containsObject:kTDPresentScreenWidth];
            _ge_disableCarrier = [__ge_disPresetProperties containsObject:kTDPresentCarrier];
            _ge_disableDeviceId = [__ge_disPresetProperties containsObject:kTDPresentDeviceId];
            _ge_disableSystemLanguage = [__ge_disPresetProperties containsObject:kTDPresentSystemLanguage];
            _ge_disableLib = [__ge_disPresetProperties containsObject:kTDPresentLib];
            _ge_disableLibVersion = [__ge_disPresetProperties containsObject:kTDPresentLibVersion];
            _ge_disableBundleId = [__ge_disPresetProperties containsObject:kTDPresentBundleId];
            _ge_disableOs = [__ge_disPresetProperties containsObject:kTDPresentOs];
            _ge_disableInstallTime = [__ge_disPresetProperties containsObject:kTDPresentInstallTime];
            _ge_disableDeviceType = [__ge_disPresetProperties containsObject:kTDPresentDeviceType];
            //_ge_disableSessionID = [__ge_disPresetProperties containsObject:kTDPresentSessionID];
            //_ge_disableCalibratedTime = [__ge_disPresetProperties containsObject:kTDPresentCalibratedTime];
            _ge_disableSessionID = YES;
            _ge_disableCalibratedTime = YES;

            
            _ge_disableNetworkType = [__ge_disPresetProperties containsObject:kTDPresentNETWORKTYPE];
            _ge_disableZoneOffset = [__ge_disPresetProperties containsObject:kTDPresentZONEOFFSET];
            _ge_disableDuration = [__ge_disPresetProperties containsObject:kTDPresentDURATION];
            _ge_disableBackgroundDuration = [__ge_disPresetProperties containsObject:kTDPresentBACKGROUNDDURATION];
            _ge_disableAppCrashedReason = [__ge_disPresetProperties containsObject:kTDPresentCRASHREASON];
            _ge_disableResumeFromBackground = [__ge_disPresetProperties containsObject:kTDPresentRESUMEFROMBACKGROUND];
            _ge_disableElementId = [__ge_disPresetProperties containsObject:kTDPresentELEMENTID];
            _ge_disableElementType = [__ge_disPresetProperties containsObject:kTDPresentELEMENTTYPE];
            _ge_disableElementContent = [__ge_disPresetProperties containsObject:kTDPresentELEMENTCONTENT];
            _ge_disableElementPosition = [__ge_disPresetProperties containsObject:kTDPresentELEMENTPOSITION];
            _ge_disableElementSelector = [__ge_disPresetProperties containsObject:kTDPresentELEMENTSELECTOR];
            _ge_disableScreenName = [__ge_disPresetProperties containsObject:kTDPresentSCREENNAME];
            _ge_disableTitle = [__ge_disPresetProperties containsObject:kTDPresentTITLE];
            _ge_disableUrl = [__ge_disPresetProperties containsObject:kTDPresentURL];
            _ge_disableReferrer = [__ge_disPresetProperties containsObject:kTDPresentREFERRER];
            _ge_disableOpsReceiptProperties = [__ge_disPresetProperties containsObject:kTDPresentOpsReceiptProperties];
            
            
        }
    });
    return __ge_disPresetProperties;
}


+ (void)handleFilterDisPresetProperties:(NSMutableDictionary *)dataDic
{
    if (!__ge_disPresetProperties || !__ge_disPresetProperties.count) {
        return ;
    }
    NSArray *propertykeys = dataDic.allKeys;
    NSArray *registerkeys = [GEPresetProperties disPresetProperties];
    NSMutableSet *set1 = [NSMutableSet setWithArray:propertykeys];
    NSMutableSet *set2 = [NSMutableSet setWithArray:registerkeys];
    [set1 intersectSet:set2];// 求交集
    if (!set1.allObjects.count) {
        return ;
    }
    [dataDic removeObjectsForKeys:set1.allObjects];
    return ;
}


+ (BOOL)disableOpsReceiptProperties {
    return  _ge_disableOpsReceiptProperties;
}

+ (BOOL)disableStartReason {
    return _ge_disableStartReason;
}

+ (BOOL)disableDisk {
    return _ge_disableDisk;
}

+ (BOOL)disableRAM {
    return _ge_disableRAM;
}

+ (BOOL)disableFPS {
    return _ge_disableFPS;
}

+ (BOOL)disableSimulator {
    return _ge_disableSimulator;
}




+ (BOOL)disableAppVersion {
    return _ge_disableAppVersion;
}

+ (BOOL)disableOsVersion {
    return _ge_disableOsVersion;
}

+ (BOOL)disableManufacturer {
    return _ge_disableManufacturer;
}

+ (BOOL)disableDeviceId {
    return _ge_disableDeviceId;
}

+ (BOOL)disableDeviceModel {
    return _ge_disableDeviceModel;
}

+ (BOOL)disableScreenHeight {
    return _ge_disableScreenHeight;
}

+ (BOOL)disableScreenWidth {
    return _ge_disableScreenWidth;
}

+ (BOOL)disableCarrier {
    return _ge_disableCarrier;
}

+ (BOOL)disableSystemLanguage {
    return _ge_disableSystemLanguage;
}

+ (BOOL)disableLib {
    return _ge_disableLib;
}

+ (BOOL)disableLibVersion {
    return _ge_disableLibVersion;
}

+ (BOOL)disableOs {
    return _ge_disableOs;
}

+ (BOOL)disableBundleId {
    return _ge_disableBundleId;
}

+ (BOOL)disableInstallTime {
    return _ge_disableInstallTime;
}

+ (BOOL)disableDeviceType {
    return _ge_disableDeviceType;
}

+ (BOOL)disableNetworkType {
    return _ge_disableNetworkType;
}

+ (BOOL)disableZoneOffset {
    return _ge_disableZoneOffset;
}

+ (BOOL)disableDuration {
    return _ge_disableDuration;
}

+ (BOOL)disableBackgroundDuration {
    return _ge_disableBackgroundDuration;
}

+ (BOOL)disableAppCrashedReason {
    return _ge_disableAppCrashedReason;
}

+ (BOOL)disableResumeFromBackground {
    return _ge_disableResumeFromBackground;
}

+ (BOOL)disableElementId {
    return _ge_disableElementId;
}

+ (BOOL)disableElementType {
    return _ge_disableElementType;
}

+ (BOOL)disableElementContent {
    return _ge_disableElementContent;
}

+ (BOOL)disableElementPosition {
    return _ge_disableElementPosition;
}

+ (BOOL)disableElementSelector {
    return _ge_disableElementSelector;
}

+ (BOOL)disableScreenName {
    return _ge_disableScreenName;
}

+ (BOOL)disableTitle {
    return _ge_disableTitle;
}

+ (BOOL)disableUrl {
    return _ge_disableUrl;
}

+ (BOOL)disableReferrer {
    return _ge_disableReferrer;
}

+ (BOOL)disableSessionID {
    return _ge_disableSessionID;
}

+ (BOOL)disableCalibratedTime {
    return _ge_disableCalibratedTime;
}

@end
