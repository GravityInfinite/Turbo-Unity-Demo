#import "GECalibratedTimeWithNTP.h"
#import "GENTPServer.h"
#import "GELogging.h"
#import "GEDeviceInfo.h"

@interface GECalibratedTimeWithNTP()
@end

static dispatch_group_t _ge_ntpGroup;
static NSString *_ge_ntpQueuelabel;
static dispatch_queue_t _ge_ntpSerialQueue;

@implementation GECalibratedTimeWithNTP

@synthesize serverTime = _serverTime;

+ (instancetype)sharedInstance {
    static dispatch_once_t once;
    static id sharedInstance;
    dispatch_once(&once, ^{
        sharedInstance = [[GECalibratedTimeWithNTP alloc] init];
        _ge_ntpGroup = dispatch_group_create();
        _ge_ntpQueuelabel = [NSString stringWithFormat:@"com.gravityinfinite.ntp.%p", (void *)self];
        _ge_ntpSerialQueue = dispatch_queue_create([_ge_ntpQueuelabel UTF8String], DISPATCH_QUEUE_SERIAL);
    });
    return sharedInstance;
}

- (void)recalibrationWithNtps:(NSArray *)ntpServers {
    
    if (_ge_ntpGroup) {
        GELogDebug(@"ntp servers async start");
    } else {
        GELogDebug(@"ntp servers async start, _ntpGroup is nil");
    }
    dispatch_group_async(_ge_ntpGroup, _ge_ntpSerialQueue, ^{
        [self startNtp:ntpServers];
    });
}

- (NSTimeInterval)serverTime {
    
    if (_ge_ntpGroup) {
    
        long ret = dispatch_group_wait(_ge_ntpGroup, dispatch_time(DISPATCH_TIME_NOW, (int64_t)(3 * NSEC_PER_SEC)));
        if (ret != 0) {
            self.stopCalibrate = YES;
        }
        return _serverTime;
    } else {
        self.stopCalibrate = YES;
    }
    
    return 0;
    
}

- (void)startNtp:(NSArray *)ntpServerHost {
    NSMutableArray *serverHostArr = [NSMutableArray array];
    for (NSString *host in ntpServerHost) {
        if ([host isKindOfClass:[NSString class]] && host.length > 0) {
            [serverHostArr addObject:host];
        }
    }
    NSError *err;
    for (NSString *host in serverHostArr) {
        GELogDebug(@"ntp host :%@", host);
        err = nil;
        GENTPServer *server = [[GENTPServer alloc] initWithHostname:host port:123];
        NSTimeInterval offset = [server dateWithError:&err];
        [server disconnect];
        
        if (err) {
            GELogDebug(@"ntp failed :%@", err);
        } else {
            self.systemUptime = [GEDeviceInfo uptime];
            self.serverTime = [[NSDate dateWithTimeIntervalSinceNow:offset] timeIntervalSince1970];
            break;
        }
    }
    
    if (err) {
        GELogDebug(@"get ntp time failed");
        self.stopCalibrate = YES;
    }
}

@end
