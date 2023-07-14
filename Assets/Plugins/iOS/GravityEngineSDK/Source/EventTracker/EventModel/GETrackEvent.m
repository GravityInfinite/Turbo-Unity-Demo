//
//  GETrackEvent.m
//  GravityEngineSDK
//
//

#import "GETrackEvent.h"
#import "GEPresetProperties.h"
#import "GEPresetProperties+GEDisProperties.h"
#import "NSDate+GEFormat.h"
#import "GEDeviceInfo.h"

@implementation GETrackEvent

- (instancetype)initWithName:(NSString *)eventName {
    if (self = [self init]) {
        self.eventName = eventName;
    }
    return self;
}

- (instancetype)init
{
    self = [super init];
    if (self) {
        self.eventType = GEEventTypeTrack;
        self.systemUpTime = [GEDeviceInfo uptime];
    }
    return self;
}

- (void)validateWithError:(NSError *__autoreleasing  _Nullable *)error {
    
    [GEPropertyValidator validateEventOrPropertyName:self.eventName withError:error];
}

- (NSMutableDictionary *)jsonObject {
    NSMutableDictionary *dict = [super jsonObject];
    
    dict[@"event"] = self.eventName;
    
    if (![GEPresetProperties disableDuration]) {
        if (self.foregroundDuration > 0) {
            self.properties[@"$event_duration"] = @([NSString stringWithFormat:@"%.3f", self.foregroundDuration].floatValue / 1000);
        }
    }
    
    if (![GEPresetProperties disableBackgroundDuration]) {
        if (self.backgroundDuration > 0) {
            self.properties[@"$background_duration"] = @([NSString stringWithFormat:@"%.3f", self.backgroundDuration].floatValue / 1000);
        }
    }
    
    if (self.timeValueType != GEEventTimeValueTypeTimeOnly) {
        if (![GEPresetProperties disableZoneOffset]) {
            self.properties[@"$timezone_offset"] = @([self timeZoneOffset]);
        }
    }
    
    return dict;
}

- (double)timeZoneOffset {
    NSTimeZone *tz = self.timeZone ?: [NSTimeZone localTimeZone];
    return [[NSDate date] ge_timeZoneOffset:tz];
}

//MARK: - Delegate

- (void)ge_validateKey:(NSString *)key value:(id)value error:(NSError *__autoreleasing  _Nullable *)error {
    [GEPropertyValidator validateNormalTrackEventPropertyKey:key value:value error:error];
}

@end
