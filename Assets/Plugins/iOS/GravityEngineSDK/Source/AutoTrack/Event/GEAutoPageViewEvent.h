//
//  GEAutoPageViewEvent.h
//  GravityEngineSDK
//
//

#import "GEAutoTrackEvent.h"

NS_ASSUME_NONNULL_BEGIN

@interface GEAutoPageViewEvent : GEAutoTrackEvent
@property (nonatomic, copy) NSString *pageUrl;
@property (nonatomic, copy) NSString *referrer;
@property (nonatomic, copy) NSString *pageTitle;
@property (nonatomic, copy) NSString *screenName;

@end

NS_ASSUME_NONNULL_END
