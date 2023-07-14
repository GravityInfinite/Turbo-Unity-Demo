//
//  GERSAEncryptor.h
//  GravityEngineSDK
//
//

#import <Foundation/Foundation.h>
#import "GEEncryptAlgorithm.h"

NS_ASSUME_NONNULL_BEGIN

@interface GERSAEncryptor : NSObject <GEEncryptAlgorithm>

@property (nonatomic, copy) NSString *key;

@end

NS_ASSUME_NONNULL_END
