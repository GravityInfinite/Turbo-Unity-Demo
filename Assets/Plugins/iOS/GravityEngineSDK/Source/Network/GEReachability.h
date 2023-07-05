//
//  GEReachability.h
//  GravityEngineSDK
//
//

#import <Foundation/Foundation.h>
#import "GEConstant.h"

NS_ASSUME_NONNULL_BEGIN

@interface GEReachability : NSObject

+ (GravityNetworkType)convertNetworkType:(NSString *)networkType;

+ (instancetype)shareInstance;

- (void)startMonitoring;

- (void)stopMonitoring;

- (NSString *)networkState;


@end

NS_ASSUME_NONNULL_END
