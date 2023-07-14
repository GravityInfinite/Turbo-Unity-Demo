//
//  GEAutoTracker.h
//  GravityEngineSDK
//
//  Copyright © 2021 gravityengine. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "GravityEngineSDK.h"
#import "GEAutoTrackEvent.h"

NS_ASSUME_NONNULL_BEGIN

@interface GEAutoTracker : NSObject

@property (atomic, assign) BOOL isOneTime;

@property (atomic, assign) BOOL autoFlush;

@property (atomic, assign) BOOL additionalCondition;

- (void)trackWithInstanceTag:(NSString *)instanceName event:(GEAutoTrackEvent *)event params:(nullable NSDictionary *)params;


@end

NS_ASSUME_NONNULL_END
