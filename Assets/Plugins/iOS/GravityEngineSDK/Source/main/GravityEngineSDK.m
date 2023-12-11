#import "GravityEngineSDKPrivate.h"

#if TARGET_OS_IOS
#import "GEAutoTrackManager.h"
//#import "TARouter.h"
#endif

#import "GECalibratedTimeWithNTP.h"
#import "GEConfig.h"
#import "GEPublicConfig.h"
#import "GEFile.h"
#import "GECheck.h"
#import "GEJSONUtil.h"
#import "NSString+GEString.h"
#import "GEPresetProperties+GEDisProperties.h"
#import "GEAppState.h"
#import "GEEventRecord.h"
#import "GEAppExtensionAnalytic.h"
#import "GEReachability.h"
#import "GEAppLifeCycle.h"
#import "GENetwork.h"
#import "GEAESEncryptor.h"
#import <Foundation/Foundation.h>
//#import "TASessionIdPropertyPlugin.h"
//#import "TASessionIdManager.h"

#import <AdServices/AAAttribution.h>


#if !__has_feature(objc_arc)
#error The GravityEngineSDK library must be compiled with ARC enabled
#endif

#define ge_force_inline __inline__ __attribute__((always_inline))

@interface GEPresetProperties (GravityEngine)

- (instancetype)initWithDictionary:(NSDictionary *)dict;
- (void)updateValuesWithDictionary:(NSDictionary *)dict;

@end

@interface GravityEngineSDK ()
@property (nonatomic, strong) GEEventTracker *eventTracker;
@property (strong,nonatomic) GEFile *file;

@end

@implementation GravityEngineSDK

static NSMutableDictionary *instances;
static NSString *defaultProjectAppid;
static GECalibratedTime *calibratedTime;
static dispatch_queue_t ge_trackQueue;

+ (nullable GravityEngineSDK *)sharedInstance {
    if (instances.count == 0) {
        GELogError(@"sharedInstance called before creating a GravityEngine instance");
        return nil;
    }
    return instances[defaultProjectAppid];
}

+ (GravityEngineSDK *)sharedInstanceWithAppid:(NSString *)appid {
    appid = appid.ge_trim;
    if (instances[appid]) {
        return instances[appid];
    } else {
        GELogError(@"sharedInstanceWithAppid called before creating a GravityEngine instance");
        return nil;
    }
}

+ (GravityEngineSDK *)startWithAppId:(NSString *)appId withUrl:(NSString *)url withConfig:(GEConfig *)config {
    appId = appId.ge_trim;
    
    NSString *name = config.name;
    if (name && [name isKindOfClass:[NSString class]] && name.length) {
        if (instances[name]) {
            return instances[name];
        } else {
            return [[self alloc] initWithAppkey:appId withServerURL:url withConfig:config];
        }
    }
    
    if (instances[appId]) {
        return instances[appId];
    } else if (![url isKindOfClass:[NSString class]] || url.length == 0) {
        return nil;
    }
    return [[self alloc] initWithAppkey:appId withServerURL:url withConfig:config];
}

+ (GravityEngineSDK *)startWithConfig:(nullable GEConfig *)config {
    return [GravityEngineSDK startWithAppId:config.appid withUrl:config.configureURL withConfig:config];
}

- (instancetype)init:(NSString *)appID {
    if (self = [super init]) {
        static dispatch_once_t onceToken;
        dispatch_once(&onceToken, ^{
            instances = [NSMutableDictionary dictionary];
            defaultProjectAppid = appID;
        });
    }
    return self;
}

+ (void)initialize {
    static dispatch_once_t GravityOnceToken;
    dispatch_once(&GravityOnceToken, ^{
        NSString *queuelabel = [NSString stringWithFormat:@"com.gravityinfinite.%p", (void *)self];
        ge_trackQueue = dispatch_queue_create([queuelabel UTF8String], DISPATCH_QUEUE_SERIAL);
    });
}

+ (dispatch_queue_t)ge_trackQueue {
    return ge_trackQueue;
}

- (instancetype)initWithAppkey:(NSString *)appid withServerURL:(NSString *)serverURL withConfig:(GEConfig *)config {
    if (self = [self init:appid]) {
        
        [GEAppState shareInstance];
        // [TASessionIdManager shareInstance];
        
        serverURL = [serverURL ge_formatUrlString];
        self.serverURL = serverURL;
        self.appid = appid;
        
        if (!config) {
            config = GEConfig.defaultGEConfig;
        }
        
        _config = [config copy];
        _config.appid = appid;
        _config.configureURL = serverURL;
        _config.accessToken = config.accessToken;
        
        instances[[self ge_getMapInstanceTag]] = self;

        // 每次启动的时候，读取本地存储的文件
        self.file = [[GEFile alloc] initWithAppid:[self ge_getMapInstanceTag]];
        [self retrievePersistedData];
        
        // 预加载
        NSString *userAgent = [self.file unarchiveUserAgent];
        if (!userAgent) {
            GELogDebug(@"user agent is nil, will request it");
            // 为nil，则开始尝试获取wkWebView的ua
            [self wkWebViewGetUserAgent:^(NSString *userAgent) {
                NSString * realUserAgent = nil;
                @try {
                    NSRegularExpression *regex = [NSRegularExpression regularExpressionWithPattern:@"Mozilla\\/[\\d.]+\\s\\(([^()]+)\\)" options:0 error:nil];
                    NSTextCheckingResult *match = [regex firstMatchInString:userAgent options:0 range:NSMakeRange(0, userAgent.length)];
                    if (match) {
                        realUserAgent = [userAgent substringWithRange:[match rangeAtIndex:1]];
                    }
                } @catch (NSException *exception) {
                    GELogError(@"userAgent regex match exception %@", exception.description);
                } @finally {
                    GELogDebug(@"user agent is %@", realUserAgent);
                    [self.file archiveUserAgent:realUserAgent];
                }
            }];
        } else {
            GELogDebug(@"user agent is ready, will not request again. %@", userAgent);
        }
        self.superProperty = [[GESuperProperty alloc] initWithAppid:[self ge_getMapInstanceTag] isLight:NO];
        
        self.propertyPluginManager = [[GEPropertyPluginManager alloc] init];
        GEPresetPropertyPlugin *presetPlugin = [[GEPresetPropertyPlugin alloc] init];
        presetPlugin.defaultTimeZone = config.defaultTimeZone;
        [self.propertyPluginManager registerPropertyPlugin:presetPlugin];
        
        NSString *instanceName = [self ge_getMapInstanceTag];
        
        _config.getInstanceName = ^NSString * _Nonnull{
            return instanceName;
        };
        
        /* remove session plugin
        TASessionIdPropertyPlugin *sessionidPlugin = [[TASessionIdPropertyPlugin alloc] init];
        sessionidPlugin.instanceToken = instanceName;
        self.sessionidPlugin = sessionidPlugin;
        [self.propertyPluginManager registerPropertyPlugin:sessionidPlugin];
         */
#if TARGET_OS_IOS

        if (_config.enableEncrypt) {
            self.encryptManager = [[GEEncryptManager alloc] initWithConfig:config];
        }
      
#elif TARGET_OS_OSX
        [_config updateConfig:^(NSDictionary * _Nonnull secretKey) {}];
#endif
        
        self.trackTimer = [[GETrackTimer alloc] init];
        
        _ignoredViewControllers = [[NSMutableSet alloc] init];
        _ignoredViewTypeList = [[NSMutableSet alloc] init];
                
        self.dataQueue = [GESqliteDataQueue sharedInstanceWithAppid:[self ge_getMapInstanceTag]];
        if (self.dataQueue == nil) {
            GELogError(@"SqliteException: init SqliteDataQueue failed");
        }
                
        if (![GEPresetProperties disableNetworkType]) {
            [[GEReachability shareInstance] startMonitoring];
        }
        
        self.eventTracker = [[GEEventTracker alloc] initWithQueue:ge_trackQueue instanceToken:[_config getMapInstanceToken]];

        [self startFlushTimer];
        
        [GEAppLifeCycle startMonitor];
        
        [self registerAppLifeCycleListener];
        
        if ([self ableMapInstanceTag]) {
            GELogInfo(@"GravityEngine %@ SDK %@ instance initialized successfully with mode: %@, Instance Name: %@,  APP ID: %@, server url: %@, device ID: %@", [[GEDeviceInfo sharedManager] libName] ,[GEDeviceInfo libVersion], [self modeEnumToString:_config.debugMode], _config.name, appid, serverURL, [self getDeviceId]);

        } else {
            GELogInfo(@"GravityEngine %@ SDK %@ instance initialized successfully with mode: %@, APP ID: %@, server url: %@, device ID: %@", [[GEDeviceInfo sharedManager] libName], [GEDeviceInfo libVersion], [self modeEnumToString:_config.debugMode], appid, serverURL, [self getDeviceId]);

        }
        
    }
    return self;
}

- (void)registerAppLifeCycleListener {
    NSNotificationCenter *notificationCenter = [NSNotificationCenter defaultCenter];

    [notificationCenter addObserver:self selector:@selector(appStateWillChangeNotification:) name:kGEAppLifeCycleStateWillChangeNotification object:nil];
    [notificationCenter addObserver:self selector:@selector(appStateDidChangeNotification:) name:kGEAppLifeCycleStateDidChangeNotification object:nil];
}

