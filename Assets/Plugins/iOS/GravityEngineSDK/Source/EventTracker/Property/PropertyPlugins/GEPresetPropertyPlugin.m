//
//  GEPresetPropertyPlugin.m
//  GravityEngineSDK
//
//

#import "GEPresetPropertyPlugin.h"
#import "GEPresetProperties.h"
#import "GEPresetProperties+GEDisProperties.h"
#import "GEDeviceInfo.h"
#import "GEReachability.h"
#import "NSDate+GEFormat.h"

@interface GEPresetPropertyPlugin ()
@property (nonatomic, strong) NSMutableDictionary<NSString *, id> *properties;

@end

@implementation GEPresetPropertyPlugin

- (instancetype)init
{
    self = [super init];
    if (self) {
        self.properties = [NSMutableDictionary dictionary];
    }
    return self;
}

- (void)start {
    if (![GEPresetProperties disableAppVersion]) {
        self.properties[@"$app_version"] = [GEDeviceInfo sharedManager].appVersion;
    }
    if (![GEPresetProperties disableBundleId]) {
        self.properties[@"$bundle_id"] = [GEDeviceInfo bundleId];
    }
        
    if (![GEPresetProperties disableInstallTime]) {
        NSString *timeString = [[GEDeviceInfo ge_getInstallTime] ge_formatWithTimeZone:self.defaultTimeZone formatString: @"yyyy-MM-dd HH:mm:ss"];
        if (timeString && [timeString isKindOfClass:[NSString class]] && timeString.length){
            self.properties[@"$install_time"] = timeString;
        }
    }
}

- (void)asyncGetPropertyCompletion:(GEPropertyPluginCompletion)completion {
    NSMutableDictionary *mutableDict = [NSMutableDictionary dictionary];
    
    [mutableDict addEntriesFromDictionary:[[GEDeviceInfo sharedManager] getAutomaticData]];
    
    if (![GEPresetProperties disableNetworkType]) {
        mutableDict[@"$network_type"] = [[GEReachability shareInstance] networkState];
    }
    
    if (completion) {
        completion(mutableDict);
    }
}

@end
