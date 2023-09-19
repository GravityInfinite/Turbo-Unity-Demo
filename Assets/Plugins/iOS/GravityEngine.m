#if __has_include(<GravityEngineSDK/GravityEngineSDK.h>)
#import <GravityEngineSDK/GravityEngineSDK.h>
#import <GravityEngineSDK/GEDeviceInfo.h>
#else
#import "GravityEngineSDK.h"
#import "GEDeviceInfo.h"
#endif
#import <pthread.h>

#define NETWORK_TYPE_DEFAULT 1
#define NETWORK_TYPE_WIFI 2
#define NETWORK_TYPE_ALL 3

typedef const char * (*GEResultHandler) (const char *type, const char *jsonData);
static GEResultHandler geResultHandler;
void GERegisterRecieveGameCallback(GEResultHandler geHandlerPointer)
{
    geResultHandler = geHandlerPointer;
}

static NSMutableDictionary *light_instances;
static pthread_rwlock_t rwlock = PTHREAD_RWLOCK_INITIALIZER;

// 一定保证app_id不为空，且有效的字符串
GravityEngineSDK* ge_getInstance(NSString *app_id) {
    GravityEngineSDK *result = nil;
    
    if (app_id == nil || [app_id isEqualToString:@""]) {
        return [GravityEngineSDK sharedInstance];
    }

    pthread_rwlock_rdlock(&rwlock);
    if (light_instances[app_id] != nil) {
        result = light_instances[app_id];
    }
    pthread_rwlock_unlock(&rwlock);
    
    if (result != nil) return result;
    
    return [GravityEngineSDK sharedInstanceWithAppid: app_id];
}

void ge_convertToDictionary(const char *json, NSDictionary **properties_dict) {
    NSString *json_string = json != NULL ? [NSString stringWithUTF8String:json] : nil;
    if (json_string) {
        *properties_dict = [NSJSONSerialization JSONObjectWithData:[json_string dataUsingEncoding:NSUTF8StringEncoding] options:kNilOptions error:nil];
    }
}

NSDictionary * ge_parse_date(NSDictionary *properties_dict) {
    NSMutableDictionary *properties = [NSMutableDictionary dictionary];
    for (NSString *key in properties_dict.allKeys) {
        id value = properties_dict[key];
        if ([value isKindOfClass:[NSDate class]]) {
            NSDateFormatter *formatter = [[NSDateFormatter alloc] init];
            formatter.dateFormat = @"yyyy-MM-dd HH:mm:ss";
            NSString *dateStr = [formatter stringFromDate:(NSDate *)value];
            properties[key] = dateStr;
        } else if ([value isKindOfClass:[NSDictionary class]]) {
            properties[key] = ge_parse_date((NSDictionary *)value);
        } else {
            properties[key] = value;
        }
    }
    return properties;
}

char* ge_strdup(const char* string) {
    if (string == NULL)
        return NULL;
    char* res = (char*)malloc(strlen(string) + 1);
    strcpy(res, string);
    return res;
}


void ge_start(const char *app_id, const char *accessToken, int mode, const char *timezone_id, bool enable_encrypt, int encrypt_version, const char *encrypt_public_key, int pinning_mode, bool allow_invalid_certificates, bool validates_domain_name) {
    NSString *app_id_string = app_id != NULL ? [NSString stringWithUTF8String:app_id] : nil;
    NSString *access_token_string = accessToken != NULL ? [NSString stringWithUTF8String:accessToken] : nil;
    
    GEConfig *config = [[GEConfig alloc] init];
    config.appid = app_id_string;
    config.accessToken = access_token_string;
    
    [config setName:app_id_string];
    if (mode == 1) {
        // DEBUG
        config.debugMode = GravityEngineDebugOn;
    }
    NSString *timezone_id_string = timezone_id != NULL ? [NSString stringWithUTF8String:timezone_id] : nil;
    NSTimeZone *timezone = [NSTimeZone timeZoneWithName:timezone_id_string];
    if (timezone) {
        config.defaultTimeZone = timezone;
    }
    if (enable_encrypt == YES) {
        NSString *encrypt_public_key_string = encrypt_public_key != NULL ? [NSString stringWithUTF8String:encrypt_public_key] : nil;
        // Enable data encryption
        config.enableEncrypt = YES;
        // Set public key and version
        config.secretKey = [[GESecretKey alloc] initWithVersion:encrypt_version publicKey:encrypt_public_key_string];
    }

    [GravityEngineSDK startWithConfig:config];
}

