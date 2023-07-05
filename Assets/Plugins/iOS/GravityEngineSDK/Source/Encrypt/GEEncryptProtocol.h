//
//  GEEncryptProtocol.h
//  GravityEngineSDK
//


NS_ASSUME_NONNULL_BEGIN

@protocol GEEncryptProtocol <NSObject>


- (NSString *)symmetricEncryptType;


- (NSString *)asymmetricEncryptType;


- (NSString *)encryptEvent:(NSData *)event;


- (NSString *)encryptSymmetricKeyWithPublicKey:(NSString *)publicKey;

@end

NS_ASSUME_NONNULL_END
