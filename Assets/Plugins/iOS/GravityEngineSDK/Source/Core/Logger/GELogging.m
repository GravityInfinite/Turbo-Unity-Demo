#import "GELogging.h"

#import <os/log.h>
#import "GEOSLog.h"

@implementation GELogging

+ (instancetype)sharedInstance {
    static dispatch_once_t once;
    static id sharedInstance;
    dispatch_once(&once, ^{
        sharedInstance = [[self alloc] init];
    });
    return sharedInstance;
}

- (void)logCallingFunction:(GELoggingLevel)type format:(id)messageFormat, ... {
    if (messageFormat) {
        va_list formatList;
        va_start(formatList, messageFormat);
        NSString *formattedMessage = [[NSString alloc] initWithFormat:messageFormat arguments:formatList];
        va_end(formatList);
        
#ifdef __IPHONE_10_0
        if (@available(iOS 10.0, *)) {
            [GEOSLog log:NO message:formattedMessage type:type];
        }
#else
        NSLog(@"[GravityEngine] %@", formattedMessage);
#endif
    }
}

@end