- (NSString*)modeEnumToString:(GravityEngineDebugMode)enumVal {
    NSArray *modeEnumArray = [[NSArray alloc] initWithObjects:kModeEnumArray];
    return [modeEnumArray objectAtIndex:enumVal];
}

- (BOOL)ableMapInstanceTag {
    return _config.name && [_config.name isKindOfClass:[NSString class]] && _config.name.length;
}

- (NSString *)ge_getMapInstanceTag {
    return [self.config getMapInstanceToken];
}

- (NSString *)description {
    if ([self ableMapInstanceTag]) {
        return [NSString stringWithFormat:@"<GravityEngineSDK: %p - instanceName: %@ appid: %@ serverUrl: %@>", (void *)self, _config.name, self.appid, self.serverURL];
    } else {
        return [NSString stringWithFormat:@"<GravityEngineSDK: %p - appid: %@ serverUrl: %@>", (void *)self, self.appid, self.serverURL];
    }
}

- (void)queryUserInfoWithSuccessCallback:(void(^)(NSDictionary* _Nonnull data))successCallback withErrorCallback:(CallbackWithError)errorCallback {
    NSString * clientId = [self.file unarchiveClientId];
    NSString * encodedClientId = [clientId ge_urlEncodedString];
    NSString *urlStr = [NSString stringWithFormat:@"%@/event_center/api/v1/user/get/?access_token=%@&client_id=%@", self.serverURL, _config.accessToken, encodedClientId];
    void (^block)(NSData *, NSURLResponse *, NSError *) = ^(NSData *data, NSURLResponse *response, NSError *error) {
        
        if (error || ![response isKindOfClass:[NSHTTPURLResponse class]]) {
            NSString *msg = [NSString stringWithFormat:@"get user info network failed:%@", error];
            GELogError(msg);
            if (errorCallback) {
                errorCallback([NSError errorWithDomain:@"com.gravityengine" code:-1 userInfo:@{
                    @"error": msg
                }]);
            }
            return;
        }
        NSError *err;
        if (!data) {
            if (errorCallback) {
                errorCallback([NSError errorWithDomain:@"com.gravityengine" code:-1 userInfo:@{
                    @"error": @"data is empty"
                }]);
            }
            return;
        }
        NSDictionary *ret = [NSJSONSerialization JSONObjectWithData:data options:kNilOptions error:&err];
        if (err) {
            NSString *msg = [NSString stringWithFormat:@"get user info json error:%@", err];
            GELogError(msg);
            if (errorCallback) {
                errorCallback([NSError errorWithDomain:@"com.gravityengine" code:-1 userInfo:@{
                    @"error": msg
                }]);
            }
        } else if ([ret isKindOfClass:[NSDictionary class]] && [ret[@"code"] isEqualToNumber:[NSNumber numberWithInt:0]]) {
            GELogDebug(@"get user info for %@", [ret objectForKey:@"data"]);
            if (successCallback) {
                successCallback(ret[@"data"]);
            }
        } else {
            GELogError(@"get user info failed");
            if (errorCallback) {
                errorCallback([NSError errorWithDomain:@"com.gravityengine" code:-1 userInfo:@{
                    @"error": @"get user info failed"
                }]);
            }
        }
    };
    
    NSMutableURLRequest *request = [NSMutableURLRequest requestWithURL:[NSURL URLWithString:urlStr]];
    [request setHTTPMethod:@"Get"];
    NSURLSession *session = [NSURLSession sharedSession];
    NSURLSessionDataTask *task = [session dataTaskWithRequest:request completionHandler:block];
    [task resume];
}

- (void)trackRegisterEvent {
    // 上报用户注册事件
    [self track:@"$AppRegister"];
    [self flush];
}

- (void)initializeGravityEngineWithClientId:(NSString *) clientId withUserName:(NSString *)userName withVersion:(int)version withAsaEnable:(bool)enableAsa withIdfa:(NSString *) idfa withIdfv:(NSString *)idfv withCaid1:(NSString *)caid1_md5 withCaid2:(NSString *)caid2_md5 withSyncAttribution:(bool)syncAttribution withCreateTime:(long)createTimestamp withCompany:(NSString *)company withSuccessCallback:(CallbackWithSuccess)successCallback withErrorCallback:(CallbackWithError)errorCallback{
    if (!(idfa && idfa.length > 0) && !(idfv && idfv.length > 0) && !(caid1_md5 && caid1_md5.length > 0) && !(caid2_md5 && caid2_md5.length > 0)) {
        GELogError(@"idfa/idfv/caid1_md5/caid2_md5 全部为空，请确保后续调用uploadDeviceInfo接口补报ID数据，否则会严重影响归因准确性！");
    }
    
    GENetwork *network = [[GENetwork alloc] init];
    network.debugMode = GravityEngineDebugOff;
    network.appid = _config.appid;
    network.serverURL = [NSURL URLWithString:[NSString stringWithFormat:@"%@/event_center/api/v1/user/register/?access_token=%@", _config.configureURL, _config.accessToken]];
    network.securityPolicy = _config.securityPolicy;
    
    NSMutableDictionary *deviceInfo = [NSMutableDictionary new];
    deviceInfo[@"os_name"] = @"ios";
    if(idfa) {
        deviceInfo[@"idfa"] = idfa;
    }
    if(idfv) {
        deviceInfo[@"idfv"] = idfv;
    }
    if(caid1_md5) {
        deviceInfo[@"caid1_md5"] = caid1_md5;
    }
    if(caid2_md5) {
        deviceInfo[@"caid2_md5"] = caid2_md5;
    }

    deviceInfo[@"uptime_time"] = [GEDeviceInfo bootTimeSec];
    deviceInfo[@"latest_update_time"] = [GEDeviceInfo systemUpdateTime];

    NSMutableDictionary *initializeBody = [NSMutableDictionary dictionaryWithDictionary:@{
        @"client_id": clientId,
        @"name": userName,
        @"channel": @"base_channel",
        @"version": [NSNumber numberWithInt:version],
        @"device_info": deviceInfo
    }];
    if (syncAttribution == YES) {
        initializeBody[@"need_return_attribution"] = @true;
    }
    if (createTimestamp != 0 && company) {
        initializeBody[@"history_info"] = @{
            @"create_time": [NSNumber numberWithLong:createTimestamp],
            @"company": company
        };
    }
    
    if (@available(iOS 14.3, *)) {
        if (enableAsa == YES) {
            NSError *error;
            NSString *asaToken = [AAAttribution attributionTokenWithError:&error];
            GELogDebug(@"asa token is %@", asaToken);
            // asaToken参数，注册 的时候获取，获取完成之后，传入给引力
            if (asaToken && asaToken.length > 0) {
                initializeBody[@"asa_token"] = asaToken;
            }
        }
    }
    
    void(^block)(NSDictionary * _Nonnull result, NSError * _Nullable error) = ^(NSDictionary * _Nonnull result, NSError * _Nullable error) {
        NSNumber *code = [result objectForKey:@"code"];
        GELogDebug(@"initialize respons code is %@", code);
        if ([code isEqualToNumber:@0]) {
            // 注册成功，准备开始上报各种信息
            [self.file archiveClientId:clientId];
            
            NSDictionary *responseJson = [result objectForKey:@"data"];

            if (successCallback) {
                GELogDebug(@"didFinishAdnSuccess, client id is %@, response data is %@", [self.file unarchiveClientId], responseJson);
                successCallback(responseJson);
            }
            
            // 上报用户信息
            [self user_set_once:@{
                @"$manufacturer": @"Apple",
                @"$brand": @"Apple",
                @"$os": @"iOS",
                @"$channel": @"base_channel",
                @"$model": [GEDeviceInfo sharedManager].ge_iphoneType
            }];
            
            return;
        }
        
        if (!error) {
            error = [NSError errorWithDomain:@"com.gravityengine" code:-1 userInfo:result];
        }
        
        if (errorCallback) {
            errorCallback(error);
        }
    };
    
    if ([self.file unarchiveUserAgent]) {
        // 有值，直接调用
        GELogDebug(@"user agent is ready, will use it directly.");
        [network postDataWith:initializeBody handler:block];
    } else {
        GELogDebug(@"user agent is not ready, will try to request it.");
        // 为nil，则开始尝试获取wkWebView的ua，之后调用
        [self wkWebViewGetUserAgent:^(NSString *userAgent) {
            NSString * realUserAgent = nil;
            @try {
                NSRegularExpression *regex = [NSRegularExpression regularExpressionWithPattern:@"Mozilla\\/[\\d.]+\\s\\(([^()]+)\\)" options:0 error:nil];
                NSTextCheckingResult *match = [regex firstMatchInString:userAgent options:0 range:NSMakeRange(0, userAgent.length)];
                if (match) {
                    realUserAgent = [userAgent substringWithRange:[match rangeAtIndex:1]];
                }
            } @catch (NSException *exception) {
                GELogError(@"userAgent regex match exception %@", exception.description);
            } @finally {
                GELogDebug(@"user agent is %@", realUserAgent);
                [self.file archiveUserAgent:realUserAgent];
                [network postDataWith:initializeBody handler:block];
            }
        }];
    }
}


