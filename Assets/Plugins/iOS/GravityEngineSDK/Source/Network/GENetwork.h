#import <Foundation/Foundation.h>

#import "GravityEngineSDKPrivate.h"

NS_ASSUME_NONNULL_BEGIN

typedef void (^TDFlushConfigBlock)(NSDictionary *result, NSError * _Nullable error);
typedef void (^GEPostDataBlock)(NSDictionary *result, NSError * _Nullable error);

@interface GENetwork : NSObject <NSURLSessionTaskDelegate, NSURLSessionDataDelegate>

@property (nonatomic, copy) NSString *appid;
@property (nonatomic, strong) NSURL *serverURL;

@property (nonatomic, assign) GravityEngineDebugMode debugMode;
@property (nonatomic, strong) GESecurityPolicy *securityPolicy;
@property (nonatomic, copy) TDURLSessionDidReceiveAuthenticationChallengeBlock sessionDidReceiveAuthenticationChallenge;

- (BOOL)flushEvents:(NSArray<NSDictionary *> *)events;
- (void)fetchRemoteConfig:(NSString *)appid handler:(TDFlushConfigBlock)handler;
- (void)postDataWith:(NSDictionary *)postBodyDic handler:(GEPostDataBlock)handler;
@end

NS_ASSUME_NONNULL_END

