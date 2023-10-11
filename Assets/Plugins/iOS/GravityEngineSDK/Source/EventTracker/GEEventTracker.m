//
//  GEEventTracker.m
//  GravityEngineSDK
//
//

#import "GEEventTracker.h"
#import "GENetwork.h"
#import "GEReachability.h"
#import "GEEventRecord.h"
#import "NSString+GEString.h"

static dispatch_queue_t ge_networkQueue;

@interface GEEventTracker ()
@property (atomic, strong) GENetwork *network;
@property (atomic, strong) GEConfig *config;
@property (atomic, strong) dispatch_queue_t queue;
@property (nonatomic, strong) GESqliteDataQueue *dataQueue;

@end

@implementation GEEventTracker

+ (void)initialize {
    static dispatch_once_t GravityOnceToken;
    dispatch_once(&GravityOnceToken, ^{
        NSString *queuelabel = [NSString stringWithFormat:@"com.gravityinfinite.%p", (void *)self];
        NSString *networkLabel = [queuelabel stringByAppendingString:@".network"];
        ge_networkQueue = dispatch_queue_create([networkLabel UTF8String], DISPATCH_QUEUE_SERIAL);
    });
}

+ (dispatch_queue_t)ge_networkQueue {
    return ge_networkQueue;
}

- (instancetype)initWithQueue:(dispatch_queue_t)queue instanceToken:(nonnull NSString *)instanceToken {
    if (self = [self init]) {
        self.queue = queue;
        self.config = [GravityEngineSDK sharedInstanceWithAppid:instanceToken].config;
        self.network = [self generateNetworkWithConfig:self.config];
        self.dataQueue = [GESqliteDataQueue sharedInstanceWithAppid:[self.config getMapInstanceToken]];
    }
    return self;
}

- (GENetwork *)generateNetworkWithConfig:(GEConfig *)config {
    GENetwork *network = [[GENetwork alloc] init];
    network.debugMode = config.debugMode;
    network.appid = config.appid;
    network.sessionDidReceiveAuthenticationChallenge = config.securityPolicy.sessionDidReceiveAuthenticationChallenge;
    network.serverURL = [NSURL URLWithString:[NSString stringWithFormat:@"%@/event_center/api/v1/event/collect/?access_token=%@", _config.configureURL, _config.accessToken]];
    network.securityPolicy = config.securityPolicy;
    return network;
}

//MARK: - Public

- (void)track:(NSDictionary *)event immediately:(BOOL)immediately saveOnly:(BOOL)isSaveOnly {
    NSInteger count = 0;
    if (immediately) {
        
        if (isSaveOnly) {
            return;
        }
        GELogDebug(@"queueing data flush immediately:%@", event);
        dispatch_async(self.queue, ^{
            dispatch_async(ge_networkQueue, ^{
                [self flushImmediately:event];
            });
        });
    } else {
        GELogDebug(@"queueing data:%@", event);
        count = [self saveEventsData:event];
    }
    if (count >= [self.config.uploadSize integerValue]) {
        
        if (isSaveOnly) {
            return;
        }
        GELogDebug(@"flush action, count: %ld, uploadSize: %d",count, [self.config.uploadSize integerValue]);
        [self flush];
    }
}

- (void)flushImmediately:(NSDictionary *)event {
    [self.network flushEvents:@[event]];
}

- (NSInteger)saveEventsData:(NSDictionary *)data {
    NSMutableDictionary *event = [[NSMutableDictionary alloc] initWithDictionary:data];
    NSInteger count = 0;
    @synchronized (GESqliteDataQueue.class) {
        
        if (_config.enableEncrypt) {
#if TARGET_OS_IOS
            NSDictionary *encryptData = [[GravityEngineSDK sharedInstanceWithAppid:self.config.appid].encryptManager encryptJSONObject:event];
            if (encryptData == nil) {
                encryptData = event;
            }
            count = [self.dataQueue addObject:encryptData withAppid:[self.config getMapInstanceToken]];
#elif TARGET_OS_OSX
            count = [self.dataQueue addObject:event withAppid:[self.config getMapInstanceToken]];
#endif
        } else {
            count = [self.dataQueue addObject:event withAppid:[self.config getMapInstanceToken]];
        }
    }
    return count;
}

- (void)flush {
    [self _asyncWithCompletion:^{}];
}

