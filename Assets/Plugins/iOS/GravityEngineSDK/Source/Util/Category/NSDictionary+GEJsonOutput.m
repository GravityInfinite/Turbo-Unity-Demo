//
//  NSDictionary+GEJsonOutput.m
//  GravityEngineSDK
//
//  Copyright © 2021 gravityengine. All rights reserved.
//

#import "NSDictionary+GEJsonOutput.h"

@implementation NSDictionary (GEJsonOutput)

- (NSString *)descriptionWithLocale:(nullable id)locale {
    if ([NSJSONSerialization isValidJSONObject:self]) {
        NSString *output = nil;
        @try {
            NSData *jsonData = [NSJSONSerialization dataWithJSONObject:self options:NSJSONWritingPrettyPrinted error:nil];
            output = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
            output = [output stringByReplacingOccurrencesOfString:@"\\/" withString:@"/"];
        }
        @catch (NSException *exception) {
            output = self.description;
        }
        return  output;
    } else {
        return self.description;
    }
}

@end
