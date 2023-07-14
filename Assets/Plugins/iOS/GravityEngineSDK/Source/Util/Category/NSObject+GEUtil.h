//
//  NSObject+GEUtil.h
//  GravityEngineSDK
//
//  Copyright © 2021 gravityengine. All rights reserved.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

@interface NSObject (GEUtil)

+ (id)performSelector:(SEL)selector onTarget:(id)target withArguments:(NSArray *)arguments;

@end

NS_ASSUME_NONNULL_END
