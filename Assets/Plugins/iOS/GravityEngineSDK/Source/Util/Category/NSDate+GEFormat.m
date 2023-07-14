//
//  NSDate+GEFormat.m
//  GravityEngineSDK
//
//

#import "NSDate+GEFormat.h"

@implementation NSDate (GEFormat)

- (double)ge_timeZoneOffset:(NSTimeZone *)timeZone {
    if (!timeZone) {
        return 0;
    }
    NSInteger sourceGMTOffset = [timeZone secondsFromGMTForDate:self];
    return (double)(sourceGMTOffset/3600);
}

- (NSString *)ge_formatWithTimeZone:(NSTimeZone *)timeZone formatString:(NSString *)formatString {
    NSDateFormatter *timeFormatter = [[NSDateFormatter alloc] init];
    timeFormatter.dateFormat = formatString;
    timeFormatter.locale = [[NSLocale alloc] initWithLocaleIdentifier:@"en_US"];
    timeFormatter.calendar = [[NSCalendar alloc] initWithCalendarIdentifier:NSCalendarIdentifierGregorian];
    timeFormatter.timeZone = timeZone;
    return [timeFormatter stringFromDate:self];
}

@end
