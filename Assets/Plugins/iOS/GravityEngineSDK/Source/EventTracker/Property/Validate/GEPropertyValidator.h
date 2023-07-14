//
//  GEPropertyValidator.h
//  Adjust
//
//

#import <Foundation/Foundation.h>
#import "GEValidatorProtocol.h"

NS_ASSUME_NONNULL_BEGIN

@interface GEPropertyValidator : NSObject

+ (void)validateEventOrPropertyName:(NSString *)name withError:(NSError **)error;

+ (void)validateBaseEventPropertyKey:(NSString *)key value:(NSString *)value error:(NSError **)error;

+ (void)validateNormalTrackEventPropertyKey:(NSString *)key value:(NSString *)value error:(NSError **)error;

+ (void)validateAutoTrackEventPropertyKey:(NSString *)key value:(NSString *)value error:(NSError **)error;


+ (NSMutableDictionary *)validateProperties:(NSDictionary *)properties;

+ (NSMutableDictionary *)validateProperties:(NSDictionary *)properties validator:(id<GEEventPropertyValidating>)validator;

@end

NS_ASSUME_NONNULL_END