void ge_enable_log(BOOL enable_log) {
    if (enable_log) {
        [GravityEngineSDK setLogLevel:GELoggingLevelDebug];
    } else {
        [GravityEngineSDK setLogLevel:GELoggingLevelNone];
    }
}

void ge_set_network_type(int type) {
    switch (type) {
        case NETWORK_TYPE_DEFAULT:
            [ge_getInstance(nil) setNetworkType:GENetworkTypeDefault];
            break;
        case NETWORK_TYPE_WIFI:
            [ge_getInstance(nil) setNetworkType:GENetworkTypeOnlyWIFI];
            break;
        case NETWORK_TYPE_ALL:
            [ge_getInstance(nil) setNetworkType:GENetworkTypeALL];
            break;
    }
}

void ge_identify(const char *app_id, const char *unique_id) {
    NSString *app_id_string = app_id != NULL ? [NSString stringWithUTF8String:app_id] : nil;
    NSString *id_string = unique_id != NULL ? [NSString stringWithUTF8String:unique_id] : nil;
    [ge_getInstance(app_id_string) identify:id_string];
}

const char *ge_get_distinct_id(const char *app_id) {
    NSString *app_id_string = app_id != NULL ? [NSString stringWithUTF8String:app_id] : nil;
    NSString *distinct_id =[ge_getInstance(app_id_string) getDistinctId];
    return ge_strdup([distinct_id UTF8String]);
}

void ge_login(const char *app_id, const char *account_id) {
    NSString *app_id_string = app_id != NULL ? [NSString stringWithUTF8String:app_id] : nil;
    NSString *id_string = account_id != NULL ? [NSString stringWithUTF8String:account_id] : nil;
    [ge_getInstance(app_id_string) login:id_string];
}

void ge_logout(const char *app_id) {
    NSString *app_id_string = app_id != NULL ? [NSString stringWithUTF8String:app_id] : nil;
    [ge_getInstance(app_id_string) logoutWithCompletion:^{
        geResultHandler("LogoutCallback", "{}");
    }];
}

void ge_config_custom_lib_info(const char *lib_name, const char *lib_version) {
    NSString *lib_name_string = lib_name != NULL ? [NSString stringWithUTF8String:lib_name] : nil;
    NSString *lib_version_string = lib_version != NULL ? [NSString stringWithUTF8String:lib_version] : nil;
    [GravityEngineSDK setCustomerLibInfoWithLibName:lib_name_string libVersion:lib_version_string];
}

void ge_track(const char *app_id, const char *event_name, const char *properties, long long time_stamp_millis, const char *timezone) {
    NSString *event_name_string = event_name != NULL ? [NSString stringWithUTF8String:event_name] : nil;
    NSString *app_id_string = app_id != NULL ? [NSString stringWithUTF8String:app_id] : nil;
    
    NSDictionary *properties_dict = nil;
    ge_convertToDictionary(properties, &properties_dict);
    
    NSString *time_zone_string = timezone != NULL ? [NSString stringWithUTF8String:timezone] : nil;
    NSTimeZone *tz;
    if ([@"Local" isEqualToString:time_zone_string]) {
        tz = [NSTimeZone localTimeZone];
    } else {
        tz = [NSTimeZone timeZoneWithName:time_zone_string];
    }
    
    NSDate *time = [NSDate dateWithTimeIntervalSince1970:time_stamp_millis / 1000.0];
    
    if (tz) {
        [ge_getInstance(app_id_string) track:event_name_string properties:properties_dict time:time timeZone:tz];
    } else {
        if (time_stamp_millis > 0) {
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Wdeprecated-declarations"
            [ge_getInstance(app_id_string) track:event_name_string properties:properties_dict time:time];
#pragma clang diagnostic pop
        } else {
            [ge_getInstance(app_id_string) track:event_name_string properties:properties_dict];
        }
    }
}

