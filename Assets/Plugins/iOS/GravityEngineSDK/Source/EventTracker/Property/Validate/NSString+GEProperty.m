//
//  NSString+GEProperty.m
//  Adjust
//
//

#import "NSString+GEProperty.h"


static NSInteger kGEPropertyNameMaxLength = 50;

@implementation NSString (GEProperty)

- (void)ge_validatePropertyKeyWithError:(NSError *__autoreleasing  _Nullable *)error {
    if (self.length == 0) {
        NSString *errorMsg = @"Property key or Event name is empty";
        GELogError(errorMsg);
        *error = GEPropertyError(10003, errorMsg);
        return;
    }

    if (self.length > kGEPropertyNameMaxLength) {
        NSString *errorMsg = [NSString stringWithFormat:@"Property key or Event name %@'s length is longer than %ld", self, kGEPropertyNameMaxLength];
        GELogError(errorMsg);
        *error = GEPropertyError(10006, errorMsg);
        return;
    }
    *error = nil;
}

- (void)ge_validatePropertyValueWithError:(NSError *__autoreleasing  _Nullable *)error {
    
}

@end
