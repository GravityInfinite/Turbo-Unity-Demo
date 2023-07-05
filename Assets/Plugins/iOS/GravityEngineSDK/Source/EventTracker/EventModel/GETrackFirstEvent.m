//
//  GETrackFirstEvent.m
//  GravityEngineSDK
//
//

#import "GETrackFirstEvent.h"
#import "GEDeviceInfo.h"

@implementation GETrackFirstEvent

- (instancetype)init
{
    self = [super init];
    if (self) {
        self.eventType = GEEventTypeTrackFirst;
    }
    return self;
}

- (void)validateWithError:(NSError *__autoreleasing  _Nullable *)error {
    [super validateWithError:error];
    if (*error) {
        return;
    }
    if (self.firstCheckId.length <= 0) {
        NSString *errorMsg = @"property 'firstCheckId' cannot be empty which in FirstEvent";
        *error = GEPropertyError(100010, errorMsg);
        return;
    }
}

- (NSMutableDictionary *)jsonObject {
    NSMutableDictionary *dict = [super jsonObject];
    
    dict[@"$first_check_id"] = self.firstCheckId ?: [GEDeviceInfo sharedManager].deviceId;
    
    return dict;
}

@end
