#import "GravityExceptionHandler.h"

#include <libkern/OSAtomic.h>
#include <stdatomic.h>
#import "GELogging.h"
#import "GEPresetProperties+GEDisProperties.h"

static NSString * const TDUncaughtExceptionHandlerSignalExceptionName = @"UncaughtExceptionHandlerSignalExceptionName";
static NSString * const TDUncaughtExceptionHandlerSignalKey = @"UncaughtExceptionHandlerSignalKey";
static int TDSignals[] = {SIGILL, SIGABRT, SIGBUS, SIGSEGV, SIGFPE, SIGPIPE, SIGTRAP};
static volatile atomic_int_fast32_t TDExceptionCount = 0;
static const atomic_int_fast32_t TDExceptionMaximum = 9;

@implementation GravityExceptionHandler

+ (instancetype)sharedHandler {
    static GravityExceptionHandler *gSharedHandler = nil;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        gSharedHandler = [[GravityExceptionHandler alloc] init];
    });
    return gSharedHandler;
}

- (instancetype)init {
    self = [super init];
    if (self) {
        _gravityEngineSDKInstances = [NSHashTable weakObjectsHashTable];
        _ge_signalHandlers = calloc(NSIG, sizeof(struct sigaction));
        [self setupHandlers];
    }
    return self;
}

- (void)setupHandlers {
    _ge_lastExceptionHandler = NSGetUncaughtExceptionHandler();
    NSSetUncaughtExceptionHandler(&TDHandleException);
    
    struct sigaction action;
    sigemptyset(&action.sa_mask);
    action.sa_flags = SA_SIGINFO;
    action.sa_sigaction = &TDSignalHandler;
    for (int i = 0; i < sizeof(TDSignals) / sizeof(int); i++) {
        struct sigaction prev_action;
        int err = sigaction(TDSignals[i], &action, &prev_action);
        if (err == 0) {
            memcpy(_ge_signalHandlers + TDSignals[i], &prev_action, sizeof(prev_action));
        } else {
            GELogError(@"Error Signal: %d", TDSignals[i]);
        }
    }
}

static void TDHandleException(NSException *exception) {
    GravityExceptionHandler *handler = [GravityExceptionHandler sharedHandler];

    atomic_int_fast32_t exceptionCount = atomic_fetch_add_explicit(&TDExceptionCount, 1, memory_order_relaxed);
    if (exceptionCount <= TDExceptionMaximum) {
        [handler ge_handleUncaughtException:exception];
    }
    if (handler.ge_lastExceptionHandler) {
        handler.ge_lastExceptionHandler(exception);
    }
}

static void TDSignalHandler(int signalNumber, struct __siginfo *info, void *context) {
    GravityExceptionHandler *handler = [GravityExceptionHandler sharedHandler];
    NSMutableDictionary *crashInfo;
    NSString *reason;
    NSException *exception;
    
    atomic_int_fast32_t exceptionCount = atomic_fetch_add_explicit(&TDExceptionCount, 1, memory_order_relaxed);
    if (exceptionCount <= TDExceptionMaximum) {
        [crashInfo setObject:[NSNumber numberWithInt:signalNumber] forKey:TDUncaughtExceptionHandlerSignalKey];
        reason = [NSString stringWithFormat:@"Signal %d was raised.", signalNumber];
        exception = [NSException exceptionWithName:TDUncaughtExceptionHandlerSignalExceptionName reason:reason userInfo:crashInfo];
        [handler ge_handleUncaughtException:exception];
    }
    
    struct sigaction prev_action = handler.ge_signalHandlers[signalNumber];
    if (prev_action.sa_handler == SIG_DFL) {
        signal(signalNumber, SIG_DFL);
        raise(signalNumber);
        return;
    }
    if (prev_action.sa_flags & SA_SIGINFO) {
        if (prev_action.sa_sigaction) {
            prev_action.sa_sigaction(signalNumber, info, context);
        }
    } else if (prev_action.sa_handler) {
        prev_action.sa_handler(signalNumber);
    }
}