void ge_flush(const char *app_id) {
    NSString *app_id_string = app_id != NULL ? [NSString stringWithUTF8String:app_id] : nil;
    [ge_getInstance(app_id_string) flush];
}

void ge_set_super_properties(const char *app_id, const char *properties) {
    NSString *app_id_string = app_id != NULL ? [NSString stringWithUTF8String:app_id] : nil;
    NSDictionary *properties_dict = nil;
    ge_convertToDictionary(properties, &properties_dict);
    if (properties_dict) {
        [ge_getInstance(app_id_string) setSuperProperties:properties_dict];
    }
}

void ge_unset_super_property(const char *app_id, const char *property_name) {
    NSString *app_id_string = app_id != NULL ? [NSString stringWithUTF8String:app_id] : nil;
    NSString *property_name_string = property_name != NULL ? [NSString stringWithUTF8String:property_name] : nil;
    [ge_getInstance(app_id_string) unsetSuperProperty:property_name_string];
}

void ge_clear_super_properties(const char *app_id) {
    NSString *app_id_string = app_id != NULL ? [NSString stringWithUTF8String:app_id] : nil;
    [ge_getInstance(app_id_string) clearSuperProperties];
}

const char *ge_get_super_properties(const char *app_id) {
    NSString *app_id_string = app_id != NULL ? [NSString stringWithUTF8String:app_id] : nil;
    NSDictionary *property_dict = [ge_getInstance(app_id_string) currentSuperProperties];
    // nsdictionary --> nsdata
    NSData *data = [NSJSONSerialization dataWithJSONObject:property_dict options:kNilOptions error:nil];
    // nsdata -> nsstring
    NSString *jsonString = [[NSString alloc]initWithData:data encoding:NSUTF8StringEncoding];
    return ge_strdup([jsonString UTF8String]);
}

void ge_time_event(const char *app_id, const char *event_name) {
    NSString *app_id_string = app_id != NULL ? [NSString stringWithUTF8String:app_id] : nil;
    NSString *event_name_string = event_name != NULL ? [NSString stringWithUTF8String:event_name] : nil;
    [ge_getInstance(app_id_string) timeEvent:event_name_string];
}

void ge_user_set(const char *app_id, const char *properties) {
    NSString *app_id_string = app_id != NULL ? [NSString stringWithUTF8String:app_id] : nil;
    NSDictionary *properties_dict = nil;
    ge_convertToDictionary(properties, &properties_dict);
    if (properties_dict) {
        [ge_getInstance(app_id_string) user_set:properties_dict];
    }
}

void ge_user_set_with_time(const char *app_id, const char *properties, long long time_stamp_millis) {
    NSDate *time = [NSDate dateWithTimeIntervalSince1970:time_stamp_millis / 1000.0];
    NSString *app_id_string = app_id != NULL ? [NSString stringWithUTF8String:app_id] : nil;
    NSDictionary *properties_dict = nil;
    ge_convertToDictionary(properties, &properties_dict);
    if (properties_dict) {
        [ge_getInstance(app_id_string) user_set:properties_dict withTime:time];
    }
}

void ge_user_unset(const char *app_id, const char *properties) {
    NSString *app_id_string = app_id != NULL ? [NSString stringWithUTF8String:app_id] : nil;
    NSString *properties_string = properties != NULL ? [NSString stringWithUTF8String:properties] : nil;
    [ge_getInstance(app_id_string) user_unset:properties_string];
}

