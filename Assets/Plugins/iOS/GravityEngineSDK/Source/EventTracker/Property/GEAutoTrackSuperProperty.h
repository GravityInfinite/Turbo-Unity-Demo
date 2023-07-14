//
//  GEAutoTrackSuperProperty.h
//  GravityEngineSDK
//
//

#import <Foundation/Foundation.h>
#import "GEConstant.h"

NS_ASSUME_NONNULL_BEGIN

@interface GEAutoTrackSuperProperty : NSObject

- (void)registerSuperProperties:(NSDictionary *)properties withType:(GravityEngineAutoTrackEventType)type;

- (NSDictionary *)currentSuperPropertiesWithEventName:(NSString *)eventName;

- (void)registerDynamicSuperProperties:(NSDictionary<NSString *, id> *(^)(GravityEngineAutoTrackEventType, NSDictionary *))dynamicSuperProperties;

- (NSDictionary *)obtainDynamicSuperPropertiesWithType:(GravityEngineAutoTrackEventType)type currentProperties:(NSDictionary *)properties;

- (void)clearSuperProperties;

@end

NS_ASSUME_NONNULL_END
