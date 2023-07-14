//
//  GECheck.h
//  GravityEngineSDK
//
//  Copyright © 2021 gravityengine. All rights reserved.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

#define GE_CHECK_NIL(_object) (_object == nil || [_object isKindOfClass:[NSNull class]])

#define GE_CHECK_CLASS(_object, _class) (!GE_CHECK_NIL(_object) && [_object isKindOfClass:[_class class]])

#define GE_CHECK_CLASS_NSString(_object) GE_CHECK_CLASS(_object, [NSString class])
#define GE_CHECK_CLASS_NSNumber(_object) GE_CHECK_CLASS(_object, [NSNumber class])
#define GE_CHECK_CLASS_NSArray(_object) GE_CHECK_CLASS(_object, [NSArray class])
#define GE_CHECK_CLASS_NSData(_object) GE_CHECK_CLASS(_object, [NSData class])
#define GE_CHECK_CLASS_NSDate(_object) GE_CHECK_CLASS(_object, [NSDate class])
#define GE_CHECK_CLASS_NSDictionary(_object) GE_CHECK_CLASS(_object, [NSDictionary class])

#define GE_Valid_NSString(_object) (GE_CHECK_CLASS_NSString(_object) && (_object.length > 0))
#define GE_Valid_NSArray(_object) (GE_CHECK_CLASS_NSArray(_object) && (_object.count > 0))
#define GE_Valid_NSData(_object) (GE_CHECK_CLASS_NSData(_object) && (_object.length > 0))
#define GE_Valid_NSDictionary(_object) (GE_CHECK_CLASS_NSDictionary(_object) && (_object.allKeys.count > 0))

@interface GECheck : NSObject

+ (NSDictionary *)ge_checkToJSONObjectRecursive:(NSDictionary *)properties timeFormatter:(NSDateFormatter *)timeFormatter;

@end


NS_ASSUME_NONNULL_END
