#import "GEConfig.h"

#import "GENetwork.h"
#import "GravityEngineSDKPrivate.h"
#import "GESecurityPolicy.h"
#import "GEFile.h"
#import "NSString+GEString.h"

#define TDSDKSETTINGS_PLIST_SETTING_IMPL(TYPE, PLIST_KEY, GETTER, SETTER, DEFAULT_VALUE, ENABLE_CACHE) \
static TYPE *g_##PLIST_KEY = nil; \
+ (TYPE *)GETTER \
{ \
  if (!g_##PLIST_KEY && ENABLE_CACHE) { \
    g_##PLIST_KEY = [[[NSUserDefaults standardUserDefaults] objectForKey:@#PLIST_KEY] copy]; \
  } \
  if (!g_##PLIST_KEY) { \
    g_##PLIST_KEY = [[[NSBundle mainBundle] objectForInfoDictionaryKey:@#PLIST_KEY] copy] ?: DEFAULT_VALUE; \
  } \
  return g_##PLIST_KEY; \
} \
+ (void)SETTER:(TYPE *)value { \
  g_##PLIST_KEY = [value copy]; \
  if (ENABLE_CACHE) { \
    if (value) { \
      [[NSUserDefaults standardUserDefaults] setObject:value forKey:@#PLIST_KEY]; \
    } else { \
      [[NSUserDefaults standardUserDefaults] removeObjectForKey:@#PLIST_KEY]; \
    } \
  } \
}


#define kTAConfigInfo @"TAConfigInfo"


static GEConfig * _defaultGEConfig;
static NSDictionary *configInfo;

@implementation GEConfig

TDSDKSETTINGS_PLIST_SETTING_IMPL(NSNumber, GravityEngineSDKMaxCacheSize, _maxNumEventsNumber, _setMaxNumEventsNumber, @10000, NO);
TDSDKSETTINGS_PLIST_SETTING_IMPL(NSNumber, GravityEngineSDKExpirationDays, _expirationDaysNumber, _setExpirationDaysNumber, @10, NO);

+ (GEConfig *)defaultGEConfig {
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        _defaultGEConfig = [GEConfig new];
        
    });
    return _defaultGEConfig;
}

- (instancetype)init {
    self = [super init];
    if (self) {
        _trackRelaunchedInBackgroundEvents = NO;
        _autoTrackEventType = GravityEngineEventTypeNone;
        _networkTypePolicy = GravityNetworkTypeWIFI | GravityNetworkType3G  | GravityNetworkType4G | GravityNetworkType2G | GravityNetworkType5G;
        _securityPolicy = [GESecurityPolicy defaultPolicy];
        _defaultTimeZone = [NSTimeZone localTimeZone];
        _configureURL = @"https://backend.gravity-engine.com";
    
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Wdeprecated-declarations"
        if (!configInfo) {
            configInfo = (NSDictionary *)[[[NSBundle mainBundle] infoDictionary] objectForKey: kTAConfigInfo];
        }
        
        if (configInfo && [configInfo.allKeys containsObject:@"maxNumEvents"]) {
            [GEConfig setMaxNumEvents:[configInfo[@"maxNumEvents"] integerValue]];
        }
        if (configInfo && [configInfo.allKeys containsObject:@"expirationDays"]) {
            [GEConfig setExpirationDays:[configInfo[@"expirationDays"] integerValue]];
        }
#pragma clang diagnostic pop

    }
    return self;
}

- (void)setNetworkType:(GravityEngineNetworkType)type {
    if (type == GENetworkTypeDefault) {
        _networkTypePolicy = GravityNetworkTypeWIFI | GravityNetworkType3G | GravityNetworkType4G | GravityNetworkType2G | GravityNetworkType5G;
    } else if (type == GENetworkTypeOnlyWIFI) {
        _networkTypePolicy = GravityNetworkTypeWIFI;
    } else if (type == GENetworkTypeALL) {
        _networkTypePolicy = GravityNetworkTypeWIFI | GravityNetworkType3G | GravityNetworkType4G | GravityNetworkType2G | GravityNetworkType5G;
    }
}

#pragma mark - NSCopying
- (id)copyWithZone:(NSZone *)zone {
    GEConfig *config = [[[self class] allocWithZone:zone] init];
    config.trackRelaunchedInBackgroundEvents = self.trackRelaunchedInBackgroundEvents;
    config.autoTrackEventType = self.autoTrackEventType;
    config.networkTypePolicy = self.networkTypePolicy;
    config.launchOptions = [self.launchOptions copyWithZone:zone];
    config.debugMode = self.debugMode;
    config.securityPolicy = [self.securityPolicy copyWithZone:zone];
    config.defaultTimeZone = [self.defaultTimeZone copyWithZone:zone];
    config.name = [self.name copy];
    config.enableEncrypt = self.enableEncrypt;
#if TARGET_OS_IOS
    config.secretKey = [self.secretKey copyWithZone:zone];
#endif
    
    return config;
}

#pragma mark - SETTINGS
+ (NSInteger)maxNumEvents {
    NSInteger maxNumEvents = [self _maxNumEventsNumber].integerValue;
    if (maxNumEvents < 5000) {
        maxNumEvents = 5000;
    }
    return maxNumEvents;
}

+ (void)setMaxNumEvents:(NSInteger)maxNumEventsNumber {
    [self _setMaxNumEventsNumber:@(maxNumEventsNumber)];
}

+ (NSInteger)expirationDays {
    NSInteger maxNumEvents = [self _expirationDaysNumber].integerValue;
    return maxNumEvents >= 0 ? maxNumEvents : 10;
}

+ (void)setExpirationDays:(NSInteger)expirationDays {
    [self _setExpirationDaysNumber:@(expirationDays)];
}


- (NSString *)getMapInstanceToken {
    return self.appid;
}

@end
