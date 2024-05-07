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


- (int)extractAndConvertToNumber {
    // 定义一个正则表达式，匹配数字
    NSString *pattern = @"\\d+";
    
    // 创建一个正则表达式对象
    NSError *error = NULL;
    NSRegularExpression *regex = [NSRegularExpression regularExpressionWithPattern:pattern options:0 error:&error];
    if (error != nil) {
        return 1; // 如果创建正则表达式失败，返回1
    }
    
    // 使用正则表达式匹配输入字符串
    NSArray<NSTextCheckingResult *> *matches = [regex matchesInString:self options:0 range:NSMakeRange(0, [self length])];
    
    // 遍历匹配结果，提取数字
    NSMutableString *extractedNumbers = [NSMutableString string];
    for (NSTextCheckingResult *match in matches) {
        NSRange matchRange = [match range];
        NSString *matchedString = [self substringWithRange:matchRange];
        [extractedNumbers appendString:matchedString];
    }
    
    // 如果提取出的数字为空字符串，则返回1
    if ([extractedNumbers isEqualToString:@""]) {
        return 1;
    }
    
    // 将提取出的数字转换为NSNumber类型并返回
    return ([extractedNumbers intValue]);
}

@end
