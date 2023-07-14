#import <Foundation/Foundation.h>

#if __has_include(<GravityEngineSDK/GECalibratedTime.h>)
#import <GravityEngineSDK/GECalibratedTime.h>
#else
#import "GECalibratedTime.h"
#endif

NS_ASSUME_NONNULL_BEGIN

@interface GECalibratedTimeWithNTP : GECalibratedTime

- (void)recalibrationWithNtps:(NSArray *)ntpServers;

@end

NS_ASSUME_NONNULL_END
