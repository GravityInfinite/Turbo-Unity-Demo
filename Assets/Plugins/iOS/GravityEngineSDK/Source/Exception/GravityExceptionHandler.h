#import <Foundation/Foundation.h>

#import "GravityEngineSDKPrivate.h"

NS_ASSUME_NONNULL_BEGIN

@interface GravityExceptionHandler : NSObject

@property (nonatomic, strong) NSHashTable *gravityEngineSDKInstances;

@property (nonatomic) NSUncaughtExceptionHandler *ge_lastExceptionHandler;

@property (nonatomic, unsafe_unretained) struct sigaction *ge_signalHandlers;

+ (instancetype)sharedHandler;

- (void)addGravityInstance :(GravityEngineSDK *)instance;

@end

NS_ASSUME_NONNULL_END
