//
//  GEValidatorProtocol.h
//  GravityEngineSDK
//
//

#ifndef GEValidatorProtocol_h
#define GEValidatorProtocol_h

#import <Foundation/Foundation.h>

#if __has_include(<GravityEngineSDK/GELogging.h>)
#import <GravityEngineSDK/GELogging.h>
#else
#import "GELogging.h"
#endif

#define GEPropertyError(errorCode, errorMsg) \
    [NSError errorWithDomain:@"GravityEngineErrorDomain" \
                        code:errorCode \
                    userInfo:@{NSLocalizedDescriptionKey:errorMsg}] \


@protocol GEPropertyKeyValidating <NSObject>

- (void)ge_validatePropertyKeyWithError:(NSError **)error;

@end

/// The validator protocol of the attribute value, used to verify the attribute value
@protocol GEPropertyValueValidating <NSObject>

- (void)ge_validatePropertyValueWithError:(NSError **)error;

@end

/// The validator protocol of event properties, used to verify the key-value of a certain property
@protocol GEEventPropertyValidating <NSObject>

- (void)ge_validateKey:(NSString *)key value:(id)value error:(NSError **)error;

@end

#endif /* GEValidatorProtocol_h */
