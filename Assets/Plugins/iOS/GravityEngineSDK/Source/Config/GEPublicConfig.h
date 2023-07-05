//
//  GEPublicConfig.h
//  GravityEngineSDK
//
//  Copyright © 2020 gravityengine. All rights reserved.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

@interface GEPublicConfig : NSObject

@property(copy,nonatomic) NSArray* controllers;
@property(copy,nonatomic) NSString* version;
+ (NSArray*)controllers;
+ (NSString*)version;

@end

NS_ASSUME_NONNULL_END
