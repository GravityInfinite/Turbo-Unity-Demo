
#import "GEEventModel.h"
#import "GravityEngineSDKPrivate.h"

kEDEventTypeName const GE_EVENT_TYPE_TRACK_FIRST       = @"track_first";
kEDEventTypeName const GE_EVENT_TYPE_TRACK_UPDATE       = @"track_update";
kEDEventTypeName const GE_EVENT_TYPE_TRACK_OVERWRITE    = @"track_overwrite";

@interface GEEventModel ()

@property (nonatomic, copy) NSString *eventName;
@property (nonatomic, copy) kEDEventTypeName eventType;

@end

@implementation GEEventModel

- (instancetype)initWithEventName:(NSString *)eventName {
    return [self initWithEventName:eventName eventType:GE_EVENT_TYPE_TRACK];
}

- (instancetype)initWithEventName:(NSString *)eventName eventType:(kEDEventTypeName)eventType {
    if (self = [[[GEEventModel class] alloc] init]) {
        self.persist = YES;
        self.eventName = eventName ?: @"";
        self.eventType = eventType ?: @"";
        if ([self.eventType isEqualToString:GE_EVENT_TYPE_TRACK_FIRST]) {
            _extraID = [GEDeviceInfo sharedManager].deviceId ?: @"";
        }
    }
    return self;
}

#pragma mark - Public

- (void)configTime:(NSDate *)time timeZone:(NSTimeZone *)timeZone {
    if (!time || ![time isKindOfClass:[NSDate class]]) {
        self.timeValueType = GETimeValueTypeNone;
    } else {
        self.time = time;
        self.timeZone = timeZone;
        
        NSDateFormatter *timeFormatter = [[NSDateFormatter alloc] init];
        timeFormatter.dateFormat = kDefaultTimeFormat;
        timeFormatter.locale = [[NSLocale alloc] initWithLocaleIdentifier:@"en_US"];
        timeFormatter.calendar = [[NSCalendar alloc] initWithCalendarIdentifier:NSCalendarIdentifierGregorian];
        if (timeZone && [timeZone isKindOfClass:[NSTimeZone class]]) {
            self.timeValueType = GETimeValueTypeAll;
            timeFormatter.timeZone = timeZone;
        } else {
            self.timeValueType = GETimeValueTypeTimeOnly;
            timeFormatter.timeZone = [NSTimeZone localTimeZone];
        }
        self.timeString = [timeFormatter stringFromDate:time];
    }
}

#pragma mark - Setter

- (void)setExtraID:(NSString *)extraID {
    if (extraID.length > 0) {
        _extraID = extraID;
    } else {
        if ([self.eventType isEqualToString:GE_EVENT_TYPE_TRACK_FIRST]) {
            GELogError(@"Invalid firstCheckId. Use device Id");
        } else {
            GELogError(@"Invalid eventId");
        }
    }
}

@end
