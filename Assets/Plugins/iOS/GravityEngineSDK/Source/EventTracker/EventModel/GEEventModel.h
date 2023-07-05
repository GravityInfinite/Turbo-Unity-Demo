#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

typedef NS_OPTIONS(NSInteger, TimeValueType) {
    GETimeValueTypeNone      = 0,
    GETimeValueTypeTimeOnly  = 1 << 0,
    GETimeValueTypeAll       = 1 << 1,
};

typedef NSString *kEDEventTypeName;

FOUNDATION_EXTERN kEDEventTypeName const GE_EVENT_TYPE_TRACK_FIRST;
FOUNDATION_EXTERN kEDEventTypeName const GE_EVENT_TYPE_TRACK_UPDATE;
FOUNDATION_EXTERN kEDEventTypeName const GE_EVENT_TYPE_TRACK_OVERWRITE;

@interface GEEventModel : NSObject

- (instancetype)init NS_UNAVAILABLE;
+ (instancetype)new NS_UNAVAILABLE;

@property (nonatomic, copy, readonly) NSString *eventName;
@property (nonatomic, copy, readonly) kEDEventTypeName eventType; // Default is GE_EVENT_TYPE_TRACK

@property (nonatomic, strong) NSDictionary *properties;

- (void)configTime:(NSDate *)time timeZone:(NSTimeZone * _Nullable)timeZone;

@end

NS_ASSUME_NONNULL_END
