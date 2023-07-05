//
//  GERunTime.h
//  GravityEngineSDK
//
//  When used for plug-in, get the class name and parameters through reflection
// This class is not thread-safe, pay attention to multi-threading issues when using it


#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

@interface GERunTime : NSObject

// start reason
+ (NSString *)getAppLaunchReason;

@end

NS_ASSUME_NONNULL_END
