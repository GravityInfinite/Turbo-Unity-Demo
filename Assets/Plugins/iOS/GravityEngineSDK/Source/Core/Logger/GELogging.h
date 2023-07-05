#import <Foundation/Foundation.h>

#if __has_include(<GravityEngineSDK/GEConstant.h>)
#import <GravityEngineSDK/GEConstant.h>
#else
#import "GEConstant.h"
#endif


NS_ASSUME_NONNULL_BEGIN

#define GELogDebug(message, ...)  GELogWithType(GELoggingLevelDebug, message, ##__VA_ARGS__)
#define GELogInfo(message,  ...)  GELogWithType(GELoggingLevelInfo, message, ##__VA_ARGS__)
#define GELogError(message, ...)  GELogWithType(GELoggingLevelError, message, ##__VA_ARGS__)

#define GELogWithType(type, message, ...) \
{ \
if ([GELogging sharedInstance].loggingLevel != GELoggingLevelNone && type <= [GELogging sharedInstance].loggingLevel) \
{ \
[[GELogging sharedInstance] logCallingFunction:type format:(message), ##__VA_ARGS__]; \
} \
}



@interface GELogging : NSObject

@property (class, nonatomic, readonly) GELogging *sharedInstance;
@property (assign, nonatomic) GELoggingLevel loggingLevel;
- (void)logCallingFunction:(GELoggingLevel)type format:(id)messageFormat, ...;

@end

NS_ASSUME_NONNULL_END
