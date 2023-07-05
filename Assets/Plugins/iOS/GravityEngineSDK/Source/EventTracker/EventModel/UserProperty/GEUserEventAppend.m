//
//  GEUserEventAppend.m
//  GravityEngineSDK
//
//

#import "GEUserEventAppend.h"

@implementation GEUserEventAppend

- (instancetype)init {
    if (self = [super init]) {
        self.eventType = GEEventTypeUserAppend;
    }
    return self;
}

//MARK: - Delegate

- (void)ge_validateKey:(NSString *)key value:(id)value error:(NSError *__autoreleasing  _Nullable *)error {
    [super ge_validateKey:key value:value error:error];
    if (*error) {
        return;
    }
    if (![value isKindOfClass:NSArray.class]) {
        NSString *errMsg = [NSString stringWithFormat:@"Property value must be type NSArray. got: %@ %@. ", [value class], value];
        *error = GEPropertyError(10009, errMsg);
    }
}

@end
