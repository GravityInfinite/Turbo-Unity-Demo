#if __has_include(<GravityEngineSDK/GravityEngineSDK.h>)
#import <GravityEngineSDK/GravityEngineSDK.h>
#else
#import "GravityEngineSDK.h"
#endif

#import <Foundation/Foundation.h>
#import <CoreTelephony/CTCarrier.h>
#import <CoreTelephony/CTTelephonyNetworkInfo.h>
#import <objc/runtime.h>
#import <WebKit/WebKit.h>

#if TARGET_OS_IOS
#import "GravityExceptionHandler.h"
#import "GEAutoTrackEvent.h"
#import "GEAutoTrackSuperProperty.h"
#import "GEEncrypt.h"
#endif

#import "GELogging.h"
#import "GEDeviceInfo.h"
#import "GEConfig.h"
#import "GESqliteDataQueue.h"
#import "GEEventModel.h"

#import "GETrackTimer.h"
#import "GESuperProperty.h"
#import "GETrackEvent.h"
#import "GETrackFirstEvent.h"
#import "GETrackOverwriteEvent.h"
#import "GETrackUpdateEvent.h"
#import "GEUserPropertyHeader.h"
#import "GEPropertyPluginManager.h"
//#import "TASessionIdPropertyPlugin.h"
#import "GEPresetPropertyPlugin.h"
#import "GEBaseEvent+H5.h"
#import "NSDate+GEFormat.h"
#import "GEEventTracker.h"
#import "GEAppLifeCycle.h"

NS_ASSUME_NONNULL_BEGIN

static NSString * const GE_APP_START_EVENT                  = @"$AppStart";
static NSString * const GE_APP_START_BACKGROUND_EVENT       = @"$AppBgStart";
static NSString * const GE_APP_END_EVENT                    = @"$AppEnd";
static NSString * const GE_APP_VIEW_EVENT                   = @"$AppView";
static NSString * const GE_APP_CLICK_EVENT                  = @"$AppClick";
static NSString * const GE_APP_CRASH_EVENT                  = @"$AppCrash";
static NSString * const GE_APP_INSTALL_EVENT                = @"$AppInstall";

static NSString * const GE_CRASH_REASON                     = @"$app_crashed_reason";
static NSString * const GE_RESUME_FROM_BACKGROUND           = @"$resume_from_background";
static NSString * const GE_START_REASON                     = @"$start_reason";
static NSString * const GE_BACKGROUND_DURATION              = @"$background_duration";

static kEDEventTypeName const GE_EVENT_TYPE_TRACK           = @"track";

static kEDEventTypeName const GE_EVENT_TYPE_USER_DEL        = @"profile_delete";
static kEDEventTypeName const GE_EVENT_TYPE_USER_ADD        = @"profile_increment";
static kEDEventTypeName const GE_EVENT_TYPE_USER_NUMBER_MAX = @"profile_number_max";
static kEDEventTypeName const GE_EVENT_TYPE_USER_NUMBER_MIN = @"profile_number_min";
static kEDEventTypeName const GE_EVENT_TYPE_USER_SET        = @"profile_set";
static kEDEventTypeName const GE_EVENT_TYPE_USER_SETONCE    = @"profile_set_once";
static kEDEventTypeName const GE_EVENT_TYPE_USER_UNSET      = @"profile_unset";
static kEDEventTypeName const GE_EVENT_TYPE_USER_APPEND     = @"profile_append";
static kEDEventTypeName const GE_EVENT_TYPE_USER_UNIQ_APPEND= @"profile_uniq_append";

#ifndef ge_dispatch_main_sync_safe
#define ge_dispatch_main_sync_safe(block)\
if (dispatch_queue_get_label(DISPATCH_CURRENT_QUEUE_LABEL) == dispatch_queue_get_label(dispatch_get_main_queue())) {\
block();\
} else {\
dispatch_sync(dispatch_get_main_queue(), block);\
}
#endif

#define kDefaultTimeFormat  @"yyyy-MM-dd HH:mm:ss"