/// Synchronize data asynchronously (synchronize data in the local database to TA)
/// Need to add this event to the serialQueue queue
/// In some scenarios, event warehousing and sending network requests happen at the same time. Event storage is performed in serialQueue, and data reporting is performed in networkQueue. To ensure that events are stored first, you need to add the reported data operation to serialQueue
- (void)_asyncWithCompletion:(void(^)(void))completion {
    
    void(^block)(void) = ^{
        dispatch_async(ge_networkQueue, ^{
            [self _syncWithSize:kBatchSize completion:completion];
        });
    };
    if (dispatch_queue_get_label(DISPATCH_CURRENT_QUEUE_LABEL) == dispatch_queue_get_label(self.queue)) {
        block();
    } else {
        dispatch_async(self.queue, block);
    }    
}

/// Synchronize data (synchronize the data in the local database to GE)
/// @param size The maximum number of items obtained from the database each time, the default is 50
/// @param completion synchronous callback
/// This method needs to be performed in networkQueue, and will continue to send network requests until the data in the database is sent
- (void)_syncWithSize:(NSUInteger)size completion:(void(^)(void))completion {
    
    
    NSString *networkType = [[GEReachability shareInstance] networkState];
    if (!([GEReachability convertNetworkType:networkType] & self.config.networkTypePolicy)) {
        if (completion) {
            completion();
        }
        return;
    }
    
    NSArray<NSDictionary *> *recordArray;
    NSArray *recodIds;
    NSArray *uuids;
    @synchronized (GESqliteDataQueue.class) {
        
        NSArray<GEEventRecord *> *records = [self.dataQueue getFirstRecords:kBatchSize withAppid:[self.config getMapInstanceToken]];
        NSArray<GEEventRecord *> *encryptRecords = [self encryptEventRecords:records];
        NSMutableArray *indexs = [[NSMutableArray alloc] initWithCapacity:encryptRecords.count];
        NSMutableArray *recordContents = [[NSMutableArray alloc] initWithCapacity:encryptRecords.count];
        for (GEEventRecord *record in encryptRecords) {
            [indexs addObject:record.index];
            [recordContents addObject:record.event];
        }
        recodIds = indexs;
        recordArray = recordContents;
        
        
        uuids = [self.dataQueue upadteRecordIds:recodIds];
    }
     
    
    if (recordArray.count == 0 || uuids.count == 0) {
        if (completion) {
            completion();
        }
        return;
    }
    
    
    
    BOOL flushSucc = YES;
    while (recordArray.count > 0 && uuids.count > 0 && flushSucc) {
        flushSucc = [self.network flushEvents:recordArray];
        if (flushSucc) {
            @synchronized (GESqliteDataQueue.class) {
                BOOL ret = [self.dataQueue removeDataWithuids:uuids];
                if (!ret) {
                    break;
                }
                
                NSArray<GEEventRecord *> *records = [self.dataQueue getFirstRecords:kBatchSize withAppid:[self.config getMapInstanceToken]];
                NSArray<GEEventRecord *> *encryptRecords = [self encryptEventRecords:records];
                NSMutableArray *indexs = [[NSMutableArray alloc] initWithCapacity:encryptRecords.count];
                NSMutableArray *recordContents = [[NSMutableArray alloc] initWithCapacity:encryptRecords.count];
                for (GEEventRecord *record in encryptRecords) {
                    [indexs addObject:record.index];
                    [recordContents addObject:record.event];
                }
                recodIds = indexs;
                recordArray = recordContents;
                
                
                uuids = [self.dataQueue upadteRecordIds:recodIds];
            }
        } else {
            break;
        }
    }
    if (completion) {
        completion();
    }
}

- (NSArray<GEEventRecord *> *)encryptEventRecords:(NSArray<GEEventRecord *> *)records {
#if TARGET_OS_IOS
    NSMutableArray *encryptRecords = [NSMutableArray arrayWithCapacity:records.count];
    
    GEEncryptManager *encryptManager = [GravityEngineSDK sharedInstanceWithAppid:[self.config getMapInstanceToken]].encryptManager;
    
    if (self.config.enableEncrypt && encryptManager.isValid) {
        for (GEEventRecord *record in records) {
            
            if (record.encrypted) {
                
                [encryptRecords addObject:record];
            } else {
                
                NSDictionary *obj = [encryptManager encryptJSONObject:record.event];
                if (obj) {
                    [record setSecretObject:obj];
                    [encryptRecords addObject:record];
                } else {
                    [encryptRecords addObject:record];
                }
            }
        }
        return encryptRecords.count == 0 ? records : encryptRecords;
    } else {
        return records;
    }
#elif TARGET_OS_OSX
    return records;
#endif
}

- (void)syncSendAllData {
    dispatch_sync(ge_networkQueue, ^{});
}


//MARK: - Setter & Getter


@end
