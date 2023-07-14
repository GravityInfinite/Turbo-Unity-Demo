//
//  GEAppStartEvent.h
//  GravityEngineSDK
//
//

#import "GEAutoTrackEvent.h"

NS_ASSUME_NONNULL_BEGIN

@interface GEAppStartEvent : GEAutoTrackEvent
@property (nonatomic, copy) NSString *startReason;
@property (nonatomic, assign) BOOL resumeFromBackground;


@end

NS_ASSUME_NONNULL_END
