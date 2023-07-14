//
//  NSDate+GEFormat.h
//  GravityEngineSDK
//
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

@interface NSDate (GEFormat)

/// input the time zone
/// @param timeZone timeZone
- (double)ge_timeZoneOffset:(NSTimeZone *)timeZone;

/// Format NSDate
/// @param timeZone timeZone
/// @param formatString formatString
- (NSString *)ge_formatWithTimeZone:(NSTimeZone *)timeZone formatString:(NSString *)formatString;

@end

NS_ASSUME_NONNULL_END
