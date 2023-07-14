//
//  GEEncryptAlgorithm.h
//  GravityEngineSDK
//


NS_ASSUME_NONNULL_BEGIN

@protocol GEEncryptAlgorithm <NSObject>


- (nullable NSString *)encryptData:(NSData *)data;


- (NSString *)algorithm;

@end

NS_ASSUME_NONNULL_END
