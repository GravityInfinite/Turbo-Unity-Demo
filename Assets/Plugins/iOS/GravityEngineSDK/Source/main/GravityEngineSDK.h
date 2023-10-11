#import <Foundation/Foundation.h>

#if TARGET_OS_IOS
#import <UIKit/UIKit.h>

#if __has_include(<GravityEngineSDK/GEAutoTrackPublicHeader.h>)
#import <GravityEngineSDK/GEAutoTrackPublicHeader.h>
#else
#import "GEAutoTrackPublicHeader.h"
#endif

#elif TARGET_OS_OSX
#import <AppKit/AppKit.h>
#endif

#if __has_include(<GravityEngineSDK/GEFirstEventModel.h>)
#import <GravityEngineSDK/GEFirstEventModel.h>
#else
#import "GEFirstEventModel.h"
#endif

#if __has_include(<GravityEngineSDK/GEEditableEventModel.h>)
#import <GravityEngineSDK/GEEditableEventModel.h>
#else
#import "GEEditableEventModel.h"
#endif


#if __has_include(<GravityEngineSDK/GEConfig.h>)
#import <GravityEngineSDK/GEConfig.h>
#else
#import "GEConfig.h"
#endif

#if __has_include(<GravityEngineSDK/GEPresetProperties.h>)
#import <GravityEngineSDK/GEPresetProperties.h>
#else
#import "GEPresetProperties.h"
#endif


NS_ASSUME_NONNULL_BEGIN

/**
 GravityEngine API
 
 ## Initialization
 
 ```objective-c
 GravityEngineSDK *instance = [GravityEngineSDK startWithAppId:@"YOUR_APPID" withUrl:@"YOUR_SERVER_URL"];
 ```
 
 ## Track Event
 
 ```objective-c
 instance.track("some_event");
 ```
or
 ```objective-c
 [[GravityEngineSDK sharedInstanceWithAppid:@"YOUR_APPID"] track:@"some_event"];
 ```
 If you only have one instance in your project, you can also use
 ```objective-c
 [[GravityEngineSDK sharedInstance] track:@"some_event"];
 ```

 */

typedef void (^CallbackWithSuccess)(NSDictionary *response);

typedef void (^CallbackWithError)(NSError * error);

@interface GravityEngineSDK : NSObject

#pragma mark - Tracking

/**
 Get default instance

 @return SDK instance
 */
+ (nullable GravityEngineSDK *)sharedInstance;

/**
  Get one instance according to appid or instanceName

  @param appid APP ID or instanceName
  @return SDK instance
  */
+ (nullable GravityEngineSDK *)sharedInstanceWithAppid:(NSString *)appid;

/**
  Initialization method
  After the SDK initialization is complete, the saved instance can be obtained through this api

  @param config initialization configuration
  @return one instance
  */
+ (GravityEngineSDK *)startWithConfig:(nullable GEConfig *)config;

/**
  Initialization method
  After the SDK initialization is complete, the saved instance can be obtained through this api

  @param appId appId
  @param url server url
  @param config initialization configuration object
  @return one instance
  */
+ (GravityEngineSDK *)startWithAppId:(NSString *)appId withUrl:(NSString *)url withConfig:(nullable GEConfig *)config;


#pragma mark - Other API
/**
 register GravityEngine
 */
- (void)registerGravityEngineWithClientId:(NSString *) clientId withUserName:(NSString *)userName withVersion:(int)version withAsaEnable:(bool)enableAsa withIdfa:(NSString *) idfa withIdfv:(NSString *)idfv withCaid1:(NSString *)caid1_md5 withCaid2:(NSString *)caid2_md5 withSyncAttribution:(bool)syncAttribution withSuccessCallback:(CallbackWithSuccess)successCallback withErrorCallback:(CallbackWithError)errorCallback;

- (void)queryUserInfoWithSuccessCallback:(void(^)(NSDictionary* _Nonnull data))successCallback withErrorCallback:(CallbackWithError)errorCallback;

