
#if __has_include(<GravityEngineSDK/GEEventModel.h>)
#import <GravityEngineSDK/GEEventModel.h>
#else
#import "GEEventModel.h"
#endif


NS_ASSUME_NONNULL_BEGIN

@interface GEFirstEventModel : GEEventModel

- (instancetype)initWithEventName:(NSString * _Nullable)eventName;

- (instancetype)initWithEventName:(NSString * _Nullable)eventName firstCheckID:(NSString *)firstCheckID;

@end

NS_ASSUME_NONNULL_END
