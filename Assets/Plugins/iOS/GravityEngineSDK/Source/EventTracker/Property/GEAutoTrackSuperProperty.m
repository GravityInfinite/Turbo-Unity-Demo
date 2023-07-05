//
//  GEAutoTrackSuperProperty.m
//  GravityEngineSDK
//
//

#import "GEAutoTrackSuperProperty.h"
#import "GravityEngineSDKPrivate.h"

@interface GEAutoTrackSuperProperty ()
@property (atomic, strong) NSMutableDictionary<NSString *, NSDictionary *> *eventProperties;
@property (nonatomic, copy) NSDictionary *(^dynamicSuperProperties)(GravityEngineAutoTrackEventType type, NSDictionary *properties);

@end

@implementation GEAutoTrackSuperProperty

- (instancetype)init
{
    self = [super init];
    if (self) {
        self.eventProperties = [NSMutableDictionary dictionary];
    }
    return self;
}

- (void)registerSuperProperties:(NSDictionary *)properties withType:(GravityEngineAutoTrackEventType)type {
    NSDictionary<NSNumber *, NSString *> *autoTypes = @{
        @(GravityEngineEventTypeAppStart) : GE_APP_START_EVENT,
        @(GravityEngineEventTypeAppEnd) : GE_APP_END_EVENT,
        @(GravityEngineEventTypeAppClick) : GE_APP_CLICK_EVENT,
        @(GravityEngineEventTypeAppInstall) : GE_APP_INSTALL_EVENT,
        @(GravityEngineEventTypeAppViewCrash) : GE_APP_CRASH_EVENT,
        @(GravityEngineEventTypeAppViewScreen) : GE_APP_VIEW_EVENT
    };
    
    NSArray<NSNumber *> *typeKeys = autoTypes.allKeys;
    for (NSInteger i = 0; i < typeKeys.count; i++) {
        NSNumber *key = typeKeys[i];
        GravityEngineAutoTrackEventType eventType = key.integerValue;
        if ((type & eventType) == eventType) {
            NSString *eventName = autoTypes[key];
            if (properties) {
                
                NSDictionary *oldProperties = self.eventProperties[eventName];
                if (oldProperties && [oldProperties isKindOfClass:[NSDictionary class]]) {
                    NSMutableDictionary *mutiOldProperties = [oldProperties mutableCopy];
                    [mutiOldProperties addEntriesFromDictionary:properties];
                    self.eventProperties[eventName] = mutiOldProperties;
                } else {
                    self.eventProperties[eventName] = properties;
                }
                
                
                if (eventType == GravityEngineEventTypeAppStart) {
                    NSDictionary *startParam = self.eventProperties[GE_APP_START_EVENT];
                    if (startParam && [startParam isKindOfClass:[NSDictionary class]]) {
                        self.eventProperties[GE_APP_START_BACKGROUND_EVENT] = startParam;
                    }
                }
            }
        }
    }
}


- (NSDictionary *)currentSuperPropertiesWithEventName:(NSString *)eventName {
    NSDictionary *autoEventProperty = [self.eventProperties objectForKey:eventName];
    
    NSDictionary *validProperties = [GEPropertyValidator validateProperties:[autoEventProperty copy]];
    return validProperties;
}

- (void)registerDynamicSuperProperties:(NSDictionary<NSString *, id> *(^)(GravityEngineAutoTrackEventType, NSDictionary *))dynamicSuperProperties {
    @synchronized (self) {
        self.dynamicSuperProperties = dynamicSuperProperties;
    }
}

- (NSDictionary *)obtainDynamicSuperPropertiesWithType:(GravityEngineAutoTrackEventType)type currentProperties:(NSDictionary *)properties {
    @synchronized (self) {
        if (self.dynamicSuperProperties) {
            NSDictionary *result = self.dynamicSuperProperties(type, properties);
            
            NSDictionary *validProperties = [GEPropertyValidator validateProperties:[result copy]];
            return validProperties;
        }
        return nil;
    }
}

- (void)clearSuperProperties {
    self.eventProperties = [@{} mutableCopy];
}

//MARK: - Private Methods



@end
