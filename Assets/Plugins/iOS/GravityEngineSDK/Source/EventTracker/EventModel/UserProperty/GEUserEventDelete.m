//
//  GEUserEventDelete.m
//  GravityEngineSDK
//
//

#import "GEUserEventDelete.h"

@implementation GEUserEventDelete

- (instancetype)init {
    if (self = [super init]) {
        self.eventType = GEEventTypeUserDel;
    }
    return self;
}

@end
