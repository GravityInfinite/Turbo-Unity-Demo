//
//  GEBaseEvent.m
//  GravityEngineSDK
//
//

#import "GEBaseEvent.h"

#if __has_include(<GravityEngineSDK/GELogging.h>)
#import <GravityEngineSDK/GELogging.h>
#else
#import "GELogging.h"
#endif

#import "GravityEngineSDKPrivate.h"

kGEEventType const kGEEventTypeTrack = @"track";

kGEEventType const kGEEventTypeTrackFirst = @"track_first";
kGEEventType const kGEEventTypeTrackUpdate = @"track_update";
kGEEventType const kGEEventTypeTrackOverwrite = @"track_overwrite";

kGEEventType const kGEEventTypeUserSet = @"profile_set";
kGEEventType const kGEEventTypeUserUnset = @"profile_unset";
kGEEventType const kGEEventTypeUserAdd = @"profile_increment";
kGEEventType const kGEEventTypeUserNumberMax = @"profile_number_max";
kGEEventType const kGEEventTypeUserNumberMin = @"profile_number_min";
kGEEventType const kGEEventTypeUserDel = @"profile_delete";
kGEEventType const kGEEventTypeUserSetOnce = @"profile_set_once";
kGEEventType const kGEEventTypeUserAppend = @"profile_append";
kGEEventType const kGEEventTypeUserUniqueAppend = @"profile_uniq_append";

@interface GEBaseEvent ()
@property (nonatomic, strong) NSDateFormatter *timeFormatter;

@end

@implementation GEBaseEvent

+ (BOOL)isTrackEvent:(NSString *)eventType {
    return [GE_EVENT_TYPE_TRACK isEqualToString:eventType]
    || [GE_EVENT_TYPE_TRACK_FIRST isEqualToString:eventType]
    || [GE_EVENT_TYPE_TRACK_UPDATE isEqualToString:eventType]
    || [GE_EVENT_TYPE_TRACK_OVERWRITE isEqualToString:eventType]
    ;
}

- (instancetype)init
{
    self = [super init];
    if (self) {
        
        _time = [NSDate date];
        self.timeValueType = GEEventTimeValueTypeNone;
        self.uuid = [NSUUID UUID].UUIDString;
    }
    return self;
}

- (instancetype)initWithType:(GEEventType)type {
    if (self = [self init]) {
        self.eventType = type;
    }
    return self;
}

- (void)validateWithError:(NSError *__autoreleasing  _Nullable *)error {
    
}

- (NSMutableDictionary *)jsonObject {
    NSMutableDictionary *dict = [NSMutableDictionary dictionary];
    UInt64 time = [self.time timeIntervalSince1970] * 1000;
    dict[@"time"] = @(time);
    
    if ([GEBaseEvent isTrackEvent:[self eventTypeString]]) {
        // 对于track事件的event，下放到GETrackEvent里去做
        dict[@"type"] = [self eventTypeString];
    } else {
        dict[@"event"] = [self eventTypeString];
        dict[@"type"] = @"profile";
    }
    
    if (self.accountId) {
        dict[@"$account_id"] = self.accountId;
    }
    if (self.distinctId) {
        dict[@"$distinct_id"] = self.distinctId;
    }
    self.properties[@"$trace_id"] = self.uuid;
    dict[@"properties"] = self.properties;
    return dict;
}

- (NSMutableDictionary *)formatDateWithDict:(NSDictionary *)dict {
    if (dict == nil || ![dict isKindOfClass:NSDictionary.class]) {
        return nil;
    }
    NSMutableDictionary *mutableDict = nil;
    if ([dict isKindOfClass:NSMutableDictionary.class]) {
        mutableDict = (NSMutableDictionary *)dict;
    } else {
        mutableDict = [dict mutableCopy];
    }
    
    NSArray<NSString *> *keys = dict.allKeys;
    for (NSInteger i = 0; i < keys.count; i++) {
        id value = dict[keys[i]];
        if ([value isKindOfClass:NSDate.class]) {
            NSString *newValue = [self.timeFormatter stringFromDate:(NSDate *)value];
            mutableDict[keys[i]] = newValue;
        } else if ([value isKindOfClass:NSDictionary.class]) {
            NSDictionary *newValue = [self formatDateWithDict:value];
            mutableDict[keys[i]] = newValue;
        }
    }
    return mutableDict;
}

