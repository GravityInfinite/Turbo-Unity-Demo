//
//  GEAppLifeCycle.m
//  GravityEngineSDK
//
//

#import "GEAppLifeCycle.h"
#import "GEAppState.h"

#if TARGET_OS_IOS
#import <UIKit/UIKit.h>
#endif

#if __has_include(<GravityEngineSDK/GELogging.h>)
#import <GravityEngineSDK/GELogging.h>
#else
#import "GELogging.h"
#endif

NSNotificationName const kGEAppLifeCycleStateWillChangeNotification = @"com.gravityinfinite.GEAppLifeCycleStateWillChange";
NSNotificationName const kGEAppLifeCycleStateDidChangeNotification = @"com.gravityinfinite.GEAppLifeCycleStateDidChange";
NSString * const kGEAppLifeCycleNewStateKey = @"new";
NSString * const kGEAppLifeCycleOldStateKey = @"old";


@interface GEAppLifeCycle ()
/// status
@property (nonatomic, assign) GEAppLifeCycleState state;

@end

@implementation GEAppLifeCycle

+ (void)startMonitor {
    [GEAppLifeCycle shareInstance];
}

+ (instancetype)shareInstance {
    static dispatch_once_t onceToken;
    static GEAppLifeCycle *appLifeCycle = nil;
    dispatch_once(&onceToken, ^{
        appLifeCycle = [[GEAppLifeCycle alloc] init];
    });
    return appLifeCycle;
}

- (instancetype)init {
    self = [super init];
    if (self) {
        _state = GEAppLifeCycleStateInit;
        [self registerListeners];
        [self setupLaunchedState];
    }
    return self;
}

- (void)dealloc {
    [[NSNotificationCenter defaultCenter] removeObserver:self];
}

- (void)registerListeners {
    if ([GEAppState runningInAppExtension]) {
        return;
    }

    NSNotificationCenter *notificationCenter = [NSNotificationCenter defaultCenter];
#if TARGET_OS_IOS
    [notificationCenter addObserver:self
                           selector:@selector(applicationDidBecomeActive:)
                               name:UIApplicationDidBecomeActiveNotification
                             object:nil];
    
    [notificationCenter addObserver:self
                           selector:@selector(applicationDidEnterBackground:)
                               name:UIApplicationDidEnterBackgroundNotification
                             object:nil];
    
    [notificationCenter addObserver:self
                           selector:@selector(applicationWillTerminate:)
                               name:UIApplicationWillTerminateNotification
                             object:nil];

#elif TARGET_OS_OSX

//    [notificationCenter addObserver:self
//                           selector:@selector(applicationDidFinishLaunching:)
//                               name:NSApplicationDidFinishLaunchingNotification
//                             object:nil];
//
//    [notificationCenter addObserver:self
//                           selector:@selector(applicationDidBecomeActive:)
//                               name:NSApplicationDidBecomeActiveNotification
//                             object:nil];
//    [notificationCenter addObserver:self
//                           selector:@selector(applicationDidResignActive:)
//                               name:NSApplicationDidResignActiveNotification
//                             object:nil];
//
//    [notificationCenter addObserver:self
//                           selector:@selector(applicationWillTerminate:)
//                               name:NSApplicationWillTerminateNotification
//                             object:nil];
#endif
}

