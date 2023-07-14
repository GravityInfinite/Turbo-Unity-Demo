//
//  GEPropertyDefaultValidator.m
//  Adjust
//
//

#import "GEPropertyDefaultValidator.h"
#import "GEPropertyValidator.h"

@implementation GEPropertyDefaultValidator

- (void)ge_validateKey:(NSString *)key value:(id)value error:(NSError *__autoreleasing  _Nullable *)error {
    [GEPropertyValidator validateBaseEventPropertyKey:key value:value error:error];
}

@end