- (void)resetClientID:(NSString *) newClientID withSuccessCallback:(CallbackWithSuccess)successCallback withErrorCallback:(CallbackWithError)errorCallback;

/**
 * 上报付费事件
 *
 * @param payAmount  付费金额 单位为分
 * @param payType    货币类型 按照国际标准组织ISO 4217中规范的3位字母，例如CNY人民币、USD美金等
 * @param orderId    订单号
 * @param payReason  付费原因 例如：购买钻石、办理月卡
 * @param payMethod  付费方式 例如：支付宝、微信、银联等
 */
- (void)trackPayEventWithAmount:(int)payAmount withPayType:(NSString *)payType withOrderId:(NSString *)orderId withPayReason:(NSString *)payReason withPayMethod:(NSString *)payMethod;

/**
 * 上报提现事件
 *
 * @param payAmount  提现金额 单位为分
 * @param payType    货币类型 按照国际标准组织ISO 4217中规范的3位字母，例如CNY人民币、USD美金等
 * @param orderId    订单号
 * @param payReason  提现原因 例如：用户首次提现、用户抽奖提现
 * @param payMethod  提现支付方式 例如：支付宝、微信、银联等
 */
- (void)trackWithdrawEvent:(int)payAmount withPayType:(NSString *)payType withOrderId:(NSString *)orderId withPayReason:(NSString *)payReason withPayMethod:(NSString *)payMethod;

/**
 * 上报广告事件
 *
 * @param adUnionType   广告聚合平台类型  （取值为：topon、gromore、admore、self，分别对应Topon、Gromore、Admore、自建聚合）
 * @param adPlacementId 广告瀑布流ID
 * @param adSourceId    广告源ID
 * @param adType        广告类型 （取值为：reward、banner、 native 、interstitial、 splash ，分别对应激励视频广告、横幅广告、信息流广告、插屏广告、开屏广告）
 * @param adAdnType       广告平台类型（取值为：csj、gdt、ks、 mint 、baidu，分别对应为穿山甲、优量汇、快手联盟、Mintegral、百度联盟）
 * @param ecpm          预估ECPM价格（单位为元）
 * @param duration      广告播放时长（单位为秒）
 * @param isPlayOver    广告是否播放完毕
 */
- (void)trackAdLoadEventWithUninType:(NSString *)adUnionType withPlacementId:(NSString *)adPlacementId withSourceId:(NSString *)adSourceId withAdType:(NSString *)adType withAdnType:(NSString *)adAdnType;

- (void)trackAdShowEventWithUninType:(NSString *)adUnionType withPlacementId:(NSString *)adPlacementId withSourceId:(NSString *)adSourceId withAdType:(NSString *)adType withAdnType:(NSString *)adAdnType withEcpm:(NSNumber *)ecpm;

- (void)trackAdSkipEventWithUninType:(NSString *)adUnionType withPlacementId:(NSString *)adPlacementId withSourceId:(NSString *)adSourceId withAdType:(NSString *)adType withAdnType:(NSString *)adAdnType withEcpm:(NSNumber *)ecpm;

- (void)trackAdClickEventWithUninType:(NSString *)adUnionType withPlacementId:(NSString *)adPlacementId withSourceId:(NSString *)adSourceId withAdType:(NSString *)adType withAdnType:(NSString *)adAdnType withEcpm:(NSNumber *)ecpm;

- (void)trackAdPlayStartEventWithUninType:(NSString *)adUnionType withPlacementId:(NSString *)adPlacementId withSourceId:(NSString *)adSourceId withAdType:(NSString *)adType withAdnType:(NSString *)adAdnType withEcpm:(NSNumber *)ecpm;