- (void)initializeGravityEngineWithClientId:(NSString *) clientId withUserName:(NSString *)userName withVersion:(int)version withAsaEnable:(bool)enableAsa withIdfa:(NSString *) idfa withIdfv:(NSString *)idfv withCaid1:(NSString *)caid1_md5 withCaid2:(NSString *)caid2_md5 withSyncAttribution:(bool)syncAttribution withSuccessCallback:(CallbackWithSuccess)successCallback withErrorCallback:(CallbackWithError)errorCallback {
    [self initializeGravityEngineWithClientId:clientId withUserName:userName withVersion:version withAsaEnable:enableAsa withIdfa:idfa withIdfv:idfv withCaid1:caid1_md5 withCaid2:caid2_md5 withSyncAttribution:syncAttribution withCreateTime:0 withCompany:@"" withSuccessCallback:successCallback withErrorCallback:errorCallback];
}

- (void)resetClientID:(NSString *)newClientID withSuccessCallback:(CallbackWithSuccess)successCallback withErrorCallback:(CallbackWithError)errorCallback {
    NSString * clientId = [self.file unarchiveClientId];
    NSString * encodedClientId = [clientId ge_urlEncodedString];
    
    if (!clientId) {
        GELogError(@"client id is nil, will not reset client id");
        if (errorCallback) {
            errorCallback([NSError errorWithDomain:@"com.gravityengine" code:-1 userInfo:@{
                @"error": @"client id is nil, will not reset client id"
            }]);
        }
        return;
    }
    GENetwork *network = [[GENetwork alloc] init];
    network.debugMode = GravityEngineDebugOff;
    network.appid = _config.appid;
    
    network.serverURL = [NSURL URLWithString:[NSString stringWithFormat:@"%@/event_center/api/v1/user/reset_client_id/?access_token=%@&client_id=%@", _config.configureURL, _config.accessToken, encodedClientId]];
    network.securityPolicy = _config.securityPolicy;
    
    NSDictionary *initializeBody = @{
        @"new_client_id": newClientID
    };

    [network postDataWith:(initializeBody) handler:^(NSDictionary * _Nonnull result, NSError * _Nullable error) {
        GELogDebug(@"reset result is %@", result);
        if (error) {
            if (errorCallback) {
                errorCallback(error);
            }
        } else {
            NSNumber *code = [result objectForKey:@"code"];
            if ([code isEqualToNumber:@0]) {
                [self.file archiveClientId:newClientID];
                if (successCallback) {
                    successCallback(@{});
                }
            } else {
                if (errorCallback) {
                    errorCallback(error);
                }
            }
        }
    }];
}

- (void)uploadDeviceInfoWithIdfa:(NSString *) idfa withIdfv:(NSString *)idfv withCaid1:(NSString *)caid1_md5 withCaid2:(NSString *)caid2_md5 {
    NSString * clientId = [self.file unarchiveClientId];
    NSString * encodedClientId = [clientId ge_urlEncodedString];
    
    if (!clientId) {
        GELogError(@"client id is nil, will not upload device info");
        return;
    }
    if (!(idfa && idfa.length > 0) && !(idfv && idfv.length > 0) && !(caid1_md5 && caid1_md5.length > 0) && !(caid2_md5 && caid2_md5.length > 0)) {
        GELogError(@"idfa/idfv/caid1_md5/caid2_md5 全部为空！本次补报ID行为将不会上传给引力后台！");
        return;
    }
    GENetwork *network = [[GENetwork alloc] init];
    network.debugMode = GravityEngineDebugOff;
    network.appid = _config.appid;
    
    network.serverURL = [NSURL URLWithString:[NSString stringWithFormat:@"%@/event_center/api/v1/user/device_info/?access_token=%@&client_id=%@", _config.configureURL, _config.accessToken, encodedClientId]];
    network.securityPolicy = _config.securityPolicy;
    
    NSMutableDictionary *deviceInfo = [NSMutableDictionary new];
    deviceInfo[@"os_name"] = @"ios";
    if(idfa) {
        deviceInfo[@"idfa"] = idfa;
    }
    if(idfv) {
        deviceInfo[@"idfv"] = idfv;
    }
    if(caid1_md5) {
        deviceInfo[@"caid1_md5"] = caid1_md5;
    }
    if(caid2_md5) {
        deviceInfo[@"caid2_md5"] = caid2_md5;
    }
    
    NSDictionary *uploadBody = @{
        @"data": deviceInfo
    };

    [network postDataWith:(uploadBody) handler:^(NSDictionary * _Nonnull result, NSError * _Nullable error) {
        GELogDebug(@"upload device info result is %@", result);
    }];
}

- (void)trackPayEventWithAmount:(int)payAmount withPayType:(NSString *)payType withOrderId:(NSString *)orderId withPayReason:(NSString *)payReason withPayMethod:(NSString *)payMethod {
    [self trackPayOrWithdrawEvent:@"$PayEvent" withPayAmount:payAmount withPayType:payType withOrderId:orderId withPayReason:payReason withPayMethod:payMethod];
}

- (void)trackWithdrawEvent:(int)payAmount withPayType:(NSString *)payType withOrderId:(NSString *)orderId withPayReason:(NSString *)payReason withPayMethod:(NSString *)payMethod {
    [self trackPayOrWithdrawEvent:@"$UserWithdraw" withPayAmount:payAmount withPayType:payType withOrderId:orderId withPayReason:payReason withPayMethod:payMethod];
}

- (void)trackPayOrWithdrawEvent:(NSString *)eventName withPayAmount:(int)payAmount withPayType:(NSString *)payType withOrderId:(NSString *)orderId withPayReason:(NSString *)payReason withPayMethod:(NSString *)payMethod {
    [self track:eventName properties:@{
        @"$pay_type": payType,
        @"$pay_amount": [NSNumber numberWithInt:payAmount],
        @"$order_id": orderId,
        @"$pay_reason": payReason,
        @"$pay_method": payMethod
    }];
    [self flush];
}

- (void)bindTAThirdPlatformWithAccountId:(NSString *)taAccountId withDistinctId:(NSString *)taDistinctId {
    [self track:@"$BindThirdPlatform" properties:@{
        @"$ta_account_id": taAccountId,
        @"$ta_distinct_id": taDistinctId,
        @"$third_platform_type": @"ta",
    }];
    [self flush];
}

- (void)trackAdEventWithEventName:(NSString *)eventName withUninType:(NSString *)adUnionType withPlacementId:(NSString *)adPlacementId withSourceId:(NSString *)adSourceId withAdType:(NSString *)adType withAdnType:(NSString *)adAdnType withEcpm:(NSNumber *)ecpm{
    [self track:eventName properties:@{
        @"$ad_through": adUnionType,
        @"$ad_placement_id": adPlacementId,
        @"$ad_source_id": adSourceId,
        @"$ad_type": adType,
        @"$adn_type": adAdnType,
        @"$ecpm": ecpm
    }];
    [self flush];
}

- (void)trackAdLoadEventWithUninType:(NSString *)adUnionType withPlacementId:(NSString *)adPlacementId withSourceId:(NSString *)adSourceId withAdType:(NSString *)adType withAdnType:(NSString *)adAdnType{
    [self trackAdEventWithEventName:@"$AdLoad" withUninType:adUnionType withPlacementId:adPlacementId withSourceId:adSourceId withAdType:adType withAdnType:adAdnType withEcpm:@0];
    [self flush];
}

- (void)trackAdShowEventWithUninType:(NSString *)adUnionType withPlacementId:(NSString *)adPlacementId withSourceId:(NSString *)adSourceId withAdType:(NSString *)adType withAdnType:(NSString *)adAdnType withEcpm:(NSNumber *)ecpm{
    [self trackAdEventWithEventName:@"$AdShow" withUninType:adUnionType withPlacementId:adPlacementId withSourceId:adSourceId withAdType:adType withAdnType:adAdnType withEcpm:ecpm];
    [self flush];
}

- (void)trackAdSkipEventWithUninType:(NSString *)adUnionType withPlacementId:(NSString *)adPlacementId withSourceId:(NSString *)adSourceId withAdType:(NSString *)adType withAdnType:(NSString *)adAdnType withEcpm:(NSNumber *)ecpm{
    [self trackAdEventWithEventName:@"$AdSkip" withUninType:adUnionType withPlacementId:adPlacementId withSourceId:adSourceId withAdType:adType withAdnType:adAdnType withEcpm:ecpm];
    [self flush];
}

- (void)trackAdClickEventWithUninType:(NSString *)adUnionType withPlacementId:(NSString *)adPlacementId withSourceId:(NSString *)adSourceId withAdType:(NSString *)adType withAdnType:(NSString *)adAdnType withEcpm:(NSNumber *)ecpm{
    [self trackAdEventWithEventName:@"$AdClick" withUninType:adUnionType withPlacementId:adPlacementId withSourceId:adSourceId withAdType:adType withAdnType:adAdnType withEcpm:ecpm];
    [self flush];
}

- (void)trackAdPlayStartEventWithUninType:(NSString *)adUnionType withPlacementId:(NSString *)adPlacementId withSourceId:(NSString *)adSourceId withAdType:(NSString *)adType withAdnType:(NSString *)adAdnType withEcpm:(NSNumber *)ecpm{
    [self trackAdEventWithEventName:@"$AdPlayStart" withUninType:adUnionType withPlacementId:adPlacementId withSourceId:adSourceId withAdType:adType withAdnType:adAdnType withEcpm:ecpm];
    [self flush];
}

