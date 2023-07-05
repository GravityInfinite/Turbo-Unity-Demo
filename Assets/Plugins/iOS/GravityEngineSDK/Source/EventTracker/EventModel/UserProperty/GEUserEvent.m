//
//  GEUserEvent.m
//  GravityEngineSDK
//
//

#import "GEUserEvent.h"

@implementation GEUserEvent

- (instancetype)init
{
    self = [super init];
    if (self) {
        self.timeValueType = GEEventTimeValueTypeNone;
    }
    return self;
}

//MARK: - Delegate

- (void)ge_validateKey:(NSString *)key value:(id)value error:(NSError *__autoreleasing  _Nullable *)error {
    [GEPropertyValidator validateBaseEventPropertyKey:key value:value error:error];
}

//MARK: - Setter & Getter

- (void)setTime:(NSDate *)time {
    [super setTime:time];
    
    self.timeValueType = time == nil ? GEEventTimeValueTypeNone : GEEventTimeValueTypeTimeOnly;
}

@end