- (void)trackAdPlayEndEventWithUninType:(NSString *)adUnionType withPlacementId:(NSString *)adPlacementId withSourceId:(NSString *)adSourceId withAdType:(NSString *)adType withAdnType:(NSString *)adAdnType withEcpm:(NSNumber *)ecpm withDruation:(NSNumber *)duration withIsPlayOver:(BOOL)isPlayOver;


/**
 * 绑定数数账号
 *
 * @param taAccountId    数数的account_id
 * @param taDistinctId  数数的distinct_id
 */
- (void)bindTAThirdPlatformWithAccountId:(NSString *)taAccountId withDistinctId:(NSString *)taDistinctId;

#pragma mark - Action Track

/**
 Track Events

 @param event         event name
 */
- (void)track:(NSString *)event;


/**
 Track Events

 @param event         event name
 @param propertieDict event properties
 */
- (void)track:(NSString *)event properties:(nullable NSDictionary *)propertieDict;

/**
 Track Events

 @param event         event name
 @param propertieDict event properties
 @param time          event trigger time
 */
- (void)track:(NSString *)event properties:(nullable NSDictionary *)propertieDict time:(NSDate *)time __attribute__((deprecated("please use track:properties:time:timeZone: method")));

/**
 Track Events
 
  @param event event name
  @param propertieDict event properties
  @param time event trigger time
  @param timeZone event trigger time time zone
  */
- (void)track:(NSString *)event properties:(nullable NSDictionary *)propertieDict time:(NSDate *)time timeZone:(NSTimeZone *)timeZone;

/**
 Track Events
 
  @param eventModel event Model
  */
- (void)trackWithEventModel:(GEEventModel *)eventModel;

/**
 Get the events collected in the App Extension and report them
 
  @param appGroupId The app group id required for data sharing
  */
- (void)trackFromAppExtensionWithAppGroupId:(NSString *)appGroupId;

#pragma mark -

/**
 Timing Events
 Record the event duration, call this method to start the timing, stop the timing when the target event is uploaded, and add the attribute #duration to the event properties, in seconds.
 */
- (void)timeEvent:(NSString *)event;

/**
 Identify
 Set the distinct ID to replace the default UUID distinct ID.
 */
- (void)identify:(NSString *)distinctId;

/**
 Get Distinctid
 Get a visitor ID: The #distinct_id value in the reported data.
 */
- (NSString *)getDistinctId;

/**
 Get sdk version
 */
+ (NSString *)getSDKVersion;

/**
 Login
 Set the account ID. Each setting overrides the previous value. Login events will be uploaded.

 @param clientId client ID
 */
- (void)login:(NSString *)clientId;

/**
 Logout with completion
 Clearing the account ID and upload user logout events.
 */
- (void)logoutWithCompletion:(void(^)(void))completion;

/**
 User_Set
 Sets the user property, replacing the original value with the new value if the property already exists.

 @param properties user properties
 */
- (void)user_set:(NSDictionary *)properties;

/**
 User_Set

 @param properties user properties
 @param time event trigger time
*/
- (void)user_set:(NSDictionary *)properties withTime:(NSDate * _Nullable)time;

/**
 User_Unset
 
 @param propertyName user properties
 */
- (void)user_unset:(NSString *)propertyName;

/**
 User_Unset
 Reset user properties.

 @param propertyName user properties
 @param time event trigger time
*/
- (void)user_unset:(NSString *)propertyName withTime:(NSDate * _Nullable)time;

/**
 User_SetOnce
 Sets a single user attribute, ignoring the new attribute value if the attribute already exists.

 @param properties user properties
 */
- (void)user_set_once:(NSDictionary *)properties;

/**
 User_SetOnce

 @param properties user properties
 @param time event trigger time
*/
- (void)user_set_once:(NSDictionary *)properties withTime:(NSDate * _Nullable)time;

/**
 User_Add
 Adds the numeric type user attributes.

 @param properties user properties
 */
- (void)user_increment:(NSDictionary *)properties;