- (void)trackAdPlayEndEventWithUninType:(NSString *)adUnionType withPlacementId:(NSString *)adPlacementId withSourceId:(NSString *)adSourceId withAdType:(NSString *)adType withAdnType:(NSString *)adAdnType withEcpm:(NSNumber *)ecpm withDruation:(NSNumber *)duration withIsPlayOver:(BOOL)isPlayOver{
    [self track:@"$AdPlayEnd" properties:@{
        @"$ad_through": adUnionType,
        @"$ad_placement_id": adPlacementId,
        @"$ad_source_id": adSourceId,
        @"$ad_type": adType,
        @"$adn_type": adAdnType,
        @"$ecpm": ecpm,
        @"$duration": duration,
        @"$is_play_over": @(isPlayOver)
    }];
    [self flush];
}

+ (id)sharedUIApplication {
    return [GEAppState sharedApplication];
}

- (void)setTrackStatus: (GETrackStatus)status {
    switch (status) {
        case GETrackStatusPause: {
            GELogDebug(@"%@ switchTrackStatus: GETrackStatusPause...", self);
            [self enableTracking:NO];
            break;
        }
        case GETrackStatusStop: {
            GELogDebug(@"%@ switchTrackStatus: GETrackStatusStop...", self);
            [self doOptOutTracking];
            break;
        }
        case GETrackStatusSaveOnly: {
            GELogDebug(@"%@ switchTrackStatus: GETrackStatusSaveOnly...", self);
            self.trackPause = YES;
            self.isEnabled = YES;
            self.isOptOut = NO;
            dispatch_async(ge_trackQueue, ^{
                [self.file archiveTrackPause:YES];
                [self.file archiveIsEnabled:YES];
                [self.file archiveOptOut:NO];
            });
            break;
        }
        case GETrackStatusNormal: {
            GELogDebug(@"%@ switchTrackStatus: GETrackStatusNormal...", self);
            self.trackPause = NO;
            self.isEnabled = YES;
            self.isOptOut = NO;
            dispatch_async(ge_trackQueue, ^{
                [self.file archiveTrackPause:NO];
                [self.file archiveIsEnabled:self.isEnabled];
                [self.file archiveOptOut:NO];
            });
            [self flush];
            break;
        }
        default:
            break;
    }
}

#pragma mark - EnableTracking
- (void)enableTracking:(BOOL)enabled {
    self.isEnabled = enabled;
    dispatch_async(ge_trackQueue, ^{
        [self.file archiveIsEnabled:self.isEnabled];
    });
}

- (void)optOutTracking {
    GELogDebug(@"%@ optOutTracking...", self);
    [self doOptOutTracking];
}

- (void)optOutTrackingAndDeleteUser {
    GELogDebug(@"%@ optOutTrackingAndDeleteUser...", self);
    GEUserEventDelete *deleteEvent = [[GEUserEventDelete alloc] init];
    deleteEvent.immediately = YES;
    [self asyncUserEventObject:deleteEvent properties:nil isH5:NO];
    
    [self doOptOutTracking];
}

- (void)optInTracking {
    GELogDebug(@"%@ optInTracking...", self);
    self.isOptOut = NO;
    dispatch_async(ge_trackQueue, ^{
        [self.file archiveOptOut:NO];
    });
}

- (BOOL)hasDisabled {
    return !self.isEnabled || self.isOptOut;
}

- (void)doOptOutTracking {
    self.isOptOut = YES;
    
#if TARGET_OS_IOS
    @synchronized (self.autoTrackSuperProperty) {
        [self.autoTrackSuperProperty clearSuperProperties];
    }
#endif

    [self.superProperty registerDynamicSuperProperties:nil];

    void(^block)(void) = ^{
        [self.dataQueue deleteAll:[self ge_getMapInstanceTag]];
        [self.trackTimer clear];
        [self.superProperty clearSuperProperties];
        self.identifyId = [GEDeviceInfo sharedManager].uniqueId;
        self.accountId = nil;
    
        [self.file archiveAccountID:nil];
        [self.file archiveIdentifyId:nil];
        [self.file archiveSuperProperties:nil];
        [self.file archiveOptOut:YES];
    };
    if (dispatch_queue_get_label(DISPATCH_CURRENT_QUEUE_LABEL) == dispatch_queue_get_label(ge_trackQueue)) {
        block();
    } else {
        dispatch_async(ge_trackQueue, block);
    }
}

#pragma mark - Persistence
- (void)retrievePersistedData {
    self.accountId = [self.file unarchiveAccountID];
    self.identifyId = [self.file unarchiveIdentifyID];
    self.trackPause = [self.file unarchiveTrackPause];
    self.isEnabled = [self.file unarchiveEnabled];
    self.isOptOut  = [self.file unarchiveOptOut];
    self.config.uploadSize = [self.file unarchiveUploadSize];
    self.config.uploadInterval = [self.file unarchiveUploadInterval];
    if (self.identifyId.length == 0) {
        self.identifyId = [GEDeviceInfo sharedManager].uniqueId;
    }
    if (self.accountId.length == 0) {
        self.accountId = [self.file unarchiveAccountID];
        [self.file archiveAccountID:self.accountId];
    }
    GELogDebug(@"persistend data is %@", [self.file description]);
}

- (void)deleteAll {
    dispatch_async(ge_trackQueue, ^{
        @synchronized (GESqliteDataQueue.class) {
            [self.dataQueue deleteAll:[self ge_getMapInstanceTag]];
        }
    });
}

//MARK: - AppLifeCycle

- (void)appStateWillChangeNotification:(NSNotification *)notification {
    GEAppLifeCycleState newState = [[notification.userInfo objectForKey:kGEAppLifeCycleNewStateKey] integerValue];
   
    if (newState == GEAppLifeCycleStateEnd) {
        [self stopFlushTimer];
    }
}


- (void)appStateDidChangeNotification:(NSNotification *)notification {
    GEAppLifeCycleState newState = [[notification.userInfo objectForKey:kGEAppLifeCycleNewStateKey] integerValue];

    if (newState == GEAppLifeCycleStateStart) {
        [self startFlushTimer];

        // 更新时长统计
        NSTimeInterval systemUpTime = [GEDeviceInfo uptime];
        [self.trackTimer enterForegroundWithSystemUptime:systemUpTime];
    } else if (newState == GEAppLifeCycleStateEnd) {
        // 更新事件时长统计
        NSTimeInterval systemUpTime = [GEDeviceInfo uptime];
        [self.trackTimer enterBackgroundWithSystemUptime:systemUpTime];
        
#if TARGET_OS_IOS
        UIApplication *application = [GEAppState sharedApplication];;
        __block UIBackgroundTaskIdentifier backgroundTaskIdentifier = UIBackgroundTaskInvalid;
        void (^endBackgroundTask)(void) = ^() {
            [application endBackgroundTask:backgroundTaskIdentifier];
            backgroundTaskIdentifier = UIBackgroundTaskInvalid;
        };
        backgroundTaskIdentifier = [application beginBackgroundTaskWithExpirationHandler:endBackgroundTask];
        
        [self.eventTracker _asyncWithCompletion:endBackgroundTask];
#else
        [self.eventTracker flush];
#endif
        
    } else if (newState == GEAppLifeCycleStateTerminate) {
        dispatch_sync(ge_trackQueue, ^{});
        [self.eventTracker syncSendAllData];
    }
}

// MARK: -

- (void)setNetworkType:(GravityEngineNetworkType)type {
    if ([self hasDisabled])
        return;
    
    [self.config setNetworkType:type];
}

+ (NSString *)getNetWorkStates {
    return [[GEReachability shareInstance] networkState];
}

//MARK: - Track

- (void)track:(NSString *)event {
    [self track:event properties:nil];
}

- (void)track:(NSString *)event properties:(NSDictionary *)propertiesDict {
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Wnonnull"
    [self track:event properties:propertiesDict time:nil timeZone:nil];
#pragma clang diagnostic pop
}

// deprecated
- (void)track:(NSString *)event properties:(NSDictionary *)propertiesDict time:(NSDate *)time {
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Wnonnull"
    [self track:event properties:propertiesDict time:time timeZone:nil];
#pragma clang diagnostic pop
}

- (void)track:(NSString *)event properties:(nullable NSDictionary *)properties time:(NSDate *)time timeZone:(NSTimeZone *)timeZone {
    GETrackEvent *trackEvent = [[GETrackEvent alloc] initWithName:event];
    // GELogDebug(@"$#### track.systemUpTime: %lf", trackEvent.systemUpTime);
    [self configEventTimeValueWithEvent:trackEvent time:time timeZone:timeZone];
    [self handleTimeEvent:trackEvent];
    [self asyncTrackEventObject:trackEvent properties:properties isH5:NO];
}

