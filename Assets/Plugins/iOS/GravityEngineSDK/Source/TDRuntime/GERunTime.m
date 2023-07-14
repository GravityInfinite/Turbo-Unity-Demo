//
//  GERunTime.m
//  GravityEngineSDK
//
//

#import "GERunTime.h"
#import "GEJSONUtil.h"
#import "GEPresetProperties+GEDisProperties.h"

@implementation GERunTime

#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Wundeclared-selector"
    
+ (NSString *)getAppLaunchReason {
    // start reason
    Class cls = NSClassFromString(@"GEAppLaunchReason");
    id appLaunch = [cls performSelector:@selector(sharedInstance)];
    
    if (appLaunch &&
        [appLaunch respondsToSelector:@selector(appLaunchParams)] &&
        !GEPresetProperties.disableStartReason)
    {
        NSDictionary *startReason = [appLaunch performSelector:@selector(appLaunchParams)];
        NSString *url = startReason[@"url"];
        NSDictionary *data = startReason[@"data"];
        if (url.length == 0 && data.allKeys.count == 0) {
            return @"";
        } else {
            if (data.allKeys.count == 0) {
                startReason = @{@"url":url, @"data":@""};
            }
            NSString *startReasonString = [GEJSONUtil JSONStringForObject:startReason];
            if (startReasonString && startReasonString.length) {
                return startReasonString;
            }
        }
    }
    return @"";
}

#pragma clang diagnostic pop

@end
