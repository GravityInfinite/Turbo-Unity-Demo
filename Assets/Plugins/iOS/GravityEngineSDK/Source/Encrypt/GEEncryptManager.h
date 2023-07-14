//
//  GEEncryptManager.h
//  GravityEngineSDK
//


#import <Foundation/Foundation.h>

#import "GEConfig.h"

@class GEEventRecord;

NS_ASSUME_NONNULL_BEGIN

@interface GEEncryptManager : NSObject


@property(nonatomic, assign, getter=isValid) BOOL valid;


- (instancetype)initWithConfig:(GEConfig *)config;


- (void)handleEncryptWithConfig:(NSDictionary *)encryptConfig;


- (NSDictionary *)encryptJSONObject:(NSDictionary *)obj;

@end

NS_ASSUME_NONNULL_END
