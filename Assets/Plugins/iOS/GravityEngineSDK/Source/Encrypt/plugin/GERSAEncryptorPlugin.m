//
//  GERSAEncryptorPlugin.m
//  GravityEngineSDK
//
//

#import "GERSAEncryptorPlugin.h"
#import "GEAESEncryptor.h"
#import "GERSAEncryptor.h"

@interface GERSAEncryptorPlugin ()

@property (nonatomic, strong) GEAESEncryptor *aesEncryptor;
@property (nonatomic, strong) GERSAEncryptor *rsaEncryptor;

@end

@implementation GERSAEncryptorPlugin

- (instancetype)init {
    self = [super init];
    if (self) {
        _aesEncryptor = [[GEAESEncryptor alloc] init];
        _rsaEncryptor = [[GERSAEncryptor alloc] init];
    }
    return self;
}


- (NSString *)symmetricEncryptType {
    return [_aesEncryptor algorithm];
}


- (NSString *)asymmetricEncryptType {
    return [_rsaEncryptor algorithm];
}


- (NSString *)encryptEvent:(NSData *)event {
    return [_aesEncryptor encryptData:event];
}


- (NSString *)encryptSymmetricKeyWithPublicKey:(NSString *)publicKey {
    if (![_rsaEncryptor.key isEqualToString:publicKey]) {
        _rsaEncryptor.key = publicKey;
    }
    return [_rsaEncryptor encryptData:_aesEncryptor.key];
}

@end