- (void)setupLaunchedState {
    if ([GEAppState runningInAppExtension]) {
        return;
    }
    
    dispatch_block_t mainThreadBlock = ^(){
#if TARGET_OS_IOS
        UIApplication *application = [GEAppState sharedApplication];
        BOOL isAppStateBackground = application.applicationState == UIApplicationStateBackground;
#else
        BOOL isAppStateBackground = NO;
#endif
        [GEAppState shareInstance].relaunchInBackground = isAppStateBackground;

        self.state = GEAppLifeCycleStateStart;
    };

    if (@available(iOS 13.0, *)) {
        // The reason why iOS 13 and above modify the status in the block of the asynchronous main queue:+
        // 1. Make sure that the initialization of the SDK has been completed before sending the appstatus change notification. This can ensure that the public properties have been set when the automatic collection management class sends the app_start event (in fact, it can also be achieved by listening to UIApplicationDidFinishLaunchingNotification)
        // 2. In a project that contains SceneDelegate, it is accurate to delay obtaining applicationState (obtaining by listening to UIApplicationDidFinishLaunchingNotification is inaccurate)
        dispatch_async(dispatch_get_main_queue(), mainThreadBlock);
    } else {
        // iOS 13 and below handle background wakeup and cold start (non-delayed initialization) by listening to the notification of UIApplicationDidFinishLaunchingNotification:
        // 1. When iOS 13 or later wakes up in the background, the block of the asynchronous main queue will not be executed. So you need to listen to UIApplicationDidFinishLaunchingNotification at the same time
        // 2. iOS 13 and below will not contain SceneDelegate
#if TARGET_OS_IOS
        [[NSNotificationCenter defaultCenter] addObserver:self
                                                 selector:@selector(applicationDidFinishLaunching:)
                                                     name:UIApplicationDidFinishLaunchingNotification
                                                   object:nil];
#endif
        // Handle cold start below iOS 13, where the client delays initialization. UIApplicationDidFinishLaunchingNotification notification has been missed when lazy initialization
        dispatch_async(dispatch_get_main_queue(), mainThreadBlock);
    }
}

//MARK: - Notification Action

- (void)applicationDidFinishLaunching:(NSNotification *)notification {
#if TARGET_OS_IOS
    UIApplication *application = [GEAppState sharedApplication];
    BOOL isAppStateBackground = application.applicationState == UIApplicationStateBackground;
#else
    BOOL isAppStateBackground = NO;
#endif

    [GEAppState shareInstance].relaunchInBackground = isAppStateBackground;
    
    self.state = GEAppLifeCycleStateStart;
}

- (void)applicationDidBecomeActive:(NSNotification *)notification {
    GELogDebug(@"application did become active");

#if TARGET_OS_IOS
    if (![notification.object isKindOfClass:[UIApplication class]]) {
        return;
    }

    UIApplication *application = (UIApplication *)notification.object;
    if (application.applicationState != UIApplicationStateActive) {
        return;
    }
#elif TARGET_OS_OSX
    if (![notification.object isKindOfClass:[NSApplication class]]) {
        return;
    }

    NSApplication *application = (NSApplication *)notification.object;
    if (!application.isActive) {
        return;
    }
#endif

    [GEAppState shareInstance].relaunchInBackground = NO;

    self.state = GEAppLifeCycleStateStart;
}

#if TARGET_OS_IOS
- (void)applicationDidEnterBackground:(NSNotification *)notification {
    GELogDebug(@"application did enter background");
    
    if (![notification.object isKindOfClass:[UIApplication class]]) {
        return;
    }

    UIApplication *application = (UIApplication *)notification.object;
    if (application.applicationState != UIApplicationStateBackground) {
        return;
    }

    self.state = GEAppLifeCycleStateEnd;
}

#elif TARGET_OS_OSX
- (void)applicationDidResignActive:(NSNotification *)notification {
    GELogDebug(@"application did resignActive");

    if (![notification.object isKindOfClass:[NSApplication class]]) {
        return;
    }

    NSApplication *application = (NSApplication *)notification.object;
    if (application.isActive) {
        return;
    }
    self.state = GEAppLifeCycleStateEnd;
}
#endif

- (void)applicationWillTerminate:(NSNotification *)notification {
    GELogDebug(@"application will terminate");

    self.state = GEAppLifeCycleStateTerminate;
}

//MARK: - Setter

- (void)setState:(GEAppLifeCycleState)state {

    if (_state == state) {
        return;
    }
    
    [GEAppState shareInstance].isActive = (state == GEAppLifeCycleStateStart);

    NSMutableDictionary *userInfo = [NSMutableDictionary dictionaryWithCapacity:2];
    userInfo[kGEAppLifeCycleNewStateKey] = @(state);
    userInfo[kGEAppLifeCycleOldStateKey] = @(_state);

    [[NSNotificationCenter defaultCenter] postNotificationName:kGEAppLifeCycleStateWillChangeNotification object:self userInfo:userInfo];

    _state = state;

    [[NSNotificationCenter defaultCenter] postNotificationName:kGEAppLifeCycleStateDidChangeNotification object:self userInfo:userInfo];
}

@end
