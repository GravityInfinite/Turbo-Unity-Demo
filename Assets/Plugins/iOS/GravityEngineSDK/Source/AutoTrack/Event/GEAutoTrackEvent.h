//
//  GEAutoTrackEvent.h
//  GravityEngineSDK
//
//

#import "GETrackEvent.h"
#import "GEConstant.h"

NS_ASSUME_NONNULL_BEGIN

@interface GEAutoTrackEvent : GETrackEvent

/// It is used to record the dynamic public properties of automatic collection events. The dynamic public properties need to be obtained in the current thread where the event occurs
@property (nonatomic, strong) NSDictionary *autoDynamicSuperProperties;

/// Static public property for logging autocollection events
@property (nonatomic, strong) NSDictionary *autoSuperProperties;

/// Returns the automatic collection type
- (GravityEngineAutoTrackEventType)autoTrackEventType;

@end

NS_ASSUME_NONNULL_END