- (void)trackWithEventModel:(GEEventModel *)eventModel {
    GETrackEvent *baseEvent = nil;
    if ([eventModel.eventType isEqualToString:GE_EVENT_TYPE_TRACK_FIRST]) {
        GETrackFirstEvent *trackEvent = [[GETrackFirstEvent alloc] initWithName:eventModel.eventName];
        [self configEventTimeValueWithEvent:trackEvent time:eventModel.time timeZone:eventModel.timeZone];
        trackEvent.firstCheckId = eventModel.extraID;
        baseEvent = trackEvent;
    } else if ([eventModel.eventType isEqualToString:GE_EVENT_TYPE_TRACK_UPDATE]) {
        GETrackUpdateEvent *trackEvent = [[GETrackUpdateEvent alloc] initWithName:eventModel.eventName];
        [self configEventTimeValueWithEvent:trackEvent time:eventModel.time timeZone:eventModel.timeZone];
        trackEvent.eventId = eventModel.extraID;
        baseEvent = trackEvent;
    } else if ([eventModel.eventType isEqualToString:GE_EVENT_TYPE_TRACK_OVERWRITE]) {
        GETrackOverwriteEvent *trackEvent = [[GETrackOverwriteEvent alloc] initWithName:eventModel.eventName];
        [self configEventTimeValueWithEvent:trackEvent time:eventModel.time timeZone:eventModel.timeZone];
        trackEvent.eventId = eventModel.extraID;
        baseEvent = trackEvent;
    } else if ([eventModel.eventType isEqualToString:GE_EVENT_TYPE_TRACK]) {
        GETrackEvent *trackEvent = [[GETrackEvent alloc] initWithName:eventModel.eventName];
        [self configEventTimeValueWithEvent:trackEvent time:eventModel.time timeZone:eventModel.timeZone];
        baseEvent = trackEvent;
    }
    [self asyncTrackEventObject:baseEvent properties:eventModel.properties isH5:NO];
}

- (void)trackFromAppExtensionWithAppGroupId:(NSString *)appGroupId {
    @try {
        if (appGroupId == nil || [appGroupId isEqualToString:@""]) {
            return;
        }
        
        GEAppExtensionAnalytic *analytic = [GEAppExtensionAnalytic analyticWithInstanceName:[self ge_getMapInstanceTag] appGroupId:appGroupId];
        NSArray *eventArray = [analytic readAllEvents];
        if (eventArray) {
            for (NSDictionary *dict in eventArray) {
                NSString *eventName = dict[kGEAppExtensionEventName];
                NSDictionary *properties = dict[kGEAppExtensionEventProperties];
                NSDate *time = dict[kGEAppExtensionTime];
                // track event
                if ([time isKindOfClass:NSDate.class]) {
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Wnonnull"
                    [self track:eventName properties:properties time:time timeZone:nil];
#pragma clang diagnostic pop
                } else {
                    [self track:eventName properties:properties];
                }
            }
            [analytic deleteEvents];
        }
    } @catch (NSException *exception) {
        return;
    }
}

#pragma mark - Private

- (void)asyncTrackEventObject:(GETrackEvent *)event properties:(NSDictionary *)properties isH5:(BOOL)isH5 {

    event.isEnabled = self.isEnabled;
    event.trackPause = self.isTrackPause;
    event.isOptOut = self.isOptOut;
    event.accountId = self.accountId;
    event.distinctId = self.identifyId;
    
    event.dynamicSuperProperties = [self.superProperty obtainDynamicSuperProperties];
    dispatch_async(ge_trackQueue, ^{
        [self trackEvent:event properties:properties isH5:isH5];
    });
}

- (void)asyncUserEventObject:(GEUserEvent *)event properties:(NSDictionary *)properties isH5:(BOOL)isH5 {

    event.isEnabled = self.isEnabled;
    event.trackPause = self.isTrackPause;
    event.isOptOut = self.isOptOut;
    event.accountId = self.accountId;
    event.distinctId = self.identifyId;
        
    dispatch_async(ge_trackQueue, ^{
        [self trackUserEvent:event properties:properties isH5:NO];
    });
}

- (void)configEventTimeValueWithEvent:(GEBaseEvent *)event time:(NSDate *)time timeZone:(NSTimeZone *)timeZone {
    event.timeZone = timeZone ?: self.config.defaultTimeZone;
    if (time) {
        event.time = time;
        if (timeZone == nil) {
            event.timeValueType = GEEventTimeValueTypeTimeOnly;
        } else {
            event.timeValueType = GEEventTimeValueTypeTimeAndZone;
        }
    } else {
        event.timeValueType = GEEventTimeValueTypeNone;
    }
}

+ (BOOL)isTrackEvent:(NSString *)eventType {
    return [GE_EVENT_TYPE_TRACK isEqualToString:eventType]
    || [GE_EVENT_TYPE_TRACK_FIRST isEqualToString:eventType]
    || [GE_EVENT_TYPE_TRACK_UPDATE isEqualToString:eventType]
    || [GE_EVENT_TYPE_TRACK_OVERWRITE isEqualToString:eventType]
    ;
}

#pragma mark - User

- (void)user_increment:(NSString *)propertyName andPropertyValue:(NSNumber *)propertyValue {
    [self user_increment:propertyName andPropertyValue:propertyValue withTime:nil];
}

- (void)user_increment:(NSString *)propertyName andPropertyValue:(NSNumber *)propertyValue withTime:(NSDate *)time {
    if (propertyName && propertyValue) {
        [self user_increment:@{propertyName: propertyValue} withTime:time];
    }
}

- (void)user_increment:(NSDictionary *)properties {
    [self user_increment:properties withTime:nil];
}

- (void)user_increment:(NSDictionary *)properties withTime:(NSDate *)time {
    GEUserEventAdd *event = [[GEUserEventAdd alloc] init];
    event.time = time;
    [self asyncUserEventObject:event properties:properties isH5:NO];
}

- (void)user_number_max:(NSString *)propertyName andPropertyValue:(NSNumber *)propertyValue {
    [self user_number_max:propertyName andPropertyValue:propertyValue withTime:nil];
}

- (void)user_number_max:(NSString *)propertyName andPropertyValue:(NSNumber *)propertyValue withTime:(NSDate *)time {
    if (propertyName && propertyValue) {
        [self user_number_max:@{propertyName: propertyValue} withTime:time];
    }
}

- (void)user_number_max:(NSDictionary *)properties {
    [self user_number_max:properties withTime:nil];
}

- (void)user_number_max:(NSDictionary *)properties withTime:(NSDate *)time {
    GEUserEventNumberMax *event = [[GEUserEventNumberMax alloc] init];
    event.time = time;
    [self asyncUserEventObject:event properties:properties isH5:NO];
}

- (void)user_number_min:(NSString *)propertyName andPropertyValue:(NSNumber *)propertyValue {
    [self user_number_min:propertyName andPropertyValue:propertyValue withTime:nil];
}

- (void)user_number_min:(NSString *)propertyName andPropertyValue:(NSNumber *)propertyValue withTime:(NSDate *)time {
    if (propertyName && propertyValue) {
        [self user_number_min:@{propertyName: propertyValue} withTime:time];
    }
}

- (void)user_number_min:(NSDictionary *)properties {
    [self user_number_min:properties withTime:nil];
}

- (void)user_number_min:(NSDictionary *)properties withTime:(NSDate *)time {
    GEUserEventNumberMin *event = [[GEUserEventNumberMin alloc] init];
    event.time = time;
    [self asyncUserEventObject:event properties:properties isH5:NO];
}

- (void)user_set_once:(NSDictionary *)properties {
    [self user_set_once:properties withTime:nil];
}

- (void)user_set_once:(NSDictionary *)properties withTime:(NSDate *)time {
    GEUserEventSetOnce *event = [[GEUserEventSetOnce alloc] init];
    event.time = time;
    [self asyncUserEventObject:event properties:properties isH5:NO];
}

- (void)user_set:(NSDictionary *)properties {
    [self user_set:properties withTime:nil];
}

- (void)user_set:(NSDictionary *)properties withTime:(NSDate *)time {
    GEUserEventSet *event = [[GEUserEventSet alloc] init];
    event.time = time;
    [self asyncUserEventObject:event properties:properties isH5:NO];
}

- (void)user_unset:(NSString *)propertyName {
    [self user_unset:propertyName withTime:nil];
}

- (void)user_unset:(NSString *)propertyName withTime:(NSDate *)time {
    if ([propertyName isKindOfClass:[NSString class]] && propertyName.length > 0) {
        NSDictionary *properties = @{propertyName: @0};
        GEUserEventUnset *event = [[GEUserEventUnset alloc] init];
        event.time = time;
        [self asyncUserEventObject:event properties:properties isH5:NO];
    }
}

- (void)user_delete {
    [self user_delete:nil];
}

- (void)user_delete:(NSDate *)time {
    GEUserEventDelete *event = [[GEUserEventDelete alloc] init];
    event.time = time;
    [self asyncUserEventObject:event properties:nil isH5:NO];
}

- (void)user_append:(NSDictionary<NSString *, NSArray *> *)properties {
    [self user_append:properties withTime:nil];
}

- (void)user_append:(NSDictionary<NSString *, NSArray *> *)properties withTime:(NSDate *)time {
    GEUserEventAppend *event = [[GEUserEventAppend alloc] init];
    event.time = time;
    [self asyncUserEventObject:event properties:properties isH5:NO];
}

- (void)user_uniqAppend:(NSDictionary<NSString *, NSArray *> *)properties {
    [self user_uniqAppend:properties withTime:nil];
}

- (void)user_uniqAppend:(NSDictionary<NSString *, NSArray *> *)properties withTime:(NSDate *)time {
    GEUserEventUniqueAppend *event = [[GEUserEventUniqueAppend alloc] init];
    event.time = time;
    [self asyncUserEventObject:event properties:properties isH5:NO];
}

//MARK: -

