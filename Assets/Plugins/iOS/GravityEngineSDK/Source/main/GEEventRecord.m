//
//  GEEventRecord.m
//  GravityEngineSDK
//
//

#import "GEEventRecord.h"

#import "GEJSONUtil.h"

static NSString * const GEEncryptRecordKeyEKey = @"ekey";
static NSString * const GEEncryptRecordKeyPayload = @"payload";

@implementation GEEventRecord {
    NSMutableDictionary *_event;
}


- (instancetype)initWithEvent:(NSDictionary *)event type:(NSString *)type {
    if (self = [super init]) {
        
        _event = [event mutableCopy];
        _encrypted = _event[GEEncryptRecordKeyEKey] != nil;
    }
    return self;
}

- (instancetype)initWithUUID:(NSString *)uuid content:(NSDictionary *)content {
    if (self = [super init]) {
        _uuid = uuid;

        if (content && [content isKindOfClass:[NSDictionary class]]) {
            _event = [NSMutableDictionary dictionaryWithDictionary:content];
            _encrypted = _event[GEEncryptRecordKeyEKey] != nil;
        }
    }
    return self;
}

- (instancetype)initWithIndex:(NSNumber *)index content:(NSDictionary *)content {
    if (self = [super init]) {
        _index = index;

        if (content && [content isKindOfClass:[NSDictionary class]]) {
            _event = [NSMutableDictionary dictionaryWithDictionary:content];
            _encrypted = _event[GEEncryptRecordKeyEKey] != nil;
        }
    }
    return self;
}

- (instancetype)initWithContent:(NSDictionary *)content {
    if (self = [super init]) {
        if (content && [content isKindOfClass:[NSDictionary class]]) {
            _event = [NSMutableDictionary dictionaryWithDictionary:content];
            _encrypted = _event[GEEncryptRecordKeyEKey] != nil;
        }
    }
    return self;
}

- (NSString *)ekey {
    return _event[GEEncryptRecordKeyEKey];
}

- (void)setSecretObject:(NSDictionary *)obj {
    if (!obj || ![obj isKindOfClass:[NSDictionary class]]) {
        return;
    }
    [_event removeAllObjects];
    [_event addEntriesFromDictionary:obj];

    _encrypted = YES;
}

- (BOOL)isValid {
    return self.event.count > 0;
}

- (NSString *)content {
    return [GEJSONUtil JSONStringForObject:self.event];
}

- (NSString *)flushContent:(NSString *)appid {
    if (![self isValid]) {
        return nil;
    }

    UInt64 time = [[NSDate date] timeIntervalSince1970] * 1000;
    _event[@"$flush_time"] = @(time);
    _event[@"$app_id"] =appid;
    
    return self.content;
}

@end
