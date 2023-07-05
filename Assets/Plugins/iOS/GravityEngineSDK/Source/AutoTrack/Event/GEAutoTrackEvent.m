//
//  GEAutoTrackEvent.m
//  GravityEngineSDK
//
//

#import "GEAutoTrackEvent.h"
#import "GravityEngineSDKPrivate.h"
#import "GEPresetProperties+GEDisProperties.h"

@implementation GEAutoTrackEvent

- (NSMutableDictionary *)jsonObject {
    NSMutableDictionary *dict = [super jsonObject];    
    // Reprocess the duration of automatic collection events, mainly app_start, app_end
    // app_start app_end events are collected by the automatic collection management class. There are the following problems: the automatic collection management class and the timeTracker event duration management class are processed by listening to appLifeCycle notifications, so they are not at a precise and unified time point. There will be small errors that need to be eliminated.
    // After testing, the error is less than 0.01s.
    CGFloat minDuration = 0.01;
    if (![GEPresetProperties disableDuration]) {
        if (self.foregroundDuration > minDuration) {
            self.properties[@"$event_duration"] = @([NSString stringWithFormat:@"%.3f", self.foregroundDuration].floatValue / 1000);
        }
    }
    if (![GEPresetProperties disableBackgroundDuration]) {
        if (self.backgroundDuration > minDuration) {
            self.properties[@"$background_duration"] = @([NSString stringWithFormat:@"%.3f", self.backgroundDuration].floatValue / 1000);
        }
    }
    
    return dict;
}

- (GravityEngineAutoTrackEventType)autoTrackEventType {
    if ([self.eventName isEqualToString:GE_APP_START_EVENT]) {
        return GravityEngineEventTypeAppStart;
    } else if ([self.eventName isEqualToString:GE_APP_START_BACKGROUND_EVENT]) {
        return GravityEngineEventTypeAppStart;
    } else if ([self.eventName isEqualToString:GE_APP_END_EVENT]) {
        return GravityEngineEventTypeAppEnd;
    } else if ([self.eventName isEqualToString:GE_APP_VIEW_EVENT]) {
        return GravityEngineEventTypeAppViewScreen;
    } else if ([self.eventName isEqualToString:GE_APP_CLICK_EVENT]) {
        return GravityEngineEventTypeAppClick;
    } else if ([self.eventName isEqualToString:GE_APP_CRASH_EVENT]) {
        return GravityEngineEventTypeAppViewCrash;
    } else if ([self.eventName isEqualToString:GE_APP_INSTALL_EVENT]) {
        return GravityEngineEventTypeAppInstall;
    } else {
        return GravityEngineEventTypeNone;
    }
}

- (void)ge_validateKey:(NSString *)key value:(id)value error:(NSError *__autoreleasing  _Nullable *)error {
    [GEPropertyValidator validateAutoTrackEventPropertyKey:key value:value error:error];
}

@end
