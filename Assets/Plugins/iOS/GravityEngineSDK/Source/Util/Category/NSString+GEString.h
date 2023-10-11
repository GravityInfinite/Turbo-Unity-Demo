//
//  NSString+GEString.h
//  GravityEngineSDK
//
//  Copyright © 2021 gravityengine. All rights reserved.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

@interface NSString (GEString)

- (NSString *)ge_trim;

- (NSString * _Nullable)ge_formatUrlString;

- (NSString *)ge_urlEncodedString;

@end

NS_ASSUME_NONNULL_END