/**
 User_Add

 @param properties user properties
 @param time event trigger time
*/
- (void)user_increment:(NSDictionary *)properties withTime:(NSDate * _Nullable)time;

/**
 User_Add

  @param propertyName  propertyName
  @param propertyValue propertyValue
 */
- (void)user_increment:(NSString *)propertyName andPropertyValue:(NSNumber *)propertyValue;

/**
 User_Add

 @param propertyName  propertyName
 @param propertyValue propertyValue
 @param time event trigger time
*/
- (void)user_increment:(NSString *)propertyName andPropertyValue:(NSNumber *)propertyValue withTime:(NSDate * _Nullable)time;

/**
 User_Number_Max
 Get max number and save it

 @param properties user properties
 */
- (void)user_number_max:(NSDictionary *)properties;

/**
 User_Number_Max

 @param properties user properties
 @param time event trigger time
*/
- (void)user_number_max:(NSDictionary *)properties withTime:(NSDate * _Nullable)time;

/**
 User_Number_Max

  @param propertyName  propertyName
  @param propertyValue propertyValue
 */
- (void)user_number_max:(NSString *)propertyName andPropertyValue:(NSNumber *)propertyValue;

/**
 User_Number_Max

 @param propertyName  propertyName
 @param propertyValue propertyValue
 @param time event trigger time
*/
- (void)user_number_max:(NSString *)propertyName andPropertyValue:(NSNumber *)propertyValue withTime:(NSDate * _Nullable)time;

/**
 User_Number_Min
 Get min number and save it

 @param properties user properties
 */
- (void)user_number_min:(NSDictionary *)properties;

/**
 User_Number_Min

 @param properties user properties
 @param time event trigger time
*/
- (void)user_number_min:(NSDictionary *)properties withTime:(NSDate * _Nullable)time;

/**
 User_Number_Min

  @param propertyName  propertyName
  @param propertyValue propertyValue
 */
- (void)user_number_min:(NSString *)propertyName andPropertyValue:(NSNumber *)propertyValue;

/**
 User_Number_Min

 @param propertyName  propertyName
 @param propertyValue propertyValue
 @param time event trigger time
*/
- (void)user_number_min:(NSString *)propertyName andPropertyValue:(NSNumber *)propertyValue withTime:(NSDate * _Nullable)time;

/**
 User_Delete
 Delete the user attributes,This operation is not reversible and should be performed with caution.
 */
- (void)user_delete;

/**
 User_Delete
 
 @param time event trigger time
 */
- (void)user_delete:(NSDate * _Nullable)time;

/**
 User_Append
 Append a user attribute of the List type.
 
 @param properties user properties
*/
- (void)user_append:(NSDictionary<NSString *, NSArray *> *)properties;

/**
 User_Append
 The element appended to the library needs to be done to remove the processing,and then import.
 
 @param properties user properties
 @param time event trigger time
*/
- (void)user_append:(NSDictionary<NSString *, NSArray *> *)properties withTime:(NSDate * _Nullable)time;

/**
 User_UniqAppend
 
 @param properties user properties
*/
- (void)user_uniqAppend:(NSDictionary<NSString *, NSArray *> *)properties;

/**
 User_UniqAppend
 
 @param properties user properties
 @param time event trigger time
*/
- (void)user_uniqAppend:(NSDictionary<NSString *, NSArray *> *)properties withTime:(NSDate * _Nullable)time;

+ (void)setCustomerLibInfoWithLibName:(NSString *)libName libVersion:(NSString *)libVersion;

/**
 Static Super Properties
 Set the public event attribute, which will be included in every event uploaded after that. The public event properties are saved without setting them each time.
  *
 */
- (void)setSuperProperties:(NSDictionary *)properties;

/**
 Unset Super Property
 Clears a public event attribute.
 */
- (void)unsetSuperProperty:(NSString *)property;

/**
 Clear Super Properties
 Clear all public event attributes.
 */
- (void)clearSuperProperties;

