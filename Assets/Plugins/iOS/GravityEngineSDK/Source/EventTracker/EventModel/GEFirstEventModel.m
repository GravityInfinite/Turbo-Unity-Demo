#import "GEFirstEventModel.h"
#import "GravityEngineSDKPrivate.h"

@implementation GEFirstEventModel

- (instancetype)initWithEventName:(NSString *)eventName {
    return [self initWithEventName:eventName eventType:GE_EVENT_TYPE_TRACK_FIRST];
}

- (instancetype)initWithEventName:(NSString *)eventName firstCheckID:(NSString *)firstCheckID {
    if (self = [self initWithEventName:eventName eventType:GE_EVENT_TYPE_TRACK_FIRST]) {
        self.extraID = firstCheckID;
    }
    return self;
}

@end
