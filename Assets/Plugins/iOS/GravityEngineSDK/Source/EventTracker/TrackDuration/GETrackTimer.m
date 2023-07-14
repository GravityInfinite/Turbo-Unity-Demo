//
//  GETrackTimer.m
//  GravityEngineSDK
//
//  Copyright © 2022 gravityengine. All rights reserved.
//

#import "GETrackTimer.h"
#import "GETrackTimerItem.h"
#import "GEThreadSafeDictionary.h"
#import "GEDeviceInfo.h"

@interface GETrackTimer ()
@property (nonatomic, strong) GEThreadSafeDictionary *events;

@end

@implementation GETrackTimer

- (instancetype)init
{
    self = [super init];
    if (self) {
        self.events = [GEThreadSafeDictionary dictionary];
    }
    return self;
}

- (void)trackEvent:(NSString *)eventName withSystemUptime:(NSTimeInterval)systemUptime {
    if (!eventName.length) {
        return;
    }
    GETrackTimerItem *item = [[GETrackTimerItem alloc] init];
    item.beginTime = systemUptime ?: [GEDeviceInfo uptime];
    self.events[eventName] = item;
}

- (void)enterForegroundWithSystemUptime:(NSTimeInterval)systemUptime {
    NSArray *keys = [self.events allKeys];
    for (NSString *key in keys) {
        GETrackTimerItem *item = self.events[key];
        item.beginTime = systemUptime;
        if (item.enterBackgroundTime == 0) {
            item.backgroundDuration = 0;
        } else {
            item.backgroundDuration = systemUptime - item.enterBackgroundTime + item.backgroundDuration;
        }
    }
}

- (void)enterBackgroundWithSystemUptime:(NSTimeInterval)systemUptime {
    NSArray *keys = [self.events allKeys];
    for (NSString *key in keys) {
        GETrackTimerItem *item = self.events[key];
        item.enterBackgroundTime = systemUptime;
        item.foregroundDuration = systemUptime - item.beginTime + item.foregroundDuration;
    }
}

- (NSTimeInterval)foregroundDurationOfEvent:(NSString *)eventName isActive:(BOOL)isActive systemUptime:(NSTimeInterval)systemUptime {
    if (!eventName.length) {
        return 0;
    }
    GETrackTimerItem *item = self.events[eventName];
    if (!item) {
        return 0;
    }
    
    if (isActive) {
        NSTimeInterval duration = systemUptime - item.beginTime + item.foregroundDuration;
        return [self validateDuration:duration eventName:eventName];
    } else {
        return [self validateDuration:item.foregroundDuration eventName:eventName];
    }
    
}

- (NSTimeInterval)backgroundDurationOfEvent:(NSString *)eventName isActive:(BOOL)isActive systemUptime:(NSTimeInterval)systemUptime {
    if (!eventName.length) {
        return 0;
    }
    GETrackTimerItem *item = self.events[eventName];
    if (!item) {
        return 0;
    }
    if (isActive) {
        return [self validateDuration:item.backgroundDuration eventName:eventName];
    } else {
        NSTimeInterval duration = 0;
        if (item.enterBackgroundTime == 0) {
            duration = 0;
        } else {
            duration = systemUptime - item.enterBackgroundTime + item.backgroundDuration;
        }
        return [self validateDuration:duration eventName:eventName];
    }
}

- (void)removeEvent:(NSString *)eventName {
    [self.events removeObjectForKey:eventName];
}

- (BOOL)isExistEvent:(NSString *)eventName {
    return self.events[eventName] != nil;
}

- (void)clear {
    [self.events removeAllObjects];
}

//MARK: - Private Methods

- (NSTimeInterval)validateDuration:(NSTimeInterval)duration eventName:(NSString *)eventName {
    NSInteger max = 3600 * 24;
    if (duration >= max) {
        return max;
    }
    
    return duration;
}

@end