static NSUInteger const kBatchSize = 50;
static NSUInteger const GE_PROPERTY_CRASH_LENGTH_LIMIT = 8191*2;
static NSString * const GE_JS_TRACK_SCHEME = @"gravityengine://trackEvent";

#define kModeEnumArray @"NORMAL", @"DebugOnly", @"Debug", nil

@interface GravityEngineSDK ()

#if TARGET_OS_IOS
@property (nonatomic, strong) GEAutoTrackSuperProperty *autoTrackSuperProperty;
@property (nonatomic, strong) GEEncryptManager *encryptManager;
@property (strong,nonatomic) id thirdPartyManager;
#endif

@property (atomic, copy) NSString *appid;
@property (atomic, copy) NSString *serverURL;
@property (atomic, copy, nullable) NSString *accountId;
@property (atomic, copy) NSString *identifyId;
@property (nonatomic, strong) GESuperProperty *superProperty;
@property (nonatomic, strong) GEPropertyPluginManager *propertyPluginManager;
//@property (nonatomic, strong) TASessionIdPropertyPlugin *sessionidPlugin;
@property (nonatomic, strong) GEAppLifeCycle *appLifeCycle;

@property (atomic, strong) NSMutableSet *ignoredViewTypeList;
@property (atomic, strong) NSMutableSet *ignoredViewControllers;


@property (atomic, assign, getter=isTrackPause) BOOL trackPause;
@property (atomic, assign) BOOL isEnabled;
@property (atomic, assign) BOOL isOptOut;


@property (nonatomic, strong, nullable) NSTimer *timer;

@property (nonatomic, strong) GETrackTimer *trackTimer;

@property (atomic, strong) GESqliteDataQueue *dataQueue;
@property (nonatomic, copy) GEConfig *config;
@property (nonatomic, strong) WKWebView *wkWebView;

#if TARGET_OS_IOS
- (void)autoTrackWithEvent:(GEAutoTrackEvent *)event properties:(nullable NSDictionary *)properties;
- (BOOL)isViewControllerIgnored:(UIViewController *)viewController;
- (BOOL)isAutoTrackEventTypeIgnored:(GravityEngineAutoTrackEventType)eventType;
- (BOOL)isViewTypeIgnored:(Class)aClass;
#endif

- (void)retrievePersistedData;
+ (dispatch_queue_t)ge_trackQueue;
+ (dispatch_queue_t)ge_networkQueue;
+ (id)sharedUIApplication;
- (NSInteger)saveEventsData:(NSDictionary *)data;
- (void)flushImmediately:(NSDictionary *)dataDic;
- (BOOL)hasDisabled;
- (BOOL)isValidName:(NSString *)name isAutoTrack:(BOOL)isAutoTrack;
+ (BOOL)isTrackEvent:(NSString *)eventType;
- (BOOL)checkEventProperties:(NSDictionary *)properties withEventType:(NSString *_Nullable)eventType haveAutoTrackEvents:(BOOL)haveAutoTrackEvents;
- (void)startFlushTimer;
- (double)getTimezoneOffset:(NSDate *)date timeZone:(NSTimeZone *)timeZone;
+ (NSMutableDictionary *)_getAllInstances;

+ (NSMutableDictionary *)_getAllInstances;

@end

@interface GEEventModel ()

@property (nonatomic, copy) NSString *timeString;
@property (nonatomic, assign) double zoneOffset;
@property (nonatomic, assign) TimeValueType timeValueType;
@property (nonatomic, copy) NSString *extraID;
@property (nonatomic, assign) BOOL persist;
@property (nonatomic, strong) NSDate *time;
@property (nonatomic, strong) NSTimeZone *timeZone;

- (instancetype)initWithEventName:(NSString * _Nullable)eventName;

- (instancetype _Nonnull )initWithEventName:(NSString * _Nullable)eventName eventType:(kEDEventTypeName _Nonnull )eventType;

@end

NS_ASSUME_NONNULL_END
