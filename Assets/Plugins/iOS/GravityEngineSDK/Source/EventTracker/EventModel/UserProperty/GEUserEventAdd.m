//
//  GEUserEventAdd.m
//  GravityEngineSDK
//
//

#import "GEUserEventAdd.h"

@implementation GEUserEventAdd

- (instancetype)init {
    if (self = [super init]) {
        self.eventType = GEEventTypeUserAdd;
    }
    return self;
}

- (void)validateWithError:(NSError *__autoreleasing  _Nullable *)error {
    
}

//MARK: - Delegate

- (void)ge_validateKey:(NSString *)key value:(id)value error:(NSError *__autoreleasing  _Nullable *)error {
    [super ge_validateKey:key value:value error:error];
    if (*error) {
        return;
    }
    if (![value isKindOfClass:NSNumber.class]) {
        NSString *errMsg = [NSString stringWithFormat:@"Property value must be type NSNumber. got: %@ %@. ", [value class], value];
        *error = GEPropertyError(10008, errMsg);
    }
}

@end
