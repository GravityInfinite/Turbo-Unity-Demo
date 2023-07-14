//
//  GETrackOverwriteEvent.m
//  GravityEngineSDK
//
//

#import "GETrackOverwriteEvent.h"

@implementation GETrackOverwriteEvent

- (instancetype)init
{
    self = [super init];
    if (self) {
        self.eventType = GEEventTypeTrackOverwrite;
    }
    return self;
}

- (NSMutableDictionary *)jsonObject {
    NSMutableDictionary *dict = [super jsonObject];
    
    dict[@"$event_id"] = self.eventId;
    
    return dict;
}


- (void)validateWithError:(NSError *__autoreleasing  _Nullable *)error {
    [super validateWithError:error];
    if (*error) {
        return;
    }
    if (self.eventId.length <= 0) {
        NSString *errorMsg = @"property 'eventId' cannot be empty which in OverwriteEvent";
        *error = GEPropertyError(100011, errorMsg);
        return;
    }
}

@end