- (NSString *)eventTypeString {
    switch (self.eventType) {
        case GEEventTypeTrack: {
            return GE_EVENT_TYPE_TRACK;
        } break;
        case GEEventTypeTrackFirst: {
            
            return GE_EVENT_TYPE_TRACK;
        } break;
        case GEEventTypeTrackUpdate: {
            return GE_EVENT_TYPE_TRACK_UPDATE;
        } break;
        case GEEventTypeTrackOverwrite: {
            return GE_EVENT_TYPE_TRACK_OVERWRITE;
        } break;
        case GEEventTypeUserAdd: {
            return GE_EVENT_TYPE_USER_ADD;
        } break;
        case GEEventTypeUserNumberMax: {
            return GE_EVENT_TYPE_USER_NUMBER_MAX;
        } break;
        case GEEventTypeUserNumberMin: {
            return GE_EVENT_TYPE_USER_NUMBER_MIN;
        } break;
        case GEEventTypeUserSet: {
            return GE_EVENT_TYPE_USER_SET;
        } break;
        case GEEventTypeUserUnset: {
            return GE_EVENT_TYPE_USER_UNSET;
        } break;
        case GEEventTypeUserAppend: {
            return GE_EVENT_TYPE_USER_APPEND;
        } break;
        case GEEventTypeUserUniqueAppend: {
            return GE_EVENT_TYPE_USER_UNIQ_APPEND;
        } break;
        case GEEventTypeUserDel: {
            return GE_EVENT_TYPE_USER_DEL;
        } break;
        case GEEventTypeUserSetOnce: {
            return GE_EVENT_TYPE_USER_SETONCE;
        } break;
            
        default:
            return nil;
            break;
    }
}

+ (GEEventType)typeWithTypeString:(NSString *)typeString {
    if ([typeString isEqualToString:GE_EVENT_TYPE_TRACK]) {
        return GEEventTypeTrack;
    } else if ([typeString isEqualToString:GE_EVENT_TYPE_TRACK_FIRST]) {
        return GEEventTypeTrack;
    } else if ([typeString isEqualToString:GE_EVENT_TYPE_TRACK_UPDATE]) {
        return GEEventTypeTrackUpdate;
    } else if ([typeString isEqualToString:GE_EVENT_TYPE_TRACK_OVERWRITE]) {
        return GEEventTypeTrackOverwrite;
    } else if ([typeString isEqualToString:GE_EVENT_TYPE_USER_ADD]) {
        return GEEventTypeUserAdd;
    } else if ([typeString isEqualToString:GE_EVENT_TYPE_USER_NUMBER_MAX]) {
        return GEEventTypeUserNumberMax;
    } else if ([typeString isEqualToString:GE_EVENT_TYPE_USER_NUMBER_MIN]) {
        return GEEventTypeUserNumberMin;
    } else if ([typeString isEqualToString:GE_EVENT_TYPE_USER_DEL]) {
        return GEEventTypeUserDel;
    } else if ([typeString isEqualToString:GE_EVENT_TYPE_USER_SET]) {
        return GEEventTypeUserSet;
    } else if ([typeString isEqualToString:GE_EVENT_TYPE_USER_UNSET]) {
        return GEEventTypeUserUnset;
    } else if ([typeString isEqualToString:GE_EVENT_TYPE_USER_APPEND]) {
        return GEEventTypeUserAppend;
    } else if ([typeString isEqualToString:GE_EVENT_TYPE_USER_UNIQ_APPEND]) {
        return GEEventTypeUserUniqueAppend;
    } else if ([typeString isEqualToString:GE_EVENT_TYPE_USER_SETONCE]) {
        return GEEventTypeUserSetOnce;
    }
    return GEEventTypeNone;
}

//MARK: - Private

//MARK: - Delegate

- (void)ge_validateKey:(NSString *)key value:(id)value error:(NSError *__autoreleasing  _Nullable *)error {
    
}

//MARK: - Setter & Getter

- (NSMutableDictionary *)properties {
    if (!_properties) {
        _properties = [NSMutableDictionary dictionary];
    }
    return _properties;
}

-  (void)setTimeZone:(NSTimeZone *)timeZone {
    _timeZone = timeZone;
    
    
    self.timeFormatter.timeZone = timeZone ?: [NSTimeZone localTimeZone];
}

- (NSDateFormatter *)timeFormatter {
    if (!_timeFormatter) {
        _timeFormatter = [[NSDateFormatter alloc] init];
        _timeFormatter.dateFormat = kDefaultTimeFormat;
        _timeFormatter.locale = [[NSLocale alloc] initWithLocaleIdentifier:@"en_US"];
        _timeFormatter.calendar = [[NSCalendar alloc] initWithCalendarIdentifier:NSCalendarIdentifierGregorian];
        
        _timeFormatter.timeZone = [NSTimeZone localTimeZone];
    }
    return _timeFormatter;
}

- (void)setTime:(NSDate *)time {
    
    if (time) {
        [self willChangeValueForKey:@"time"];
        _time = time;
        [self didChangeValueForKey:@"time"];
    }
}

@end
