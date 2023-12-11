#import "GENetwork.h"

#import "NSData+GEGzip.h"
#import "GEJSONUtil.h"
#import "GELogging.h"
#import "GESecurityPolicy.h"
#import "GEAppState.h"
#import "GEFile.h"

#if TARGET_OS_IOS
#import "GEToastView.h"
#endif

static NSString *kTAIntegrationType = @"TA-Integration-Type";
static NSString *kTAIntegrationVersion = @"TA-Integration-Version";
static NSString *kTAIntegrationCount = @"TA-Integration-Count";
static NSString *kTAIntegrationExtra = @"TA-Integration-Extra";
static NSString *kTADatasType = @"TA-Datas-Type";

@interface GENetwork ()

@property (nonatomic, strong) GEFile *file;

@end

@implementation GENetwork

- (void)initGEFile {
    self.file = [[GEFile alloc] initWithAppid:self.appid];
}

- (NSURLSession *)sharedURLSession {
    static NSURLSession *sharedSession = nil;
    @synchronized(self) {
        if (sharedSession == nil) {
            NSURLSessionConfiguration *sessionConfig = [NSURLSessionConfiguration defaultSessionConfiguration];
            sharedSession = [NSURLSession sessionWithConfiguration:sessionConfig delegate:self delegateQueue:nil];
        }
    }
    return sharedSession;
}

- (NSString *)URLEncode:(NSString *)string {
    
    NSString *encodedString = (NSString *)CFBridgingRelease(CFURLCreateStringByAddingPercentEscapes(NULL,
                                                                                                    (CFStringRef)string,
                                                                                                    NULL,
                                                                                                    (CFStringRef)@"!*'();:@&=+$,/?%#[]",
                                                                                                    kCFStringEncodingUTF8 ));

    return encodedString;
}