void ge_user_unset_with_time(const char *app_id, const char *properties, long long time_stamp_millis) {
    NSDate *time = [NSDate dateWithTimeIntervalSince1970:time_stamp_millis / 1000.0];
    NSString *app_id_string = app_id != NULL ? [NSString stringWithUTF8String:app_id] : nil;
    NSString *properties_string = properties != NULL ? [NSString stringWithUTF8String:properties] : nil;
    [ge_getInstance(app_id_string) user_unset:properties_string withTime:time];
}

void ge_user_set_once(const char *app_id, const char *properties) {
    NSString *app_id_string = app_id != NULL ? [NSString stringWithUTF8String:app_id] : nil;
    NSDictionary *properties_dict = nil;
    ge_convertToDictionary(properties, &properties_dict);
    if (properties_dict) {
        [ge_getInstance(app_id_string) user_set_once:properties_dict];
    }
}

void ge_user_set_once_with_time(const char *app_id, const char *properties, long long time_stamp_millis) {
    NSDate *time = [NSDate dateWithTimeIntervalSince1970:time_stamp_millis / 1000.0];
    NSString *app_id_string = app_id != NULL ? [NSString stringWithUTF8String:app_id] : nil;
    NSDictionary *properties_dict = nil;
    ge_convertToDictionary(properties, &properties_dict);
    if (properties_dict) {
        [ge_getInstance(app_id_string) user_set_once:properties_dict withTime:time];
    }
}

void ge_user_increment(const char *app_id, const char *properties) {
    NSString *app_id_string = app_id != NULL ? [NSString stringWithUTF8String:app_id] : nil;
    NSDictionary *properties_dict = nil;
    ge_convertToDictionary(properties, &properties_dict);
    if (properties_dict) {
        [ge_getInstance(app_id_string) user_increment:properties_dict];
    }
}

void ge_user_increment_with_time(const char *app_id, const char *properties, long long time_stamp_millis) {
    NSDate *time = [NSDate dateWithTimeIntervalSince1970:time_stamp_millis / 1000.0];
    NSString *app_id_string = app_id != NULL ? [NSString stringWithUTF8String:app_id] : nil;
    NSDictionary *properties_dict = nil;
    ge_convertToDictionary(properties, &properties_dict);
    if (properties_dict) {
        [ge_getInstance(app_id_string) user_increment:properties_dict withTime:time];
    }
}

void ge_user_number_max(const char *app_id, const char *properties) {
    NSString *app_id_string = app_id != NULL ? [NSString stringWithUTF8String:app_id] : nil;
    NSDictionary *properties_dict = nil;
    ge_convertToDictionary(properties, &properties_dict);
    if (properties_dict) {
        [ge_getInstance(app_id_string) user_number_max:properties_dict];
    }
}

void ge_user_number_max_with_time(const char *app_id, const char *properties, long long time_stamp_millis) {
    NSDate *time = [NSDate dateWithTimeIntervalSince1970:time_stamp_millis / 1000.0];
    NSString *app_id_string = app_id != NULL ? [NSString stringWithUTF8String:app_id] : nil;
    NSDictionary *properties_dict = nil;
    ge_convertToDictionary(properties, &properties_dict);
    if (properties_dict) {
        [ge_getInstance(app_id_string) user_number_max:properties_dict withTime:time];
    }
}

void ge_user_number_min(const char *app_id, const char *properties) {
    NSString *app_id_string = app_id != NULL ? [NSString stringWithUTF8String:app_id] : nil;
    NSDictionary *properties_dict = nil;
    ge_convertToDictionary(properties, &properties_dict);
    if (properties_dict) {
        [ge_getInstance(app_id_string) user_number_min:properties_dict];
    }
}

void ge_user_number_min_with_time(const char *app_id, const char *properties, long long time_stamp_millis) {
    NSDate *time = [NSDate dateWithTimeIntervalSince1970:time_stamp_millis / 1000.0];
    NSString *app_id_string = app_id != NULL ? [NSString stringWithUTF8String:app_id] : nil;
    NSDictionary *properties_dict = nil;
    ge_convertToDictionary(properties, &properties_dict);
    if (properties_dict) {
        [ge_getInstance(app_id_string) user_number_min:properties_dict withTime:time];
    }
}

