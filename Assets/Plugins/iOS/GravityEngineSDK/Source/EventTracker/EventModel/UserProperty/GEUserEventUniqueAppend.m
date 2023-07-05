//
//  GEUserEventUniqueAppend.m
//  GravityEngineSDK
//
//

#import "GEUserEventUniqueAppend.h"

@implementation GEUserEventUniqueAppend

- (instancetype)init {
    if (self = [super init]) {
        self.eventType = GEEventTypeUserUniqueAppend;
    }
    return self;
}

@end
