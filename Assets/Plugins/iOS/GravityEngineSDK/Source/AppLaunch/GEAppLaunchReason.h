//
//  GEAppLaunchReason.h
//  GravityEngineSDK
//
//  Copyright © 2021 gravityengine. All rights reserved.


#import <UIKit/UIKit.h>

NS_ASSUME_NONNULL_BEGIN

@interface GEAppLaunchReason : NSObject

@property(nonatomic, copy) NSDictionary *appLaunchParams;

+ (GEAppLaunchReason *)sharedInstance;

@end

NS_ASSUME_NONNULL_END