void ge_user_delete(const char *app_id) {
    NSString *app_id_string = app_id != NULL ? [NSString stringWithUTF8String:app_id] : nil;
    [ge_getInstance(app_id_string) user_delete];
}

void ge_user_delete_with_time(const char *app_id, long long time_stamp_millis) {
    NSDate *time = [NSDate dateWithTimeIntervalSince1970:time_stamp_millis / 1000.0];
    NSString *app_id_string = app_id != NULL ? [NSString stringWithUTF8String:app_id] : nil;
    [ge_getInstance(app_id_string) user_delete:time];
}

void ge_user_append(const char *app_id, const char *properties) {
    NSString *app_id_string = app_id != NULL ? [NSString stringWithUTF8String:app_id] : nil;
    NSDictionary *properties_dict = nil;
    ge_convertToDictionary(properties, &properties_dict);
    if (properties_dict) {
        [ge_getInstance(app_id_string) user_append:properties_dict];
    }
}

void ge_user_append_with_time(const char *app_id, const char *properties, long long time_stamp_millis) {
    NSDate *time = [NSDate dateWithTimeIntervalSince1970:time_stamp_millis / 1000.0];
    NSString *app_id_string = app_id != NULL ? [NSString stringWithUTF8String:app_id] : nil;
    NSDictionary *properties_dict = nil;
    ge_convertToDictionary(properties, &properties_dict);
    if (properties_dict) {
        [ge_getInstance(app_id_string) user_append:properties_dict withTime:time];
    }
}

void ge_user_uniq_append(const char *app_id, const char *properties) {
    NSString *app_id_string = app_id != NULL ? [NSString stringWithUTF8String:app_id] : nil;
    NSDictionary *properties_dict = nil;
    ge_convertToDictionary(properties, &properties_dict);
    if (properties_dict) {
        [ge_getInstance(app_id_string) user_uniqAppend:properties_dict];
    }
}

void ge_user_uniq_append_with_time(const char *app_id, const char *properties, long long time_stamp_millis) {
    NSDate *time = [NSDate dateWithTimeIntervalSince1970:time_stamp_millis / 1000.0];
    NSString *app_id_string = app_id != NULL ? [NSString stringWithUTF8String:app_id] : nil;
    NSDictionary *properties_dict = nil;
    ge_convertToDictionary(properties, &properties_dict);
    if (properties_dict) {
        [ge_getInstance(app_id_string) user_uniqAppend:properties_dict withTime:time];
    }
}

const char *ge_get_device_id() {
    NSString *distinct_id = [ge_getInstance(nil) getDeviceId];
    return ge_strdup([distinct_id UTF8String]);
}

void ge_set_dynamic_super_properties(const char *app_id) {
    NSString *app_id_string = app_id != NULL ? [NSString stringWithUTF8String:app_id] : nil;
    [ge_getInstance(app_id_string) registerDynamicSuperProperties:^NSDictionary * _Nonnull{
        const char *ret = geResultHandler("DynamicSuperProperties", nil);
        NSDictionary *dynamicSuperProperties = nil;
        ge_convertToDictionary(ret, &dynamicSuperProperties);
        return dynamicSuperProperties;
    }];
}

void ge_set_track_status(const char *app_id, int status) {
    NSString *app_id_string = app_id != NULL ? [NSString stringWithUTF8String:app_id] : nil;
    GravityEngineSDK* instance = ge_getInstance(app_id_string);
    switch (status) {
        case 1:
            [instance setTrackStatus:GETrackStatusPause];
            break;
        case 2:
            [instance setTrackStatus:GETrackStatusStop];
            break;
        case 3:
            [instance setTrackStatus:GETrackStatusSaveOnly];
            break;
        case 4:
        default:
            [instance setTrackStatus:GETrackStatusNormal];
    }
}