/**
 Get Static Super Properties
 Gets the public event properties that have been set.
 */
- (NSDictionary *)currentSuperProperties;

/**
 Dynamic super properties
 Set dynamic public properties. Each event uploaded after that will contain a public event attribute.
 */
- (void)registerDynamicSuperProperties:(NSDictionary<NSString *, id> *(^)(void))dynamicSuperProperties;

/**
 Gets prefabricated properties for all events.
 */
- (GEPresetProperties *)getPresetProperties;

/**
 Set the network conditions for uploading. By default, the SDK will set the network conditions as 3G, 4G and Wifi to upload data
 */
- (void)setNetworkType:(GravityEngineNetworkType)type;

#if TARGET_OS_IOS

/**
 Enable Auto-Tracking

 @param eventType Auto-Tracking type

 */
- (void)enableAutoTrack:(GravityEngineAutoTrackEventType)eventType;

/**
 Enable the auto tracking function.

 @param eventType  Auto-Tracking type
 @param properties properties
 */
- (void)enableAutoTrack:(GravityEngineAutoTrackEventType)eventType properties:(NSDictionary *)properties;

/**
 Enable the auto tracking function.

 @param eventType  Auto-Tracking type
 @param callback callback
 In the callback, eventType indicates the type of automatic collection, properties indicates the event properties before storage, and this block can return a dictionary for adding new properties
 */
- (void)enableAutoTrack:(GravityEngineAutoTrackEventType)eventType callback:(NSDictionary *(^)(GravityEngineAutoTrackEventType eventType, NSDictionary *properties))callback;

/**
 Set and Update the value of a custom property for Auto-Tracking
 
 @param eventType  A list of GravityEngineAutoTrackEventType, indicating the types of automatic collection events that need to be enabled
 @param properties properties
 */
- (void)setAutoTrackProperties:(GravityEngineAutoTrackEventType)eventType properties:(NSDictionary *)properties;

/**
 Ignore the Auto-Tracking of a page

 @param controllers Ignore the name of the UIViewController
 */
- (void)ignoreAutoTrackViewControllers:(NSArray *)controllers;

/**
 Ignore the Auto-Tracking  of click event

 @param aClass ignored controls  Class
 */
- (void)ignoreViewType:(Class)aClass;

#endif

//MARK: -

/**
 Get DeviceId
 */
- (NSString *)getDeviceId;

- (NSString *)getCurrentClientId;

/**
 H5 is connected with the native APP SDK and used in conjunction with the addWebViewUserAgent interface

 @param webView webView
 @param request NSURLRequest request
 @return YES：Process this request NO: This request has not been processed
 */
- (BOOL)showUpWebView:(id)webView WithRequest:(NSURLRequest *)request;

/**
 When connecting data with H5, you need to call this interface to configure UserAgent
 */
//- (void)addWebViewUserAgent;

/**
 Set Log level

 */
+ (void)setLogLevel:(GELoggingLevel)level;

/**
 Empty the cache queue. When this api is called, the data in the current cache queue will attempt to be reported.
 If the report succeeds, local cache data will be deleted.
 */
- (void)flush;

- (void)flushWithCompletion:(void(^)(void))completion;

- (void)testTest;

/**
 Switch reporting status

 @param status GETrackStatus reporting status
 */
- (void)setTrackStatus: (GETrackStatus)status;

+ (void)calibrateTimeWithNtp:(NSString *)ntpServer;

+ (void)calibrateTime:(NSTimeInterval)timestamp;

- (NSString *)getTimeString:(NSDate *)date;

#if TARGET_OS_IOS
- (void)enableThirdPartySharing:(GEThirdPartyShareType)type;

- (void)enableThirdPartySharing:(GEThirdPartyShareType)type customMap:(NSDictionary<NSString *, NSObject *> *)customMap;
#endif

+ (nullable NSString *)getLocalRegion;

@end

NS_ASSUME_NONNULL_END
