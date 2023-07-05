//
//  GEAESEncryptor.h
//  GravityEngineSDK
//
//

#import <Foundation/Foundation.h>
#import "GEEncryptAlgorithm.h"

NS_ASSUME_NONNULL_BEGIN

@interface GEAESEncryptor : NSObject <GEEncryptAlgorithm>

@property (nonatomic, copy, readonly) NSData *key;

@end

NS_ASSUME_NONNULL_END
