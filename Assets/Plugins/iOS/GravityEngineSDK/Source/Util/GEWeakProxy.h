//
//  GEWeakProxy.h
//  GravityEngineSDK
//
//  Copyright © 2021 gravityengine. All rights reserved.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

@interface GEWeakProxy : NSProxy

@property (nullable, nonatomic, weak, readonly) id target;

- (instancetype)initWithTarget:(id)target;

+ (instancetype)proxyWithTarget:(id)target;

@end

NS_ASSUME_NONNULL_END
