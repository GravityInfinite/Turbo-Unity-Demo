//
//  GEAppState.m
//  GravityEngineSDK
//
//  Copyright © 2021 gravityengine. All rights reserved.
//

#import "GEAppState.h"

#if TARGET_OS_IOS
#import <UIKit/UIKit.h>
#endif

NSString *_ge_lastKnownState;

@implementation GEAppState

+ (instancetype)shareInstance {
    static dispatch_once_t onceToken;
    static GEAppState *appState;
    dispatch_once(&onceToken, ^{
        appState = [GEAppState new];
    });
    return appState;
}

+ (id)sharedApplication {
    
#if TARGET_OS_IOS

    if ([self runningInAppExtension]) {
      return nil;
    }
    return [[UIApplication class] performSelector:@selector(sharedApplication)];
    
#endif
    return nil;
}

+ (BOOL)runningInAppExtension {
#if TARGET_OS_IOS
    return [[[[NSBundle mainBundle] bundlePath] pathExtension] isEqualToString:@"appex"];
#endif
    return NO;
}

@end
