//
//  NSNumber+GEProperty.m
//  Adjust
//
//

#import "NSNumber+GEProperty.h"

@implementation NSNumber (GEProperty)

- (void)ge_validatePropertyValueWithError:(NSError *__autoreleasing  _Nullable *)error {
    if ([self doubleValue] > 9999999999999.999 || [self doubleValue] < -9999999999999.999) {
        NSString *errorMsg = [NSString stringWithFormat:@"The number value [%@] is invalid.", self];
        GELogError(errorMsg);
        *error = GEPropertyError(10009, errorMsg);
    }
}

@end
