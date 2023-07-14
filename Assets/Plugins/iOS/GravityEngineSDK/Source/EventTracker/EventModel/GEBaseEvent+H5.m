//
//  GEBaseEvent+H5.m
//  GravityEngineSDK
//
//

#import "GEBaseEvent+H5.h"
#import <objc/runtime.h>

static char GE_EVENT_H5_TIME_STRING;
static char GE_EVENT_H5_ZONE_OFF_SET;

@implementation GEBaseEvent (H5)

- (NSString *)h5TimeString {
    return objc_getAssociatedObject(self, &GE_EVENT_H5_TIME_STRING);
}

- (void)setH5TimeString:(NSString *)h5TimeString {
    objc_setAssociatedObject(self, &GE_EVENT_H5_TIME_STRING, h5TimeString, OBJC_ASSOCIATION_COPY_NONATOMIC);
}

- (NSNumber *)h5ZoneOffSet {
    return objc_getAssociatedObject(self, &GE_EVENT_H5_ZONE_OFF_SET);
}

- (void)setH5ZoneOffSet:(NSNumber *)h5ZoneOffSet {
    objc_setAssociatedObject(self, &GE_EVENT_H5_ZONE_OFF_SET, h5ZoneOffSet, OBJC_ASSOCIATION_RETAIN_NONATOMIC);
}

@end
