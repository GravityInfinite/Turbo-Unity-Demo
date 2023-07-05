//
//  GEOverwriteEventModel.m
//  GravityEngineSDK
//
//

#import "GEOverwriteEventModel.h"
#import "GravityEngineSDKPrivate.h"

@implementation GEOverwriteEventModel

- (instancetype)initWithEventName:(NSString *)eventName eventID:(NSString *)eventID {
    if (self = [self initWithEventName:eventName eventType:GE_EVENT_TYPE_TRACK_OVERWRITE]) {
        self.extraID = eventID;
    }
    return self;
}

@end
