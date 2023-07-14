//
//  GEOverwriteEventModel.h
//  GravityEngineSDK
//
//

#if __has_include(<GravityEngineSDK/GEEventModel.h>)
#import <GravityEngineSDK/GEEventModel.h>
#else
#import "GEEventModel.h"
#endif

NS_ASSUME_NONNULL_BEGIN

@interface GEOverwriteEventModel : GEEventModel

- (instancetype)initWithEventName:(NSString *)eventName eventID:(NSString *)eventID;

@end

NS_ASSUME_NONNULL_END