- (BOOL)flushEvents:(NSArray<NSDictionary *> *)recordArray {
    if (!self.file) {
        [self initGEFile];
    }
    
    NSString * clientId = [self.file unarchiveClientId];
    if (!clientId) {
        GELogError(@"client id is nil, will not post events.");
        return NO;
    }
    
    __block BOOL flushSucc = YES;
    UInt64 time = [[NSDate date] timeIntervalSince1970] * 1000;
    NSDictionary *flushDic = @{
        @"event_list": recordArray,
        @"$app_id": self.appid,
        @"client_id": clientId,
        @"$flush_time": @(time),
    };
    
//    __block BOOL isEncrypt;
//    [recordArray enumerateObjectsUsingBlock:^(NSDictionary * _Nonnull obj, NSUInteger idx, BOOL * _Nonnull stop) {
//        if ([obj.allKeys containsObject:@"ekey"]) {
//            isEncrypt = YES;
//            *stop = YES;
//        }
//    }];
    
    NSString *jsonString = [GEJSONUtil JSONStringForObject:flushDic];
    GELogDebug(@"url is %@", self.serverURL);
    GELogDebug(@"flush json string is ----> %@", jsonString);
    NSMutableURLRequest *request = [self buildRequestWithJSONString:jsonString];
    // 添加一些请求头
    [request addValue:[GEDeviceInfo sharedManager].libName forHTTPHeaderField:kTAIntegrationType];
    [request addValue:[GEDeviceInfo sharedManager].libVersion forHTTPHeaderField:kTAIntegrationVersion];
    [request addValue:@(recordArray.count).stringValue forHTTPHeaderField:kTAIntegrationCount];
    [request addValue:@"iOS" forHTTPHeaderField:kTAIntegrationExtra];
    if (self.debugMode == GravityEngineDebugOn) {
        [request addValue:@"1" forHTTPHeaderField:@"Turbo-Debug-Mode"];
    }

    NSString *currentUserAgent = [self.file unarchiveUserAgent];
    if (currentUserAgent) {
        [request addValue:currentUserAgent forHTTPHeaderField:@"User-Agent"];
    }

//    if (isEncrypt) {
//        [request addValue:@"1" forHTTPHeaderField:kTADatasType];
//    }
//    [request addValue:@"Keep-Alive" forHTTPHeaderField:@"Connection"];
//    [request addValue:@"timeout=15,max=100" forHTTPHeaderField:@"Keep-Alive"];
    
    dispatch_semaphore_t flushSem = dispatch_semaphore_create(0);

    void (^block)(NSData *, NSURLResponse *, NSError *) = ^(NSData *data, NSURLResponse *response, NSError *error) {
        if (error || ![response isKindOfClass:[NSHTTPURLResponse class]]) {
            flushSucc = NO;
            GELogError(@"Networking error:%@", error);
            dispatch_semaphore_signal(flushSem);
            return;
        }

        NSHTTPURLResponse *urlResponse = (NSHTTPURLResponse *)response;
        if ([urlResponse statusCode] == 200) {
            if (!data) {
                flushSucc = NO;
                GELogDebug(@"response data is empty");
                return;
            }
            NSDictionary *result = [NSJSONSerialization JSONObjectWithData:data options:kNilOptions error:nil];
            GELogDebug(@"flush response data is ---->%@", result);
            int gravityCode = [[result objectForKey:@"code"] intValue];
            if (gravityCode == 0) {
                flushSucc = YES;
                GELogDebug(@"flush success");
            } else if (gravityCode == 2000){
                flushSucc = NO;
                GELogError(@"please call initializeGravityEngine method first");
            } else {
                flushSucc = NO;
                GELogError(@"flush error other reason %d", gravityCode);
            }
        } else {
            flushSucc = NO;
            NSString *urlResponse = [[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding];
            GELogError(@"%@", [NSString stringWithFormat:@"%@ network failed with response '%@'.", self, urlResponse]);
        }

        dispatch_semaphore_signal(flushSem);
    };

    NSURLSessionDataTask *task = [[self sharedURLSession] dataTaskWithRequest:request completionHandler:block];
    [task resume];
    dispatch_semaphore_wait(flushSem, DISPATCH_TIME_FOREVER);
    return flushSucc;
}

- (NSMutableURLRequest *)buildRequestWithJSONString:(NSString *)jsonString {
    
//    NSData *zippedData = [NSData ge_gzipData:[jsonString dataUsingEncoding:NSUTF8StringEncoding]];
//    NSString *postBody = [zippedData base64EncodedStringWithOptions:0];
    NSMutableURLRequest *request = [NSMutableURLRequest requestWithURL:self.serverURL];
    [request setHTTPMethod:@"POST"];
    [request setHTTPBody:[jsonString dataUsingEncoding:NSUTF8StringEncoding]];
    NSString *contentType = [NSString stringWithFormat:@"application/json"];
    [request addValue:contentType forHTTPHeaderField:@"Content-Type"];
    [request setTimeoutInterval:60.0];
    return request;
}

- (void)fetchRemoteConfig:(NSString *)appid handler:(TDFlushConfigBlock)handler {
    void (^block)(NSData *, NSURLResponse *, NSError *) = ^(NSData *data, NSURLResponse *response, NSError *error) {
        if (error || ![response isKindOfClass:[NSHTTPURLResponse class]]) {
            GELogError(@"Fetch remote config network failed:%@", error);
            return;
        }
        NSError *err;
        if (!data) {
            return;
        }
        NSDictionary *ret = [NSJSONSerialization JSONObjectWithData:data options:kNilOptions error:&err];
        if (err) {
            GELogError(@"Fetch remote config json error:%@", err);
        } else if ([ret isKindOfClass:[NSDictionary class]] && [ret[@"code"] isEqualToNumber:[NSNumber numberWithInt:0]]) {
            GELogDebug(@"Fetch remote config for %@ : %@", appid, [ret objectForKey:@"data"]);
            handler([ret objectForKey:@"data"], error);
        } else {
            GELogError(@"Fetch remote config failed");
        }
    };
    NSString *urlStr = [NSString stringWithFormat:@"%@?appid=%@", self.serverURL, appid];
    NSMutableURLRequest *request = [NSMutableURLRequest requestWithURL:[NSURL URLWithString:urlStr]];
    [request setHTTPMethod:@"Get"];
    NSURLSessionDataTask *task = [[self sharedURLSession] dataTaskWithRequest:request completionHandler:block];
    [task resume];
}

- (void)postDataWith:(NSDictionary *)postBodyDic handler:(GEPostDataBlock)handler {
    if (!self.file) {
        [self initGEFile];
    }
    void (^block)(NSData *, NSURLResponse *, NSError *) = ^(NSData *data, NSURLResponse *response, NSError *error) {
        if (error || ![response isKindOfClass:[NSHTTPURLResponse class]]) {
            GELogError(@"post network failed:%@", error);
            handler(@{}, error);
            return;
        }
        NSError *err;
        if (!data) {
            handler(@{}, [NSError errorWithDomain:@"com.gravityengine" code:-1 userInfo:@{
                @"error":@"data is empty"
            }]);
            return;
        }
        
        NSDictionary *ret = [NSJSONSerialization JSONObjectWithData:data options:kNilOptions error:&err];
        if (err) {
            handler(ret, err);
        } else if ([ret isKindOfClass:[NSDictionary class]] && [ret[@"code"] isEqualToNumber:[NSNumber numberWithInt:0]]) {
            handler(ret, nil);
        } else {
            handler(ret, [NSError errorWithDomain:@"com.gravityengine" code:-1 userInfo:@{
                @"error":[NSString stringWithFormat:@"post failed with code not equal to zero, ret is ---> %@", ret]
            }]);
        }
    };
    
    NSMutableURLRequest *request = [NSMutableURLRequest requestWithURL:self.serverURL];
    [request setHTTPMethod:@"POST"];
    NSString *jsonString = [GEJSONUtil JSONStringForObject:postBodyDic];
    GELogDebug(@"post json string is %@ %@", jsonString, self.serverURL);
    [request setHTTPBody:[jsonString dataUsingEncoding:NSUTF8StringEncoding]];
    [request addValue:[NSString stringWithFormat:@"application/json"] forHTTPHeaderField:@"Content-Type"];
    NSString *currentUserAgent = [self.file unarchiveUserAgent];
    if (currentUserAgent) {
        GELogDebug(@"use user agent %@", currentUserAgent);
        [request addValue:currentUserAgent forHTTPHeaderField:@"User-Agent"];
    }
    [request setTimeoutInterval:60.0];
    
    NSURLSessionDataTask *task = [[self sharedURLSession] dataTaskWithRequest:request completionHandler:block];
    [task resume];
}

#pragma mark - NSURLSessionDelegate
- (void)URLSession:(NSURLSession *)session didReceiveChallenge:(NSURLAuthenticationChallenge *)challenge completionHandler:(void (^)(NSURLSessionAuthChallengeDisposition, NSURLCredential * _Nullable))completionHandler {
    NSURLSessionAuthChallengeDisposition disposition = NSURLSessionAuthChallengePerformDefaultHandling;
    NSURLCredential *credential = nil;

    if (self.sessionDidReceiveAuthenticationChallenge) {
        disposition = self.sessionDidReceiveAuthenticationChallenge(session, challenge, &credential);
    } else {
        if ([challenge.protectionSpace.authenticationMethod isEqualToString:NSURLAuthenticationMethodServerTrust]) {
            if ([self.securityPolicy evaluateServerTrust:challenge.protectionSpace.serverTrust forDomain:challenge.protectionSpace.host]) {
                credential = [NSURLCredential credentialForTrust:challenge.protectionSpace.serverTrust];
                if (credential) {
                    disposition = NSURLSessionAuthChallengeUseCredential;
                } else {
                    disposition = NSURLSessionAuthChallengePerformDefaultHandling;
                }
            } else {
                disposition = NSURLSessionAuthChallengeCancelAuthenticationChallenge;
            }
        } else {
            disposition = NSURLSessionAuthChallengePerformDefaultHandling;
        }
    }

    if (completionHandler) {
        completionHandler(disposition, credential);
    }
}

@end
