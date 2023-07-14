//
//  GEEventTracker.h
//  GravityEngineSDK
//
//

#import <Foundation/Foundation.h>

#if __has_include(<GravityEngineSDK/GEConstant.h>)
#import <GravityEngineSDK/GEConstant.h>
#else
#import "GEConstant.h"
#endif

#import "GESecurityPolicy.h"
#import "GravityEngineSDKPrivate.h"

NS_ASSUME_NONNULL_BEGIN

@class GEEventTracker;

@interface GEEventTracker : NSObject

+ (dispatch_queue_t)ge_networkQueue;

- (instancetype)initWithQueue:(dispatch_queue_t)queue instanceToken:(NSString *)instanceToken;

- (void)flush;

- (void)track:(NSDictionary *)event immediately:(BOOL)immediately saveOnly:(BOOL)isSaveOnly;

- (NSInteger)saveEventsData:(NSDictionary *)data;

- (void)_asyncWithCompletion:(void(^)(void))completion;

- (void)syncSendAllData;

#pragma mark - UNAVAILABLE
- (instancetype)init NS_UNAVAILABLE;
+ (instancetype)new NS_UNAVAILABLE;

@end

NS_ASSUME_NONNULL_END
