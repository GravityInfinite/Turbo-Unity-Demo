//
//  GEAppEndEvent.h
//  GravityEngineSDK
//
//

#import "GEAutoTrackEvent.h"

NS_ASSUME_NONNULL_BEGIN

@interface GEAppEndEvent : GEAutoTrackEvent
@property (nonatomic, copy) NSString *screenName;

@end

NS_ASSUME_NONNULL_END
