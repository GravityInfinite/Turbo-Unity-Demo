//
//  GEAutoClickEvent.h
//  GravityEngineSDK
//
//

#import "GEAutoTrackEvent.h"

NS_ASSUME_NONNULL_BEGIN

@interface GEAutoClickEvent : GEAutoTrackEvent
@property (nonatomic, copy) NSString *elementId;
@property (nonatomic, copy) NSString *elementContent;
@property (nonatomic, copy) NSString *elementType;
@property (nonatomic, copy) NSString *elementPosition;
@property (nonatomic, copy) NSString *pageTitle;
@property (nonatomic, copy) NSString *screenName;

@end

NS_ASSUME_NONNULL_END
