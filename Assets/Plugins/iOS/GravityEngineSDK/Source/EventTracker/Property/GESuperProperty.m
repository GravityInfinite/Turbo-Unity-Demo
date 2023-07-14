//
//  GESuperProperty.m
//  GravityEngineSDK
//
//

#import "GESuperProperty.h"
#import "GEPropertyValidator.h"
#import "GELogging.h"
#import "GEFile.h"

@interface GESuperProperty ()
///multi-instance identifier
@property (nonatomic, copy) NSString *token;
/// static public property
@property (atomic, strong) NSDictionary *superProperties;
/// dynamic public properties
@property (nonatomic, copy) NSDictionary<NSString *, id> *(^dynamicSuperProperties)(void);
@property (nonatomic, strong) GEFile *file;
@property (nonatomic, assign) BOOL isLight;

@end

@implementation GESuperProperty

- (instancetype)initWithAppid:(NSString *)appid isLight:(BOOL)isLight {
    if (self = [super init]) {
        NSAssert(appid.length > 0, @"token cant empty");
        self.token = appid;
        self.isLight = isLight;
        if (!isLight) {
            
            self.file = [[GEFile alloc] initWithAppid:appid];
            self.superProperties = [self.file unarchiveSuperProperties];
        }
    }
    return self;
}

- (void)registerSuperProperties:(NSDictionary *)properties {
    properties = [properties copy];
    
    properties = [GEPropertyValidator validateProperties:properties];
    if (properties.count <= 0) {
        GELogError(@"%@ propertieDict error.", properties);
        return;
    }

    
    NSMutableDictionary *tmp = [NSMutableDictionary dictionaryWithDictionary:self.superProperties];
    
    [tmp addEntriesFromDictionary:properties];
    self.superProperties = [NSDictionary dictionaryWithDictionary:tmp];

    
    [self.file archiveSuperProperties:self.superProperties];
}

- (void)unregisterSuperProperty:(NSString *)property {
    NSError *error = nil;
    [GEPropertyValidator validateEventOrPropertyName:property withError:&error];
    if (error) {
        return;
    }

    NSMutableDictionary *tmp = [NSMutableDictionary dictionaryWithDictionary:self.superProperties];
    tmp[property] = nil;
    self.superProperties = [NSDictionary dictionaryWithDictionary:tmp];
    
    [self.file archiveSuperProperties:self.superProperties];
}

- (void)clearSuperProperties {
    self.superProperties = @{};
    [self.file archiveSuperProperties:self.superProperties];
}

- (NSDictionary *)currentSuperProperties {
    if (self.superProperties) {
        return [GEPropertyValidator validateProperties:[self.superProperties copy]];
    } else {
        return @{};
    }
}

- (void)registerDynamicSuperProperties:(NSDictionary<NSString *, id> *(^ _Nullable)(void))dynamicSuperProperties {
    @synchronized (self) {
        self.dynamicSuperProperties = dynamicSuperProperties;
    }
}


- (NSDictionary *)obtainDynamicSuperProperties {
    @synchronized (self) {
        if (self.dynamicSuperProperties) {
            NSDictionary *properties = self.dynamicSuperProperties();
            
            NSDictionary *validProperties = [GEPropertyValidator validateProperties:[properties copy]];
            return validProperties;
        }
        return nil;
    }
}

@end
