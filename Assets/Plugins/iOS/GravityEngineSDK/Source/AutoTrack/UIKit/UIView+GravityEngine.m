//
//  UIView+GravityEngine.m
//  GravityEngineSDK
//
//

#import "UIView+GravityEngine.h"
#import <objc/runtime.h>

static char GE_AUTOTRACK_VIEW_ID;
static char GE_AUTOTRACK_VIEW_ID_APPID;
static char GE_AUTOTRACK_VIEW_IGNORE;
static char GE_AUTOTRACK_VIEW_IGNORE_APPID;
static char GE_AUTOTRACK_VIEW_PROPERTIES;
static char GE_AUTOTRACK_VIEW_PROPERTIES_APPID;
static char GE_AUTOTRACK_VIEW_DELEGATE;

@implementation UIView (GravityEngine)

- (NSString *)gravityEngineViewID {
    return objc_getAssociatedObject(self, &GE_AUTOTRACK_VIEW_ID);
}

- (void)setGravityEngineViewID:(NSString *)gravityEngineViewID {
    objc_setAssociatedObject(self, &GE_AUTOTRACK_VIEW_ID, gravityEngineViewID, OBJC_ASSOCIATION_COPY_NONATOMIC);
}

- (BOOL)gravityEngineIgnoreView {
    return [objc_getAssociatedObject(self, &GE_AUTOTRACK_VIEW_IGNORE) boolValue];
}

- (void)setGravityEngineIgnoreView:(BOOL)gravityEngineIgnoreView {
    objc_setAssociatedObject(self, &GE_AUTOTRACK_VIEW_IGNORE, [NSNumber numberWithBool:gravityEngineIgnoreView], OBJC_ASSOCIATION_ASSIGN);
}

- (NSDictionary *)gravityEngineIgnoreViewWithAppid {
    return objc_getAssociatedObject(self, &GE_AUTOTRACK_VIEW_IGNORE_APPID);
}

- (void)setGravityEngineIgnoreViewWithAppid:(NSDictionary *)gravityEngineViewProperties {
    objc_setAssociatedObject(self, &GE_AUTOTRACK_VIEW_IGNORE_APPID, gravityEngineViewProperties, OBJC_ASSOCIATION_RETAIN_NONATOMIC);
}

- (NSDictionary *)gravityEngineViewIDWithAppid {
    return objc_getAssociatedObject(self, &GE_AUTOTRACK_VIEW_ID_APPID);
}

- (void)setGravityEngineViewIDWithAppid:(NSDictionary *)gravityEngineViewProperties {
    objc_setAssociatedObject(self, &GE_AUTOTRACK_VIEW_ID_APPID, gravityEngineViewProperties, OBJC_ASSOCIATION_RETAIN_NONATOMIC);
}

- (NSDictionary *)gravityEngineViewProperties {
    return objc_getAssociatedObject(self, &GE_AUTOTRACK_VIEW_PROPERTIES);
}

- (void)setGravityEngineViewProperties:(NSDictionary *)gravityEngineViewProperties {
    objc_setAssociatedObject(self, &GE_AUTOTRACK_VIEW_PROPERTIES, gravityEngineViewProperties, OBJC_ASSOCIATION_RETAIN_NONATOMIC);
}

- (NSDictionary *)gravityEngineViewPropertiesWithAppid {
    return objc_getAssociatedObject(self, &GE_AUTOTRACK_VIEW_PROPERTIES_APPID);
}

- (void)setGravityEngineViewPropertiesWithAppid:(NSDictionary *)gravityEngineViewProperties {
    objc_setAssociatedObject(self, &GE_AUTOTRACK_VIEW_PROPERTIES_APPID, gravityEngineViewProperties, OBJC_ASSOCIATION_RETAIN_NONATOMIC);
}

- (id)gravityEngineDelegate {
    return objc_getAssociatedObject(self, &GE_AUTOTRACK_VIEW_DELEGATE);
}

- (void)setGravityEngineDelegate:(id)gravityEngineDelegate {
    objc_setAssociatedObject(self, &GE_AUTOTRACK_VIEW_DELEGATE, gravityEngineDelegate, OBJC_ASSOCIATION_ASSIGN);
}

@end
