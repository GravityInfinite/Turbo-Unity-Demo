//
//  GEReachability.m
//  GravityEngineSDK
//
//

#import "GEReachability.h"
#import <SystemConfiguration/SystemConfiguration.h>
#import <CoreTelephony/CTTelephonyNetworkInfo.h>

#if __has_include(<GravityEngineSDK/GELogging.h>)
#import <GravityEngineSDK/GELogging.h>
#else
#import "GELogging.h"
#endif


@interface GEReachability ()
#if TARGET_OS_IOS
@property (atomic, assign) SCNetworkReachabilityRef reachability;
#endif
@property (nonatomic, assign) BOOL isWifi;
@property (nonatomic, assign) BOOL isWwan;

@end

@implementation GEReachability

#if TARGET_OS_IOS
static void GravityReachabilityCallback(SCNetworkReachabilityRef target, SCNetworkReachabilityFlags flags, void *info) {
    GEReachability *instance = (__bridge GEReachability *)info;
    if (instance && [instance isKindOfClass:[GEReachability class]]) {
        [instance reachabilityChanged:flags];
    }
}
#endif

//MARK: - Public Methods

+ (instancetype)shareInstance {
    static dispatch_once_t onceToken;
    static GEReachability *reachability = nil;
    dispatch_once(&onceToken, ^{
        reachability = [[GEReachability alloc] init];
    });
    return reachability;
}

#if TARGET_OS_IOS

- (NSString *)networkState {
    if (self.isWifi) {
        return @"WIFI";
    } else if (self.isWwan) {
        return [self currentRadio];
    } else {
        return @"NULL";
    }
}

- (void)startMonitoring {
    [self stopMonitoring];

    SCNetworkReachabilityRef reachability = SCNetworkReachabilityCreateWithName(NULL,"thinkingdata.cn");
    self.reachability = reachability;
    
    if (self.reachability != NULL) {
        SCNetworkReachabilityFlags flags;
        BOOL didRetrieveFlags = SCNetworkReachabilityGetFlags(self.reachability, &flags);
        if (didRetrieveFlags) {
            self.isWifi = (flags & kSCNetworkReachabilityFlagsReachable) && !(flags & kSCNetworkReachabilityFlagsIsWWAN);
            self.isWwan = (flags & kSCNetworkReachabilityFlagsIsWWAN);
        }
        
        SCNetworkReachabilityContext context = {0, (__bridge void *)self, NULL, NULL, NULL};
        if (SCNetworkReachabilitySetCallback(self.reachability, GravityReachabilityCallback, &context)) {
            if (!SCNetworkReachabilityScheduleWithRunLoop(self.reachability, CFRunLoopGetMain(), kCFRunLoopCommonModes)) {
                SCNetworkReachabilitySetCallback(self.reachability, NULL, NULL);
            }
        }
    }
}

- (void)stopMonitoring {
    if (!self.reachability) {
        return;
    }
    SCNetworkReachabilityUnscheduleFromRunLoop(self.reachability, CFRunLoopGetMain(), kCFRunLoopCommonModes);
}

+ (GravityNetworkType)convertNetworkType:(NSString *)networkType {
    if ([@"NULL" isEqualToString:networkType]) {
        return GravityNetworkTypeALL;
    } else if ([@"WIFI" isEqualToString:networkType]) {
        return GravityNetworkTypeWIFI;
    } else if ([@"2G" isEqualToString:networkType]) {
        return GravityNetworkType2G;
    } else if ([@"3G" isEqualToString:networkType]) {
        return GravityNetworkType3G;
    } else if ([@"4G" isEqualToString:networkType]) {
        return GravityNetworkType4G;
    }else if([@"5G"isEqualToString:networkType])
    {
        return GravityNetworkType5G;
    }
    return GravityNetworkTypeNONE;
}

//MARK: - Private Methods

- (void)reachabilityChanged:(SCNetworkReachabilityFlags)flags {
    self.isWifi = (flags & kSCNetworkReachabilityFlagsReachable) && !(flags & kSCNetworkReachabilityFlagsIsWWAN);
    self.isWwan = (flags & kSCNetworkReachabilityFlagsIsWWAN);
}

- (NSString *)currentRadio {
    NSString *networkType = @"NULL";
    @try {
        static CTTelephonyNetworkInfo *info = nil;
        static dispatch_once_t onceToken;
        dispatch_once(&onceToken, ^{
            info = [[CTTelephonyNetworkInfo alloc] init];
        });
        NSString *currentRadio = nil;
#ifdef __IPHONE_12_0
        if (@available(iOS 12.0, *)) {
            NSDictionary *serviceCurrentRadio = [info serviceCurrentRadioAccessTechnology];
            if ([serviceCurrentRadio isKindOfClass:[NSDictionary class]] && serviceCurrentRadio.allValues.count>0) {
                currentRadio = serviceCurrentRadio.allValues[0];
            }
        }
#endif
        if (currentRadio == nil && [info.currentRadioAccessTechnology isKindOfClass:[NSString class]]) {
            currentRadio = info.currentRadioAccessTechnology;
        }
        
        if ([currentRadio isEqualToString:CTRadioAccessTechnologyLTE]) {
            networkType = @"4G";
        } else if ([currentRadio isEqualToString:CTRadioAccessTechnologyeHRPD] ||
                   [currentRadio isEqualToString:CTRadioAccessTechnologyCDMAEVDORevB] ||
                   [currentRadio isEqualToString:CTRadioAccessTechnologyCDMAEVDORevA] ||
                   [currentRadio isEqualToString:CTRadioAccessTechnologyCDMAEVDORev0] ||
                   [currentRadio isEqualToString:CTRadioAccessTechnologyCDMA1x] ||
                   [currentRadio isEqualToString:CTRadioAccessTechnologyHSUPA] ||
                   [currentRadio isEqualToString:CTRadioAccessTechnologyHSDPA] ||
                   [currentRadio isEqualToString:CTRadioAccessTechnologyWCDMA]) {
            networkType = @"3G";
        } else if ([currentRadio isEqualToString:CTRadioAccessTechnologyEdge] ||
                   [currentRadio isEqualToString:CTRadioAccessTechnologyGPRS]) {
            networkType = @"2G";
        }
#ifdef __IPHONE_14_1
        else if (@available(iOS 14.1, *)) {
            if ([currentRadio isKindOfClass:[NSString class]]) {
                if([currentRadio isEqualToString:CTRadioAccessTechnologyNRNSA] ||
                   [currentRadio isEqualToString:CTRadioAccessTechnologyNR]) {
                    networkType = @"5G";
                }
            }
        }
#endif
    } @catch (NSException *exception) {
        GELogError(@"%@: %@", self, exception);
    }
    
    return networkType;
}

#elif TARGET_OS_OSX

+ (GravityNetworkType)convertNetworkType:(NSString *)networkType {
    return GravityNetworkTypeWIFI;
}

- (void)startMonitoring {
}

- (void)stopMonitoring {
}

- (NSString *)currentRadio {
    return @"WIFI";
}

- (NSString *)networkState {
    return @"WIFI";
}

#endif

@end
