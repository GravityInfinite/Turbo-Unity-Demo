#import <Foundation/Foundation.h>

#if __has_include(<GravityEngineSDK/GEConstant.h>)
#import <GravityEngineSDK/GEConstant.h>
#else
#import "GEConstant.h"
#endif

@class GELogMessage;
@protocol GELogger;

NS_ASSUME_NONNULL_BEGIN

@interface GEOSLog : NSObject

+ (void)log:(BOOL)asynchronous
    message:(NSString *)message
       type:(GELoggingLevel)type;

@end

@protocol GELogger <NSObject>

- (void)logMessage:(GELogMessage *)logMessage;

@optional

@property (nonatomic, strong, readonly) dispatch_queue_t loggerQueue;

@end

@interface GELogMessage : NSObject

- (instancetype)initWithMessage:(NSString *)message
                           type:(GELoggingLevel)type;

@end

@interface GEAbstractLogger : NSObject <GELogger>

+ (instancetype)sharedInstance;

@end

NS_ASSUME_NONNULL_END
