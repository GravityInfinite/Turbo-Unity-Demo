//
//  GECheck.m
//  GravityEngineSDK
//
//  Copyright © 2021 gravityengine. All rights reserved.
//

#import "GECheck.h"
#import "GELogging.h"

@implementation GECheck

+ (NSDictionary *)ge_checkToJSONObjectRecursive:(NSDictionary *)properties timeFormatter:(NSDateFormatter *)timeFormatter {
    return (NSDictionary *)[self ge_checkToObjectRecursive:properties timeFormatter:timeFormatter];
}

// Five basic types: list, time, Boolean, value, text, list only supports basic data types
// Advanced data types: object, object group
+ (NSObject *)ge_checkToObjectRecursive:(NSObject *)properties timeFormatter:(NSDateFormatter *)timeFormatter {
    if (GE_CHECK_NIL(properties)) {
        return properties;
    } else if (GE_CHECK_CLASS_NSDictionary(properties)) {
        NSDictionary *propertyDic = [(NSDictionary *)properties copy];
        NSMutableDictionary<NSString *, id> *propertiesDic = [NSMutableDictionary dictionaryWithDictionary:propertyDic];
        for (NSString *key in [propertyDic keyEnumerator]) {
            NSObject *newValue = [self ge_checkToJSONObjectRecursive:propertyDic[key] timeFormatter:timeFormatter];
            propertiesDic[key] = newValue;
        }
        return propertiesDic;
    } else if (GE_CHECK_CLASS_NSArray(properties)) {
        NSMutableArray *arrayItem = [(NSArray *)properties mutableCopy];
        for (int i = 0; i < arrayItem.count ; i++) {
            id item = [self ge_checkToJSONObjectRecursive:arrayItem[i] timeFormatter:timeFormatter];
            if (item)  arrayItem[i] = item;
        }
        return arrayItem;
    } else if (GE_CHECK_CLASS_NSDate(properties)) {
        NSString *dateStr = [timeFormatter stringFromDate:(NSDate *)properties];
        return dateStr;
    } else {
        return properties;
    }
}


@end
