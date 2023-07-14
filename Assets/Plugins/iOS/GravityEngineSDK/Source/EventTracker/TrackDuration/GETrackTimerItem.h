//
//  GETrackTimerItem.h
//  GravityEngineSDK
//
//  Copyright © 2022 gravityengine. All rights reserved.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

@interface GETrackTimerItem : NSObject
/// The moment when the event starts to be recorded (the total time the device has been running)
@property (nonatomic, assign) NSTimeInterval beginTime;
/// Accumulated time in the foreground
@property (nonatomic, assign) NSTimeInterval foregroundDuration;
/// The time the event entered the background (total time the device has been running)
@property (nonatomic, assign) NSTimeInterval enterBackgroundTime;
/// accumulated time in the background
@property (nonatomic, assign) NSTimeInterval backgroundDuration;

@end

NS_ASSUME_NONNULL_END