+ (void)setCustomerLibInfoWithLibName:(NSString *)libName libVersion:(NSString *)libVersion {
    if (libName.length > 0) {
        [GEDeviceInfo sharedManager].libName = libName;
    }
    if (libVersion.length > 0) {
        [GEDeviceInfo sharedManager].libVersion = libVersion;
    }
    [[GEDeviceInfo sharedManager] ge_updateData];
}

- (NSString *)getAccountId {
    return _accountId;
}

- (NSString *)getDistinctId {
    return [self.identifyId copy];
}

+ (NSString *)getSDKVersion {
    return GEPublicConfig.version;
}

- (NSString *)getDeviceId {
    return [GEDeviceInfo sharedManager].deviceId;
}


- (NSString *)getCurrentClientId {
    NSString * clientId = [self.file unarchiveClientId];
    return clientId;
}

- (void)registerDynamicSuperProperties:(NSDictionary<NSString *, id> *(^)(void)) dynamicSuperProperties {
    if ([self hasDisabled]) {
        return;
    }
    @synchronized (self.superProperty) {
        [self.superProperty registerDynamicSuperProperties:dynamicSuperProperties];
    }
}

- (void)setSuperProperties:(NSDictionary *)properties {
    if ([self hasDisabled]) {
        return;
    }
    
    dispatch_async(ge_trackQueue, ^{
        [self.superProperty registerSuperProperties:properties];
    });
}

- (void)unsetSuperProperty:(NSString *)propertyKey {
    if ([self hasDisabled]) {
        return;
    }
    dispatch_async(ge_trackQueue, ^{
        [self.superProperty unregisterSuperProperty:propertyKey];
    });
}

- (void)clearSuperProperties {
    if ([self hasDisabled]) {
        return;
    }
    dispatch_async(ge_trackQueue, ^{
        [self.superProperty clearSuperProperties];
    });
}

- (NSDictionary *)currentSuperProperties {
    return [self.superProperty currentSuperProperties];
}

- (GEPresetProperties *)getPresetProperties {
    NSMutableDictionary *presetDic = [NSMutableDictionary dictionary];

    NSDictionary *pluginProperties = [self.propertyPluginManager currentPropertiesForPluginClasses:@[GEPresetPropertyPlugin.class]];
    [presetDic addEntriesFromDictionary:pluginProperties];
    
    double offset = [[NSDate date] ge_timeZoneOffset:self.config.defaultTimeZone];
    [presetDic setObject:@(offset) forKey:@"$timezone_offset"];
    
    if (![GEPresetProperties disableNetworkType]) {
        NSString *networkType = [self.class getNetWorkStates];
        [presetDic setObject:networkType?:@"" forKey:@"$network_type"];
    }
    
    // 将安装时间转为字符串
    if (![GEPresetProperties disableInstallTime]) {
        if (presetDic[@"$install_time"] && [presetDic[@"$install_time"] isKindOfClass:[NSDate class]]) {
            NSString *install_timeString = [(NSDate *)presetDic[@"$install_time"] ge_formatWithTimeZone:self.config.defaultTimeZone formatString:kDefaultTimeFormat];
            if (install_timeString && install_timeString.length) {
                [presetDic setObject:install_timeString forKey:@"$install_time"];
            }
        }
    }
    
    static GEPresetProperties *presetProperties = nil;
    if (presetProperties == nil) {
        presetProperties = [[GEPresetProperties alloc] initWithDictionary:presetDic];
    } else {
        @synchronized (instances) {
            [presetProperties updateValuesWithDictionary:presetDic];
        }
    }
    
    return presetProperties;
}

- (void)identify:(NSString *)distinctId {
    if (![distinctId isKindOfClass:[NSString class]] || distinctId.length == 0) {
        GELogError(@"identify cannot null");
        return;
    }
    if ([self hasDisabled]) {
        return;
    }
    self.identifyId = distinctId;
    @synchronized (self.file) {
        [self.file archiveIdentifyId:distinctId];
    }
}

- (void)login:(NSString *)clientId {
    if (![clientId isKindOfClass:[NSString class]] || clientId.length == 0) {
        GELogError(@"accountId invald", clientId);
        return;
    }

    if ([self hasDisabled]) {
        return;
    }
    self.accountId = clientId;
    @synchronized (self.file) {
        [self.file archiveClientId:clientId];
        [self.file archiveAccountID:clientId];
    }
    [self track:@"$AppLogin"];
    [self flush];
}

- (void)logoutWithCompletion:(void(^)(void))completion {
    if ([self hasDisabled]) {
        return;
    }
    [self track:@"$AppLogout"];
    [self flushWithCompletion:^{
        GELogDebug(@"flush complete");
        self.accountId = nil;
        @synchronized (self.file) {
            [self.file archiveClientId:nil];
            [self.file archiveAccountID:nil];
        }
        if (completion) {
            completion();
        }
    }];
}

- (void)timeEvent:(NSString *)event {
    if ([self hasDisabled]) {
        return;
    }
    NSError *error = nil;
    [GEPropertyValidator validateEventOrPropertyName:event withError:&error];
    if (error) {
        return;
    }
    [self.trackTimer trackEvent:event withSystemUptime:[GEDeviceInfo uptime]];
}

+ (nullable NSString *)getLocalRegion {
    NSString *countryCode = [[NSLocale currentLocale] objectForKey: NSLocaleCountryCode];
    return countryCode;
}

//MARK: -

- (void)configBaseEvent:(GEBaseEvent *)event isH5:(BOOL)isH5 {
    
    if (event.timeZone == nil) {
        event.timeZone = self.config.defaultTimeZone;
    }
    
    if (event.timeValueType == GEEventTimeValueTypeNone && calibratedTime && !calibratedTime.stopCalibrate) {
        NSTimeInterval outTime = [GEDeviceInfo uptime] - calibratedTime.systemUptime;
        NSDate *serverDate = [NSDate dateWithTimeIntervalSince1970:(calibratedTime.serverTime + outTime)];
        event.time = serverDate;
    }
}

- (void)trackUserEvent:(GEUserEvent *)event properties:(NSDictionary *)properties isH5:(BOOL)isH5 {
    
    if (!event.isEnabled || event.isOptOut) {
        return;
    }
    
    if ([GEAppState shareInstance].relaunchInBackground && !self.config.trackRelaunchedInBackgroundEvents) {
        return;
    }
    
    [self configBaseEvent:event isH5:isH5];
    
    [event.properties addEntriesFromDictionary:[GEPropertyValidator validateProperties:properties validator:event]];
    
    NSDictionary *jsonObj = [event formatDateWithDict:event.jsonObject];
    
    [self.eventTracker track:jsonObj immediately:event.immediately saveOnly:event.isTrackPause];
}

- (void)trackEvent:(GETrackEvent *)event properties:(NSDictionary *)properties isH5:(BOOL)isH5 {
    
    if (!event.isEnabled || event.isOptOut) {
        return;
    }
    
    if ([GEAppState shareInstance].relaunchInBackground && !self.config.trackRelaunchedInBackgroundEvents) {
        return;
    }
    
    [self configBaseEvent:event isH5:isH5];
    
    NSError *error = nil;
    [event validateWithError:&error];
    if (error) {
        return;
    }
    
    if ([self.config.disableEvents containsObject:event.eventName]) {
        return;
    }

    
    if ([GEAppState shareInstance].relaunchInBackground) {
        event.properties[@"$relaunched_in_background"] = @YES;
    }
    
    NSMutableDictionary *pluginProperties = [self.propertyPluginManager propertiesWithEventType:event.eventType];
    
    [GEPresetProperties handleFilterDisPresetProperties:pluginProperties];
    
    NSDictionary *superProperties = [GEPropertyValidator validateProperties:self.superProperty.currentSuperProperties validator:event];
    
    NSDictionary *dynamicSuperProperties = [GEPropertyValidator validateProperties:event.dynamicSuperProperties validator:event];
    
    NSMutableDictionary *jsonObj = [NSMutableDictionary dictionary];
    
    
    if (isH5) {
        event.properties = [superProperties mutableCopy];
        [event.properties addEntriesFromDictionary:dynamicSuperProperties];
        [event.properties addEntriesFromDictionary:properties];
        [event.properties addEntriesFromDictionary:pluginProperties];
        
        jsonObj = event.jsonObject;
        
        
        if (event.h5TimeString) {
            jsonObj[@"time"] = event.h5TimeString;
        }
        if (event.h5ZoneOffSet) {
            jsonObj[@"$timezone_offset"] = event.h5ZoneOffSet;
        }
    } else {
        [event.properties addEntriesFromDictionary:pluginProperties];
        
        jsonObj = event.jsonObject;
        [event.properties addEntriesFromDictionary:superProperties];
        [event.properties addEntriesFromDictionary:dynamicSuperProperties];
#if TARGET_OS_IOS
        if ([event isKindOfClass:[GEAutoTrackEvent class]]) {
            GEAutoTrackEvent *autoEvent = (GEAutoTrackEvent *)event;
            
            NSDictionary *autoSuperProperties = [self.autoTrackSuperProperty currentSuperPropertiesWithEventName:event.eventName];
            
            autoSuperProperties = [GEPropertyValidator validateProperties:autoSuperProperties validator:autoEvent];
            
            [event.properties addEntriesFromDictionary:autoSuperProperties];
            
            
            dispatch_semaphore_t semaphore = dispatch_semaphore_create(0);
            dispatch_async(dispatch_get_main_queue(), ^{
                NSDictionary *autoDynamicSuperProperties = [self.autoTrackSuperProperty obtainDynamicSuperPropertiesWithType:autoEvent.autoTrackEventType currentProperties:event.properties];
                autoDynamicSuperProperties = [GEPropertyValidator validateProperties:autoDynamicSuperProperties validator:autoEvent];
                [event.properties addEntriesFromDictionary:autoDynamicSuperProperties];

                dispatch_semaphore_signal(semaphore);
            });
            
            dispatch_semaphore_wait(semaphore, dispatch_time(DISPATCH_TIME_NOW, (int64_t)(0.5 * NSEC_PER_SEC)));
        }
#endif
        properties = [GEPropertyValidator validateProperties:properties validator:event];
        [event.properties addEntriesFromDictionary:properties];

    }
    
    
    jsonObj = [event formatDateWithDict:jsonObj];

    [self.eventTracker track:jsonObj immediately:event.immediately saveOnly:event.isTrackPause];
}


