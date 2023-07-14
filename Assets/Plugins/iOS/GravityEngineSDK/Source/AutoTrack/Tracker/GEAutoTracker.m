//
//  GEAutoTracker.m
//  GravityEngineSDK
//
//  Copyright © 2021 gravityengine. All rights reserved.
//

#import "GEAutoTracker.h"
#import "GravityEngineSDKPrivate.h"

@interface GEAutoTracker ()

@property (nonatomic, strong) NSMutableDictionary<NSString *, NSNumber *> *trackCounts;

@end

@implementation GEAutoTracker

- (instancetype)init
{
    self = [super init];
    if (self) {
        _isOneTime = NO;
        _autoFlush = YES;
        _additionalCondition = YES;
        
        self.trackCounts = [NSMutableDictionary dictionary];
    }
    return self;
}

- (void)trackWithInstanceTag:(NSString *)instanceName event:(GEAutoTrackEvent *)event params:(NSDictionary *)params {
    if ([self canTrackWithInstanceToken:instanceName]) {
        GravityEngineSDK *instance = [GravityEngineSDK sharedInstanceWithAppid:instanceName];
#ifdef DEBUG
        if (!instance) {
            @throw [NSException exceptionWithName:@"GravityEngine Exception" reason:[NSString stringWithFormat:@"check this gravity instance, instanceTag: %@", instanceName] userInfo:nil];
        }
#endif
        [instance autoTrackWithEvent:event properties:params];
                
        if (self.autoFlush) [instance flush];
    }
}

- (BOOL)canTrackWithInstanceToken:(NSString *)token {
    
    if (!self.additionalCondition) {
        return NO;
    }
    
    NSInteger trackCount = [self.trackCounts[token] integerValue];
    
    if (self.isOneTime && trackCount >= 1) {
        return NO;
    }
    
    if (self.isOneTime) {
        trackCount++;
        self.trackCounts[token] = @(trackCount);
    }
    
    return YES;
}

@end
