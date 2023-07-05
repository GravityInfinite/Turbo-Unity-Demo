//
//  GEUserEventSetOnce.m
//  GravityEngineSDK
//
//

#import "GEUserEventSetOnce.h"

@implementation GEUserEventSetOnce

- (instancetype)init {
    if (self = [super init]) {
        self.eventType = GEEventTypeUserSetOnce;
    }
    return self;
}

@end
