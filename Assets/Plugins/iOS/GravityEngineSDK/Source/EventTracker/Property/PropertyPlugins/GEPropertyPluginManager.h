//
//  GEPropertyPluginManager.h
//  GravityEngineSDK
//
//

#import <Foundation/Foundation.h>
#import "GEBaseEvent.h"

NS_ASSUME_NONNULL_BEGIN

typedef void(^GEPropertyPluginCompletion)(NSDictionary<NSString *, id> *properties);

@protocol GEPropertyPluginProtocol <NSObject>

@property(nonatomic, copy)NSString *instanceToken;

- (NSDictionary<NSString *, id> *)properties;

@optional

- (void)start;

- (GEEventType)eventTypeFilter;

- (void)asyncGetPropertyCompletion:(GEPropertyPluginCompletion)completion;

@end


@interface GEPropertyPluginManager : NSObject

- (void)registerPropertyPlugin:(id<GEPropertyPluginProtocol>)plugin;

- (NSMutableDictionary<NSString *, id> *)currentPropertiesForPluginClasses:(NSArray<Class> *)classes;

- (NSMutableDictionary<NSString *, id> *)propertiesWithEventType:(GEEventType)type;

@end

NS_ASSUME_NONNULL_END