- (void)flush {
    
    if ([self hasDisabled]) {
        return;
    }
    
    if (self.isTrackPause) {
        return;
    }
    [self.eventTracker flush];
}

- (void)flushWithCompletion:(void(^)(void))completion {
    
    if ([self hasDisabled]) {
        if (completion) {
            completion();
        }
        return;
    }
    
    if (self.isTrackPause) {
        if (completion) {
            completion();
        }
        return;
    }
    [self.eventTracker _asyncWithCompletion:completion];
}

#pragma mark - Flush control
- (void)startFlushTimer {
    [self stopFlushTimer];
    dispatch_async(dispatch_get_main_queue(), ^{
        if (self.config.uploadInterval > 0) {
            self.timer = [NSTimer scheduledTimerWithTimeInterval:[self.config.uploadInterval integerValue]
                                                          target:self
                                                        selector:@selector(flush)
                                                        userInfo:nil
                                                         repeats:YES];
        }
    });
}

- (void)stopFlushTimer {
    dispatch_async(dispatch_get_main_queue(), ^{
        if (self.timer) {
            [self.timer invalidate];
            self.timer = nil;
        }
    });
}

#if TARGET_OS_IOS

//MARK: - Thired Party

- (void)enableThirdPartySharing:(GEThirdPartyShareType)type {
    [self enableThirdPartySharing:type customMap:@{}];
}

- (void)enableThirdPartySharing:(GEThirdPartyShareType)type customMap:(NSDictionary<NSString *, NSObject *> *)customMap {
    
#if TARGET_OS_IOS
    Class TARouterCls = NSClassFromString(@"TARouter");
    // com.thinkingdata://call.service/TAThirdPartyManager.TAThirdPartyProtocol/...?params={}(value url encode)
    NSURL *url = [NSURL URLWithString:@"com.thinkingdata://call.service.thinkingdata/TAThirdPartyManager.TAThirdPartyProtocol.enableThirdPartySharing:instance:property:/"];
    if (TARouterCls && [TARouterCls respondsToSelector:@selector(canOpenURL:)] && [TARouterCls respondsToSelector:@selector(openURL:withParams:)]) {
        if ([TARouterCls performSelector:@selector(canOpenURL:) withObject:url]) {
            [TARouterCls performSelector:@selector(openURL:withParams:) withObject:url withObject:@{@"TAThirdPartyManager":@{@1:[NSNumber numberWithInteger:type],@2:self,@3:customMap}}];
        }
    }
    
#endif
}

//MARK: - Auto Track

- (void)enableAutoTrack:(GravityEngineAutoTrackEventType)eventType {
    [self _enableAutoTrack:eventType properties:nil callback:nil];
}

- (void)enableAutoTrack:(GravityEngineAutoTrackEventType)eventType properties:(NSDictionary *)properties {
    [self _enableAutoTrack:eventType properties:properties callback:nil];
}

- (void)enableAutoTrack:(GravityEngineAutoTrackEventType)eventType callback:(NSDictionary *(^)(GravityEngineAutoTrackEventType eventType, NSDictionary *properties))callback {
    [self _enableAutoTrack:eventType properties:nil callback:callback];
}

- (void)_enableAutoTrack:(GravityEngineAutoTrackEventType)eventType properties:(NSDictionary *)properties callback:(NSDictionary *(^)(GravityEngineAutoTrackEventType eventType, NSDictionary *properties))callback {
    if (self.autoTrackSuperProperty == nil) {
        self.autoTrackSuperProperty = [[GEAutoTrackSuperProperty alloc] init];
    }
    [self.autoTrackSuperProperty registerSuperProperties:properties withType:eventType];
    [self.autoTrackSuperProperty registerDynamicSuperProperties:callback];
    
    
    [self _enableAutoTrack:eventType];
}

- (void)_enableAutoTrack:(GravityEngineAutoTrackEventType)eventType {
    self.config.autoTrackEventType = eventType;
    
    
    [[GEAutoTrackManager sharedManager] trackWithAppid:[self ge_getMapInstanceTag] withOption:eventType];
}

- (void)setAutoTrackProperties:(GravityEngineAutoTrackEventType)eventType properties:(NSDictionary *)properties {
    
    if ([self hasDisabled]) {
        return;
    }
    
    if (properties == nil) {
        return;
    }
    
    @synchronized (self.autoTrackSuperProperty) {
        [self.autoTrackSuperProperty registerSuperProperties:[properties copy] withType:eventType];
    }
}

- (void)ignoreViewType:(Class)aClass {
    if ([self hasDisabled]) {
        return;
    }
    @synchronized (self.ignoredViewTypeList) {
        [self.ignoredViewTypeList addObject:aClass];
    }
}

- (BOOL)isViewTypeIgnored:(Class)aClass {
    return [_ignoredViewTypeList containsObject:aClass];
}

- (BOOL)isAutoTrackEventTypeIgnored:(GravityEngineAutoTrackEventType)eventType {
    return !(_config.autoTrackEventType & eventType);
}

- (void)ignoreAutoTrackViewControllers:(NSArray *)controllers {
    if ([self hasDisabled]) {
        return;
    }
    
    if (controllers == nil || controllers.count == 0) {
        return;
    }
    
    @synchronized (self.ignoredViewControllers) {
        [self.ignoredViewControllers addObjectsFromArray:controllers];
    }
}

- (void)autoTrackWithEvent:(GEAutoTrackEvent *)event properties:(NSDictionary *)properties {
    GELogDebug(@"$#### autoTrackWithEvent: %@", event.eventName);
    [self handleTimeEvent:event];
    [self asyncAutoTrackEventObject:event properties:properties];
}

/// Add event to event queue
- (void)asyncAutoTrackEventObject:(GEAutoTrackEvent *)event properties:(NSDictionary *)properties {
    
    event.isEnabled = self.isEnabled;
    event.trackPause = self.isTrackPause;
    event.isOptOut = self.isOptOut;
    event.accountId = self.accountId;
    event.distinctId = self.identifyId;
    
    event.dynamicSuperProperties = [self.superProperty obtainDynamicSuperProperties];
    dispatch_async(ge_trackQueue, ^{
        [self trackEvent:event properties:properties isH5:NO];
    });
}

- (BOOL)isViewControllerIgnored:(UIViewController *)viewController {
    if (viewController == nil) {
        return false;
    }
    NSString *screenName = NSStringFromClass([viewController class]);
    if (_ignoredViewControllers != nil && _ignoredViewControllers.count > 0) {
        if ([_ignoredViewControllers containsObject:screenName]) {
            return true;
        }
    }
    return false;
}

#endif

// MARK: - H5 tracking

- (void)clickFromH5:(NSString *)data {
    NSData *jsonData = [data dataUsingEncoding:NSUTF8StringEncoding];
    if (!jsonData) {
        return;
    }
    NSError *err;
    NSDictionary *eventDict = [NSJSONSerialization JSONObjectWithData:jsonData
                                                              options:NSJSONReadingMutableContainers
                                                                error:&err];
    NSString *appid = [eventDict[@"$app_id"] isKindOfClass:[NSString class]] ? eventDict[@"$app_id"] : self.appid;
    id dataArr = eventDict[@"data"];
    if (!err && [dataArr isKindOfClass:[NSArray class]]) {
        NSDictionary *dataInfo = [dataArr objectAtIndex:0];
        if (dataInfo != nil) {
            NSString *type = [dataInfo objectForKey:@"$type"];
            NSString *event_name = [dataInfo objectForKey:@"event"];
            NSString *time = [dataInfo objectForKey:@"time"];
            NSDictionary *properties = [dataInfo objectForKey:@"properties"];
            
            NSString *extraID;
            
            if ([type isEqualToString:GE_EVENT_TYPE_TRACK]) {
                extraID = [dataInfo objectForKey:@"$first_check_id"];
                if (extraID) {
                    type = GE_EVENT_TYPE_TRACK_FIRST;
                }
            } else {
                extraID = [dataInfo objectForKey:@"$event_id"];
            }
            
            NSMutableDictionary *dic = [properties mutableCopy];
            [dic removeObjectForKey:@"$account_id"];
            [dic removeObjectForKey:@"$distinct_id"];
            [dic removeObjectForKey:@"$device_id"];
            [dic removeObjectForKey:@"$lib"];
            [dic removeObjectForKey:@"$lib_version"];
            [dic removeObjectForKey:@"$screen_height"];
            [dic removeObjectForKey:@"$screen_width"];
            
            GravityEngineSDK *instance = [GravityEngineSDK sharedInstanceWithAppid:appid];
            if (instance) {
                dispatch_async(ge_trackQueue, ^{
                    [instance h5track:event_name
                              extraID:extraID
                           properties:dic
                                 type:type
                                 time:time];
                });
            } else {
                dispatch_async(ge_trackQueue, ^{
                    [self h5track:event_name
                          extraID:extraID
                       properties:dic
                             type:type
                             time:time];
                });
            }
        }
    }
}

