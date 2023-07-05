//
//  GEUpdateEventModel.m
//  GravityEngineSDK
//
//

#import "GEUpdateEventModel.h"
#import "GravityEngineSDKPrivate.h"

@implementation GEUpdateEventModel

- (instancetype)initWithEventName:(NSString *)eventName eventID:(NSString *)eventID {
    if (self = [self initWithEventName:eventName eventType:GE_EVENT_TYPE_TRACK_UPDATE]) {
        self.extraID = eventID;
    }
    return self;
}

@end
