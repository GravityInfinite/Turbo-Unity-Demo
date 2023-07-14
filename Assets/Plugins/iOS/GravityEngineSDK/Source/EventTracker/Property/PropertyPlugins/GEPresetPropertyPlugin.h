//
//  GEPresetPropertyPlugin.h
//  GravityEngineSDK
//
//

#if TARGET_OS_IOS
#import <UIKit/UIKit.h>
#endif
#import "GEPropertyPluginManager.h"

NS_ASSUME_NONNULL_BEGIN

@interface GEPresetPropertyPlugin : NSObject<GEPropertyPluginProtocol>

@property(nonatomic, copy)NSString *instanceToken;

@property (nonatomic, strong) NSTimeZone *defaultTimeZone;

@end

NS_ASSUME_NONNULL_END
