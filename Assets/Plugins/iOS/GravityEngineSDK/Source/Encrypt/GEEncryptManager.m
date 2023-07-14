//
//  GEEncryptManager.m
//  GravityEngineSDK
//
//

#import "GEEncryptManager.h"
#import "GEEncryptProtocol.h"
#import "GESecretKey.h"
#import "GERSAEncryptorPlugin.h"
#import "NSData+GEGzip.h"
#import "GEJSONUtil.h"
#import "GEEventRecord.h"
#import "GELogging.h"


@interface GEEncryptManager ()

@property (nonatomic, strong) GEConfig *config;
@property (nonatomic, strong) id<GEEncryptProtocol> encryptor;
@property (nonatomic, copy) NSArray<id<GEEncryptProtocol>> *encryptors;
@property (nonatomic, copy) NSString *encryptedSymmetricKey;
@property (nonatomic, strong) GESecretKey *secretKey;

@end

@implementation GEEncryptManager

- (instancetype)initWithConfig:(GEConfig *)config
{
    self = [super init];
    if (self) {
        [self updateConfig:config];
    }
    return self;
}

- (void)updateConfig:(GEConfig *)config {
    self.config = config;
    
    
    NSMutableArray *encryptors = [NSMutableArray array];
    [encryptors addObject:[GERSAEncryptorPlugin new]];
    self.encryptors = encryptors;
    
    
    [self updateEncryptor:[self loadCurrentSecretKey]];
}

- (void)handleEncryptWithConfig:(NSDictionary *)encryptConfig {
    
    if (!encryptConfig || ![encryptConfig isKindOfClass:[NSDictionary class]]) {
        return;
    }
    
    if (![encryptConfig objectForKey:@"version"]) {
        return;
    }
    
    NSInteger version = [[encryptConfig objectForKey:@"version"] integerValue];
    GESecretKey *secretKey = [[GESecretKey alloc] initWithVersion:version
                                                        publicKey:encryptConfig[@"key"]
                                             asymmetricEncryption:encryptConfig[@"asymmetric"]
                                              symmetricEncryption:encryptConfig[@"symmetric"]];
    
    
    if (![secretKey isValid]) {
        return;
    }
    
    
    if (![self encryptorWithSecretKey:secretKey]) {
        return;
    }
    
    
    [self updateEncryptor:secretKey];
}

- (void)updateEncryptor:(GESecretKey *)obj {
    @try {

        GESecretKey *secretKey = obj;
        if (!secretKey.publicKey.length) {
            return;
        }

        if ([self needUpdateSecretKey:self.secretKey newSecretKey:secretKey]) {
            return;
        }

        id<GEEncryptProtocol> encryptor = [self filterEncrptor:secretKey];
        if (!encryptor) {
            return;
        }

        NSString *encryptedSymmetricKey = [encryptor encryptSymmetricKeyWithPublicKey:secretKey.publicKey];
        
        if (encryptedSymmetricKey.length) {

            self.secretKey = secretKey;

            self.encryptor = encryptor;

            self.encryptedSymmetricKey = encryptedSymmetricKey;
            
            GELogDebug(@"\n****************secretKey****************\n public key: %@ \n encrypted symmetric key: %@\n****************secretKey****************", secretKey.publicKey, encryptedSymmetricKey);
        }
    } @catch (NSException *exception) {
        GELogError(@"%@ error: %@", self, exception);
    }
}

- (GESecretKey *)loadCurrentSecretKey {
    GESecretKey *secretKey = self.config.secretKey;
    return secretKey;
}

- (BOOL)needUpdateSecretKey:(GESecretKey *)oldSecretKey newSecretKey:(GESecretKey *)newSecretKey {
    if (oldSecretKey.version != newSecretKey.version) {
        return NO;
    }
    if (![oldSecretKey.publicKey isEqualToString:newSecretKey.publicKey]) {
        return NO;
    }
    if (![oldSecretKey.symmetricEncryption isEqualToString:newSecretKey.symmetricEncryption]) {
        return NO;
    }
    if (![oldSecretKey.asymmetricEncryption isEqualToString:newSecretKey.asymmetricEncryption]) {
        return NO;
    }
    return YES;
}

- (id<GEEncryptProtocol>)filterEncrptor:(GESecretKey *)secretKey {
    id<GEEncryptProtocol> encryptor = [self encryptorWithSecretKey:secretKey];
    if (!encryptor) {
        NSString *format = @"\n You have used the [%@] key, but the corresponding encryption plugin has not been registered. \n";
        NSString *type = [NSString stringWithFormat:@"%@+%@", secretKey.asymmetricEncryption, secretKey.symmetricEncryption];
        NSString *message = [NSString stringWithFormat:format, type];
        NSAssert(NO, message);
        return nil;
    }
    return encryptor;
}

- (id<GEEncryptProtocol>)encryptorWithSecretKey:(GESecretKey *)secretKey {
    if (!secretKey) {
        return nil;
    }
    __block id<GEEncryptProtocol> encryptor;
    [self.encryptors enumerateObjectsWithOptions:NSEnumerationReverse usingBlock:^(id<GEEncryptProtocol> obj, NSUInteger idx, BOOL *stop) {
        BOOL isSameAsymmetricType = [[obj asymmetricEncryptType] isEqualToString:secretKey.asymmetricEncryption];
        BOOL isSameSymmetricType = [[obj symmetricEncryptType] isEqualToString:secretKey.symmetricEncryption];
        if (isSameAsymmetricType && isSameSymmetricType) {
            encryptor = obj;
            *stop = YES;
        }
    }];
    return encryptor;
}

- (NSDictionary *)encryptJSONObject:(NSDictionary *)obj {
    @try {
        if (!obj) {
            GELogDebug(@"Enable encryption but the input obj is invalid!");
            return nil;
        }

        if (!self.encryptor) {
            GELogDebug(@"Enable encryption but the secret key is invalid!");
            return nil;
        }

        if (![self encryptSymmetricKey]) {
            GELogDebug(@"Enable encryption but encrypt symmetric key is failed!");
            return nil;
        }

        
        NSData *jsonData = [GEJSONUtil JSONSerializeForObject:obj];

        
        NSString *encryptedString =  [self.encryptor encryptEvent:jsonData];
        if (!encryptedString) {
            GELogDebug(@"Enable encryption but encrypted input obj is invalid!");
            return nil;
        }

        
        NSMutableDictionary *secretObj = [NSMutableDictionary dictionary];
        secretObj[@"pkv"] = @(self.secretKey.version);
        secretObj[@"ekey"] = self.encryptedSymmetricKey;
        secretObj[@"payload"] = encryptedString;
        return [NSDictionary dictionaryWithDictionary:secretObj];
    } @catch (NSException *exception) {
        GELogDebug(@"%@ error: %@", self, exception);
        return nil;
    }
}


- (BOOL)encryptSymmetricKey {
    if (self.encryptedSymmetricKey) {
        return YES;
    }
    NSString *publicKey = self.secretKey.publicKey;
    self.encryptedSymmetricKey = [self.encryptor encryptSymmetricKeyWithPublicKey:publicKey];
    return self.encryptedSymmetricKey != nil;
}

- (BOOL)isValid {
    return _encryptor ? YES:NO;
}

@end
