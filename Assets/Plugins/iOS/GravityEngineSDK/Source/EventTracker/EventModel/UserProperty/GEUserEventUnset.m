//
//  GEUserEventUnset.m
//  GravityEngineSDK
//
//

#import "GEUserEventUnset.h"

@implementation GEUserEventUnset

- (instancetype)init {
    if (self = [super init]) {
        self.eventType = GEEventTypeUserUnset;
    }
    return self;
}

@end
