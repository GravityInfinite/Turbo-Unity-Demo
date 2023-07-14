//
//  GEPropertyPluginManager.m
//  GravityEngineSDK
//
//

#import "GEPropertyPluginManager.h"

@interface GEPropertyPluginManager ()
@property (nonatomic, strong) NSMutableArray<id<GEPropertyPluginProtocol>> *plugins;

@end


@implementation GEPropertyPluginManager

//MARK: - Public Methods

- (instancetype)init {
    self = [super init];
    if (self) {
        self.plugins = [NSMutableArray array];
    }
    return self;
}

- (void)registerPropertyPlugin:(id<GEPropertyPluginProtocol>)plugin {
    BOOL isResponds = [plugin respondsToSelector:@selector(properties)];
    NSAssert(isResponds, @"properties plugin must implement `- properties` method!");
    if (!isResponds) {
        return;
    }

    // delete old plugin
    for (id<GEPropertyPluginProtocol> object in self.plugins) {
        if (object.class == plugin.class) {
            [self.plugins removeObject:object];
            break;
        }
    }
    [self.plugins addObject:plugin];

    
    if ([plugin respondsToSelector:@selector(start)]) {
        [plugin start];
    }
}

- (NSMutableDictionary<NSString *,id> *)currentPropertiesForPluginClasses:(NSArray<Class> *)classes {
    NSArray *plugins = [self.plugins copy];
    NSMutableArray<id<GEPropertyPluginProtocol>> *matchResult = [NSMutableArray array];

    for (id<GEPropertyPluginProtocol> obj in plugins) {
        
        for (Class cla in classes) {
            if ([obj isKindOfClass:cla]) {
                [matchResult addObject:obj];
                break;
            }
        }
    }
    
    NSMutableDictionary *pluginProperties = [self propertiesWithPlugins:matchResult];

    return pluginProperties;
}

- (NSMutableDictionary<NSString *,id> *)propertiesWithEventType:(GEEventType)type {
    
    NSArray *plugins = [self.plugins copy];
    NSMutableArray<id<GEPropertyPluginProtocol>> *matchResult = [NSMutableArray array];
    for (id<GEPropertyPluginProtocol> obj in plugins) {
        if ([self isMatchedWithPlugin:obj eventType:type]) {
            [matchResult addObject:obj];
        }
    }
    return [self propertiesWithPlugins:matchResult];
}

//MARK: - Private Methods

- (NSMutableDictionary *)propertiesWithPlugins:(NSArray<id<GEPropertyPluginProtocol>> *)plugins {
    NSMutableDictionary *properties = [NSMutableDictionary dictionary];
    
    dispatch_semaphore_t semaphore;
    for (id<GEPropertyPluginProtocol> plugin in plugins) {
        if ([plugin respondsToSelector:@selector(asyncGetPropertyCompletion:)]) {
            
            semaphore = dispatch_semaphore_create(0);
            [plugin asyncGetPropertyCompletion:^(NSDictionary<NSString *,id> * _Nonnull dict) {
                [properties addEntriesFromDictionary:dict];
                dispatch_semaphore_signal(semaphore);
            }];
        }
        
        NSDictionary *pluginProperties = [plugin respondsToSelector:@selector(properties)] ? plugin.properties : nil;
        if (pluginProperties) {
            [properties addEntriesFromDictionary:pluginProperties];
        }
        if (semaphore) {
            
            dispatch_semaphore_wait(semaphore, dispatch_time(DISPATCH_TIME_NOW, (int64_t)(0.5 * NSEC_PER_SEC)));
        }
        
        semaphore = nil;
    }
    return properties;
}

- (BOOL)isMatchedWithPlugin:(id<GEPropertyPluginProtocol>)plugin eventType:(GEEventType)type {
    GEEventType eventTypeFilter;

    if (![plugin respondsToSelector:@selector(eventTypeFilter)]) {
        // If the plug-in does not implement the type filtering method, it will only be added for track type data by default, including the first event, updateable event, and rewritable event. In addition to user attribute events
        eventTypeFilter = GEEventTypeTrack | GEEventTypeTrackFirst | GEEventTypeTrackUpdate | GEEventTypeTrackOverwrite;
    } else {
        eventTypeFilter = plugin.eventTypeFilter;
    }
    
    if ((eventTypeFilter & type) == type) {
        return YES;
    }
    return NO;
}

@end
