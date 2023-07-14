//
//  GEBaseEvent.h
//  GravityEngineSDK
//
//

#import <Foundation/Foundation.h>
#import "GEPropertyValidator.h"

NS_ASSUME_NONNULL_BEGIN

typedef NSString * kGEEventType;

typedef NS_OPTIONS(NSUInteger, GEEventType) {
    GEEventTypeNone = 0,
    GEEventTypeTrack = 1 << 0,
    GEEventTypeTrackFirst = 1 << 1,
    GEEventTypeTrackUpdate = 1 << 2,
    GEEventTypeTrackOverwrite = 1 << 3,
    GEEventTypeUserSet = 1 << 4,
    GEEventTypeUserUnset = 1 << 5,
    GEEventTypeUserAdd = 1 << 6,
    GEEventTypeUserDel = 1 << 7,
    GEEventTypeUserSetOnce = 1 << 8,
    GEEventTypeUserAppend = 1 << 9,
    GEEventTypeUserUniqueAppend = 1 << 10,
    GEEventTypeUserNumberMax = 1 << 11,
    GEEventTypeUserNumberMin = 1 << 12,
    GEEventTypeAll = 0xFFFFFFFF,
};

//extern kGEEventType const kGEEventTypeTrack;
//extern kGEEventType const kGEEventTypeTrackFirst;
//extern kGEEventType const kGEEventTypeTrackUpdate;
//extern kGEEventType const kGEEventTypeTrackOverwrite;
//extern kGEEventType const kGEEventTypeUserSet;
//extern kGEEventType const kGEEventTypeUserUnset;
//extern kGEEventType const kGEEventTypeUserAdd;
//extern kGEEventType const kGEEventTypeUserDel;
//extern kGEEventType const kGEEventTypeUserSetOnce;
//extern kGEEventType const kGEEventTypeUserAppend;
//extern kGEEventType const kGEEventTypeUserUniqueAppend;

typedef NS_OPTIONS(NSInteger, GEEventTimeValueType) {
    GEEventTimeValueTypeNone = 0,
    GEEventTimeValueTypeTimeOnly = 1 << 0,
    GEEventTimeValueTypeTimeAndZone = 1 << 1,
};

@interface GEBaseEvent : NSObject<GEEventPropertyValidating>
@property (nonatomic, assign) GEEventType eventType;
@property (nonatomic, copy) NSString *uuid;
@property (nonatomic, copy) NSString *accountId;
@property (nonatomic, copy) NSString *distinctId;
@property (nonatomic, strong) NSDate *time;
@property (nonatomic, strong) NSTimeZone *timeZone;
@property (nonatomic, strong, readonly) NSDateFormatter *timeFormatter;

@property (nonatomic, assign) GEEventTimeValueType timeValueType;
@property (nonatomic, strong) NSMutableDictionary *properties;

@property (nonatomic, assign) BOOL immediately;

@property (atomic, assign, getter=isTrackPause) BOOL trackPause;

@property (nonatomic, assign) BOOL isEnabled;

@property (atomic, assign) BOOL isOptOut;

- (instancetype)initWithType:(GEEventType)type;

- (void)validateWithError:(NSError **)error;

- (NSMutableDictionary *)jsonObject;

- (NSMutableDictionary *)formatDateWithDict:(NSDictionary *)dict;

- (NSString *)eventTypeString;

+ (GEEventType)typeWithTypeString:(NSString *)typeString;

@end

NS_ASSUME_NONNULL_END
