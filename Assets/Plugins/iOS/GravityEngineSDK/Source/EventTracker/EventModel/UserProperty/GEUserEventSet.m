//
//  GEUserEventSet.m
//  GravityEngineSDK
//
//

#import "GEUserEventSet.h"

@implementation GEUserEventSet

- (instancetype)init {
    if (self = [super init]) {
        self.eventType = GEEventTypeUserSet;
    }
    return self;
}

@end
