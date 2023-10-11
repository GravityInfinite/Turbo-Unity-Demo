//
//  NSString+GEString.m
//  GravityEngineSDK
//
//  Copyright © 2021 gravityengine. All rights reserved.
//

#import "NSString+GEString.h"

@implementation NSString (GEString)

- (NSString *)ge_trim {
    NSString *string = [self stringByReplacingOccurrencesOfString:@" " withString:@""];
    string = [string stringByReplacingOccurrencesOfString:@"\r" withString:@""];
    string = [string stringByReplacingOccurrencesOfString:@"\n" withString:@""];
    return string;
}

- (NSString *)ge_formatUrlString {
    NSString *urlString = [self stringByTrimmingCharactersInSet:[NSCharacterSet whitespaceCharacterSet]];
    
    NSURL *url = [NSURL URLWithString:urlString];
    NSString *scheme = [url scheme];
    NSString *host = [url host];
    NSNumber *port = [url port];
    
    if (scheme && scheme.length>0 && host && host.length>0) {
        urlString = [NSString stringWithFormat:@"%@://%@", scheme, host];
        if (port && [port stringValue]) {
            urlString = [urlString stringByAppendingFormat:@":%@", [port stringValue]];
        }
    }
    return urlString;
}

// 尽量跟js中的encodeURIComponent函数保持行为一致
- (NSString *)ge_urlEncodedString {
    NSCharacterSet *allowedCharacterSet = [NSCharacterSet characterSetWithCharactersInString:@"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-._~!*'()"];
    
    NSMutableCharacterSet *customCharacterSet = [allowedCharacterSet mutableCopy];
    [customCharacterSet removeCharactersInString:@";:@&=+$,/?%#[]"];
    
    NSString *encodedString = [self stringByAddingPercentEncodingWithAllowedCharacters:customCharacterSet];
    return encodedString;
}

@end
