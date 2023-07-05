//
//  GECommonUtil.m
//  GravityEngineSDK
//
//

#import "GECommonUtil.h"

@implementation GECommonUtil

+ (NSString *)string:(NSString *)string {
    if ([string isKindOfClass:[NSString class]] && string.length) {
        return string;
    } else {
        return @"";
    }
}

+ (NSDictionary *)dictionary:(NSDictionary *)dic {
    if (dic && [dic isKindOfClass:[NSDictionary class]] && dic.allKeys.count) {
        return dic;
    } else {
        return @{};
    }
}

@end