- (void)h5track:(NSString *)eventName
        extraID:(NSString *)extraID
     properties:(NSDictionary *)propertieDict
           type:(NSString *)type
           time:(NSString *)time {
    
    if ([GravityEngineSDK isTrackEvent:type]) {
        GETrackEvent *event = nil;
        if ([type isEqualToString:GE_EVENT_TYPE_TRACK]) {
            GETrackEvent *trackEvent = [[GETrackEvent alloc] initWithName:eventName];
            event = trackEvent;
        } else if ([type isEqualToString:GE_EVENT_TYPE_TRACK_FIRST]) {
            GETrackFirstEvent *firstEvent = [[GETrackFirstEvent alloc] initWithName:eventName];
            firstEvent.firstCheckId = extraID;
            event = firstEvent;
        } else if ([type isEqualToString:GE_EVENT_TYPE_TRACK_UPDATE]) {
            GETrackUpdateEvent *updateEvent = [[GETrackUpdateEvent alloc] initWithName:eventName];
            updateEvent.eventId = extraID;
            event = updateEvent;
        } else if ([type isEqualToString:GE_EVENT_TYPE_TRACK_OVERWRITE]) {
            GETrackOverwriteEvent *overwriteEvent = [[GETrackOverwriteEvent alloc] initWithName:eventName];
            overwriteEvent.eventId = extraID;
            event = overwriteEvent;
        }
        event.h5TimeString = time;
        if ([propertieDict objectForKey:@"$timezone_offset"]) {
            event.h5ZoneOffSet = [propertieDict objectForKey:@"$timezone_offset"];
        }
        [self asyncTrackEventObject:event properties:propertieDict isH5:YES];
    } else {
        GEUserEvent *event = [[GEUserEvent alloc] initWithType:[GEBaseEvent typeWithTypeString:type]];
        [self asyncUserEventObject:event properties:propertieDict isH5:YES];
    }
}

- (double)getTimezoneOffset:(NSDate *)date timeZone:(NSTimeZone *)timeZone {
    return [date ge_timeZoneOffset:timeZone];
}

- (BOOL)showUpWebView:(id)webView WithRequest:(NSURLRequest *)request {
    if (webView == nil || request == nil || ![request isKindOfClass:NSURLRequest.class]) {
        GELogInfo(@"showUpWebView request error");
        return NO;
    }
    
    NSString *urlStr = request.URL.absoluteString;
    if (!urlStr) {
        return NO;
    }
    
    if ([urlStr rangeOfString:GE_JS_TRACK_SCHEME].length == 0) {
        return NO;
    }
    
    NSString *query = [[request URL] query];
    NSArray *queryItem = [query componentsSeparatedByString:@"="];
    
    if (queryItem.count != 2)
        return YES;
    
    NSString *queryValue = [queryItem lastObject];
    if ([urlStr rangeOfString:GE_JS_TRACK_SCHEME].length > 0) {
        if ([self hasDisabled])
            return YES;
        
        NSString *eventData = [queryValue stringByRemovingPercentEncoding];
        if (eventData.length > 0)
            [self clickFromH5:eventData];
    }
    return YES;
}

- (void)wkWebViewGetUserAgent:(void (^)(NSString *))completion {
    self.wkWebView = [[WKWebView alloc] initWithFrame:CGRectZero];
    [self.wkWebView evaluateJavaScript:@"navigator.userAgent" completionHandler:^(id __nullable userAgent, NSError * __nullable error) {
        completion(userAgent);
    }];
}

- (void)addWebViewUserAgent {
    if ([self hasDisabled])
        return;
    
    void (^setUserAgent)(NSString *userAgent) = ^void (NSString *userAgent) {
        if ([userAgent rangeOfString:@"td-sdk-ios"].location == NSNotFound) {
            userAgent = [userAgent stringByAppendingString:@" /td-sdk-ios"];
            
            NSDictionary *userAgentDic = [[NSDictionary alloc] initWithObjectsAndKeys:userAgent, @"UserAgent", nil];
            [[NSUserDefaults standardUserDefaults] registerDefaults:userAgentDic];
            [[NSUserDefaults standardUserDefaults] synchronize];
        }
    };
    
    dispatch_block_t getUABlock = ^() {
        [self wkWebViewGetUserAgent:^(NSString *userAgent) {
            setUserAgent(userAgent);
        }];
    };
    
    ge_dispatch_main_sync_safe(getUABlock);
}

#pragma mark - Logging
+ (void)setLogLevel:(GELoggingLevel)level {
    [GELogging sharedInstance].loggingLevel = level;
}

#pragma mark - Calibrate time

+ (void)calibrateTime:(NSTimeInterval)timestamp {
    calibratedTime = [GECalibratedTime sharedInstance];
    [[GECalibratedTime sharedInstance] recalibrationWithTimeInterval:timestamp/1000.];
    //NSLog(@"After version 2.8.4, the external time calibration API is discarded, and the SDK will automatically calibrate");
}

+ (void)calibrateTimeWithNtp:(NSString *)ntpServer {
    if ([ntpServer isKindOfClass:[NSString class]] && ntpServer.length > 0) {
        calibratedTime = [GECalibratedTimeWithNTP sharedInstance];
        [[GECalibratedTimeWithNTP sharedInstance] recalibrationWithNtps:@[ntpServer]];
    }
    //NSLog(@"After version 2.8.4, the external time calibration API is discarded, and the SDK will automatically calibrate");
}

// for UNITY
- (NSString *)getTimeString:(NSDate *)date {
    return [date ge_formatWithTimeZone:self.config.defaultTimeZone formatString:kDefaultTimeFormat];
}

//MARK: - Private

- (void)handleTimeEvent:(GETrackEvent *)trackEvent {
    
    BOOL isTrackDuration = [self.trackTimer isExistEvent:trackEvent.eventName];
    BOOL isEndEvent = [trackEvent.eventName isEqualToString:GE_APP_END_EVENT];
    BOOL isStartEvent = [trackEvent.eventName isEqualToString:GE_APP_START_EVENT];
    BOOL isStateInit = [GEAppLifeCycle shareInstance].state == GEAppLifeCycleStateInit;
    
    if (isStateInit) {
        trackEvent.foregroundDuration = [self.trackTimer foregroundDurationOfEvent:trackEvent.eventName isActive:YES systemUptime:trackEvent.systemUpTime];
        [self.trackTimer removeEvent:trackEvent.eventName];
        
    } else if (isStartEvent) {
        
        trackEvent.backgroundDuration = [self.trackTimer backgroundDurationOfEvent:trackEvent.eventName isActive:NO systemUptime:trackEvent.systemUpTime];
        [self.trackTimer removeEvent:trackEvent.eventName];
        
    } else if (isEndEvent) {
        
        trackEvent.foregroundDuration = [self.trackTimer foregroundDurationOfEvent:trackEvent.eventName isActive:YES systemUptime:trackEvent.systemUpTime];
        [self.trackTimer removeEvent:trackEvent.eventName];

    } else if (isTrackDuration) {
        
        
        BOOL isActive = [GEAppState shareInstance].isActive;
        
        trackEvent.foregroundDuration = [self.trackTimer foregroundDurationOfEvent:trackEvent.eventName isActive:isActive systemUptime:trackEvent.systemUpTime];
        
        trackEvent.backgroundDuration = [self.trackTimer backgroundDurationOfEvent:trackEvent.eventName isActive:isActive systemUptime:trackEvent.systemUpTime];
        
        GELogDebug(@"$####eventName: %@, foregroundDuration: %d", trackEvent.eventName, trackEvent.foregroundDuration);
        GELogDebug(@"$####eventName: %@, backgroundDuration: %d", trackEvent.eventName, trackEvent.backgroundDuration);
        
        [self.trackTimer removeEvent:trackEvent.eventName];
    } else {
         
        if (trackEvent.eventName == GE_APP_END_EVENT) {
            return;
        }
    }
}

+ (NSMutableDictionary *)_getAllInstances {
    return instances;
}

+ (void)_clearCalibratedTime {
    calibratedTime = nil;
}

- (BOOL)isValidName:(NSString *)name isAutoTrack:(BOOL)isAutoTrack {
    return YES;
}

- (BOOL)checkEventProperties:(NSDictionary *)properties withEventType:(NSString *_Nullable)eventType haveAutoTrackEvents:(BOOL)haveAutoTrackEvents {
    return YES;
}

- (void)flushImmediately:(NSDictionary *)dataDic {

}

+ (dispatch_queue_t)ge_networkQueue {
    return [GEEventTracker ge_networkQueue];
}

- (NSInteger)saveEventsData:(NSDictionary *)data {
    return [self.eventTracker saveEventsData:data];
}

- (void)testTest {
    NSString * clientId = [self.file unarchiveClientId];
    GELogError(@"client id is %@", clientId);
}

@end