- (void)ge_handleUncaughtException:(NSException *)exception {
    NSDate *trackDate = [NSDate date];
    NSDictionary *dic = [self ge_getCrashInfo:exception];
    for (GravityEngineSDK *instance in self.gravityEngineSDKInstances) {
        GEAutoTrackEvent *crashEvent = [[GEAutoTrackEvent alloc] initWithName:GE_APP_CRASH_EVENT];
        crashEvent.time = trackDate;
        [instance autoTrackWithEvent:crashEvent properties:dic];
        
        if (![instance isAutoTrackEventTypeIgnored:GravityEngineEventTypeAppEnd]) {
            GEAutoTrackEvent *appEndEvent = [[GEAutoTrackEvent alloc] initWithName:GE_APP_END_EVENT];
            appEndEvent.time = trackDate;
            [instance autoTrackWithEvent:appEndEvent properties:nil];
        }
    }
    
    dispatch_sync([GravityEngineSDK ge_trackQueue], ^{});
    dispatch_sync([GravityEngineSDK ge_networkQueue], ^{});

    NSSetUncaughtExceptionHandler(NULL);
    for (int i = 0; i < sizeof(TDSignals) / sizeof(int); i++) {
        signal(TDSignals[i], SIG_DFL);
    }
}

- (NSMutableDictionary *)ge_getCrashInfo:(NSException *)exception {
    NSMutableDictionary *properties = [[NSMutableDictionary alloc] init];
    
    
    if ([GEPresetProperties disableAppCrashedReason]) {
        return properties;
    }
    
    NSString *crashStr;
    @try {
        if ([exception callStackSymbols]) {
            crashStr = [NSString stringWithFormat:@"Exception Reason:%@\nException Stack:%@", [exception reason], [exception callStackSymbols]];
        } else {
            NSString *exceptionStack = [[NSThread callStackSymbols] componentsJoinedByString:@"\n"];
            crashStr = [NSString stringWithFormat:@"%@ %@", [exception reason], exceptionStack];
        }
        crashStr = [crashStr stringByReplacingOccurrencesOfString:@"\n" withString:@"<br>"];

        NSUInteger strLength = [((NSString *)crashStr) lengthOfBytesUsingEncoding:NSUTF8StringEncoding];
        NSUInteger strMaxLength = GE_PROPERTY_CRASH_LENGTH_LIMIT;
        if (strLength > strMaxLength) {
            crashStr = [NSMutableString stringWithString:[self limitString:crashStr withLength:strMaxLength - 1]];
        }

        [properties setValue:crashStr forKey:GE_CRASH_REASON];
    } @catch(NSException *exception) {
        GELogError(@"%@ error: %@", self, exception);
    }
    return properties;
}

- (NSString *)limitString:(NSString *)originalString withLength:(NSInteger)length {
    NSStringEncoding encoding = CFStringConvertEncodingToNSStringEncoding(kCFStringEncodingUTF8);
    NSData *originalData = [originalString dataUsingEncoding:encoding];
    NSData *subData = [originalData subdataWithRange:NSMakeRange(0, length)];
    NSString *limitString = [[NSString alloc] initWithData:subData encoding:encoding];

    NSInteger index = 1;
    while (index <= 3 && !limitString) {
        if (length > index) {
            subData = [originalData subdataWithRange:NSMakeRange(0, length - index)];
            limitString = [[NSString alloc] initWithData:subData encoding:encoding];
        }
        index ++;
    }

    if (!limitString) {
        return originalString;
    }
    return limitString;
}

- (void)addGravityInstance :(GravityEngineSDK *)instance {
    NSParameterAssert(instance != nil);
    if (![self.gravityEngineSDKInstances containsObject:instance]) {
        [self.gravityEngineSDKInstances addObject:instance];
    }
}

@end