void ge_enable_autoTrack(const char *app_id, int autoTrackEvents, const char *properties) {
    NSString *app_id_string = app_id != NULL ? [NSString stringWithUTF8String:app_id] : nil;
    NSDictionary *properties_dict = nil;
    ge_convertToDictionary(properties, &properties_dict);
    [ge_getInstance(app_id_string) enableAutoTrack: autoTrackEvents properties:properties_dict];
}

void ge_enable_autoTrack_with_callback(const char *app_id, int autoTrackEvents) {
    NSString *app_id_string = app_id != NULL ? [NSString stringWithUTF8String:app_id] : nil;
    __block NSString * w_app_id_string = app_id_string;
    [ge_getInstance(app_id_string) enableAutoTrack: autoTrackEvents callback:^NSDictionary * _Nonnull(GravityEngineAutoTrackEventType eventType, NSDictionary * _Nonnull properties) {
        NSMutableDictionary *callbackProperties = [NSMutableDictionary dictionaryWithDictionary:properties];
        [callbackProperties setObject:@(eventType) forKey:@"EventType"];
        [callbackProperties setObject:w_app_id_string forKey:@"AppID"];
        NSData *jsonData = [NSJSONSerialization dataWithJSONObject:ge_parse_date(callbackProperties) options:NSJSONWritingPrettyPrinted error:nil];
        NSString *jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
        const char *ret = geResultHandler("AutoTrackProperties", [jsonString UTF8String]);
        NSDictionary *autoTrackProperties = nil;
        ge_convertToDictionary(ret, &autoTrackProperties);
        return autoTrackProperties;
    }];
}

void ge_set_autoTrack_properties(const char *app_id, int autoTrackEvents, const char *properties) {
    NSString *app_id_string = app_id != NULL ? [NSString stringWithUTF8String:app_id] : nil;
    NSDictionary *properties_dict = nil;
    ge_convertToDictionary(properties, &properties_dict);
    [ge_getInstance(app_id_string) setAutoTrackProperties: autoTrackEvents properties:properties_dict];
}

const char *ge_get_time_string(long long time_stamp_millis) {
    NSDate *time = [NSDate dateWithTimeIntervalSince1970:time_stamp_millis / 1000.0];
    NSString *time_string = [ge_getInstance(nil) getTimeString:time];
    return ge_strdup([time_string UTF8String]);
}

void ge_calibrate_time(long long time_stamp_millis) {
    [GravityEngineSDK calibrateTime:time_stamp_millis];
}

void ge_calibrate_time_with_ntp(const char *ntp_server) {
    NSString *ntp_server_string = ntp_server != NULL ? [NSString stringWithUTF8String:ntp_server] : nil;
    [GravityEngineSDK calibrateTimeWithNtp:ntp_server_string];
}

void ge_track_native_app_ad_show_event(const char *app_id, const char *ad_union_type, const char *ad_placement_id, const char *ad_source_id, const char *ad_type, const char *adn_type, float ecpm) {
    NSString *app_id_string = app_id != NULL ? [NSString stringWithUTF8String:app_id] : nil;
    NSString *ad_union_type_string = ad_union_type != NULL ? [NSString stringWithUTF8String:ad_union_type] : nil;
    NSString *ad_placement_id_string = ad_placement_id != NULL ? [NSString stringWithUTF8String:ad_placement_id] : nil;
    NSString *ad_source_id_string = ad_source_id != NULL ? [NSString stringWithUTF8String:ad_source_id] : nil;
    NSString *ad_type_string = ad_type != NULL ? [NSString stringWithUTF8String:ad_type] : nil;
    NSString *adn_type_string = adn_type != NULL ? [NSString stringWithUTF8String:adn_type] : nil;
    [ge_getInstance(app_id_string) trackAdShowEventWithUninType:ad_union_type_string withPlacementId:ad_placement_id_string withSourceId:ad_source_id_string
                                                     withAdType:ad_type_string withAdnType:adn_type_string withEcpm:[NSNumber numberWithFloat:ecpm]];
}

void ge_track_pay_event(const char *app_id, int pay_amount, const char *pay_type, const char *order_id, const char *pay_reason, const char *pay_method) {
    NSString *app_id_string = app_id != NULL ? [NSString stringWithUTF8String:app_id] : nil;
    NSString *pay_type_string = pay_type != NULL ? [NSString stringWithUTF8String:pay_type] : nil;
    NSString *order_id_string = order_id != NULL ? [NSString stringWithUTF8String:order_id] : nil;
    NSString *pay_reason_string = pay_reason != NULL ? [NSString stringWithUTF8String:pay_reason] : nil;
    NSString *pay_method_string = pay_method != NULL ? [NSString stringWithUTF8String:pay_method] : nil;
    [ge_getInstance(app_id_string) trackPayEventWithAmount:pay_amount withPayType:pay_type_string withOrderId:order_id_string withPayReason:pay_reason_string withPayMethod:pay_method_string];
}

void ge_register(const char *app_id, const char *client_id, const char *user_client_name, bool enable_asa, int version, const char *idfa, const char *idfv, const char *caid1_md5, const char *caid2_md5) {
    NSString *app_id_string = app_id != NULL ? [NSString stringWithUTF8String:app_id] : nil;
    NSString *client_id_string = client_id != NULL ? [NSString stringWithUTF8String:client_id] : nil;
    NSString *user_client_name_string = user_client_name != NULL ? [NSString stringWithUTF8String:user_client_name] : nil;
    NSString *idfa_string = idfa != NULL ? [NSString stringWithUTF8String:idfa] : nil;
    NSString *idfv_string = idfv != NULL ? [NSString stringWithUTF8String:idfv] : nil;
    NSString *caid1_md5_string = caid1_md5 != NULL ? [NSString stringWithUTF8String:caid1_md5] : nil;
    NSString *caid2_md5_string = caid2_md5 != NULL ? [NSString stringWithUTF8String:caid2_md5] : nil;
    
    [ge_getInstance(app_id_string) registerGravityEngineWithClientId:client_id_string withUserName:user_client_name_string withVersion:version withAsaEnable:enable_asa withIdfa:idfa_string withIdfv:idfv_string withCaid1:caid1_md5_string withCaid2:caid2_md5_string withSyncAttribution:NO withSuccessCallback:^(NSDictionary * _Nonnull response){
        NSLog(@"gravity engine register success");
        geResultHandler("RegisterCallbackSuccess", "{}");
    } withErrorCallback:^(NSError * _Nonnull error) {
        NSLog(@"gravity engine register failed, and error is %@", error);
        geResultHandler("RegisterCallbackFailed", "{}");
    }];

}

void ge_resetClientId(const char *app_id, const char *new_client_id) {
    NSString *app_id_string = app_id != NULL ? [NSString stringWithUTF8String:app_id] : nil;
    NSString *new_client_id_string = new_client_id != NULL ? [NSString stringWithUTF8String:new_client_id] : nil;
    
    [ge_getInstance(app_id_string) resetClientID:new_client_id_string withSuccessCallback:^(NSDictionary * _Nonnull response){
        NSLog(@"gravity engine reset client id success");
        geResultHandler("ResetClientIdCallbackSuccess", "{}");
    } withErrorCallback:^(NSError * _Nonnull error) {
        NSLog(@"gravity engine reset client id failed, and error is %@", error);
        geResultHandler("ResetClientIdCallbackFailed", "{}");
    }];

}

void ge_bind_ta_third_platform(const char *app_id, const char *ta_account_id,  const char *ta_distinct_id) {
    NSString *app_id_string = app_id != NULL ? [NSString stringWithUTF8String:app_id] : nil;
    NSString *ta_account_id_string = ta_account_id != NULL ? [NSString stringWithUTF8String:ta_account_id] : nil;
    NSString *ta_distinct_id_string = ta_distinct_id != NULL ? [NSString stringWithUTF8String:ta_distinct_id] : nil;
    
    [ge_getInstance(app_id_string) bindTAThirdPlatformWithAccountId:ta_account_id_string withDistinctId:ta_distinct_id_string];
}

