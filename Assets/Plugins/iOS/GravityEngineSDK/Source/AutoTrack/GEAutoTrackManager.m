#import "GEAutoTrackManager.h"

#import "GESwizzler.h"
#import "UIViewController+AutoTrack.h"
#import "NSObject+GESwizzle.h"
#import "GEJSONUtil.h"
#import "UIApplication+AutoTrack.h"
#import "GravityEngineSDKPrivate.h"
#import "GEPublicConfig.h"
#import "GEAutoClickEvent.h"
#import "GEAutoPageViewEvent.h"
#import "GEAppLifeCycle.h"
#import "GEAppState.h"
#import "GERunTime.h"
#import "GEPresetProperties+GEDisProperties.h"

#import "GEAppStartEvent.h"
#import "GEAppEndEvent.h"
#import "GEAppEndTracker.h"
#import "GEColdStartTracker.h"
#import "GEInstallTracker.h"
#import "GEAppState.h"

#ifndef GE_LOCK
#define GE_LOCK(lock) dispatch_semaphore_wait(lock, DISPATCH_TIME_FOREVER);
#endif

#ifndef GE_UNLOCK
#define GE_UNLOCK(lock) dispatch_semaphore_signal(lock);
#endif

NSString * const GE_EVENT_PROPERTY_TITLE = @"$title";
NSString * const GE_EVENT_PROPERTY_URL_PROPERTY = @"$url";
NSString * const GE_EVENT_PROPERTY_REFERRER_URL = @"$referrer";
NSString * const GE_EVENT_PROPERTY_SCREEN_NAME = @"$screen_name";
NSString * const GE_EVENT_PROPERTY_ELEMENT_ID = @"$element_id";
NSString * const GE_EVENT_PROPERTY_ELEMENT_TYPE = @"$element_type";
NSString * const GE_EVENT_PROPERTY_ELEMENT_CONTENT = @"$element_content";
NSString * const GE_EVENT_PROPERTY_ELEMENT_POSITION = @"$element_position";

@interface GEAutoTrackManager ()
@property (atomic, strong) NSMutableDictionary<NSString *, id> *autoTrackOptions;
@property (nonatomic, strong, nonnull) dispatch_semaphore_t trackOptionLock;
@property (atomic, copy) NSString *referrerViewControllerUrl;
@property (nonatomic, strong) GEHotStartTracker *appHotStartTracker;
@property (nonatomic, strong) GEAppEndTracker *appEndTracker;
@property (nonatomic, strong) GEColdStartTracker *appColdStartTracker;
@property (nonatomic, strong) GEInstallTracker *appInstallTracker;

@end


@implementation GEAutoTrackManager

#pragma mark - Public

+ (instancetype)sharedManager {
    static dispatch_once_t once;
    static GEAutoTrackManager *manager = nil;
    dispatch_once(&once, ^{
        manager = [[[GEAutoTrackManager class] alloc] init];
        manager.autoTrackOptions = [NSMutableDictionary new];
        manager.trackOptionLock = dispatch_semaphore_create(1);
        [manager registerAppLifeCycleListener];
    });
    return manager;
}

- (void)trackEventView:(UIView *)view {
    [self trackEventView:view withIndexPath:nil];
}

- (void)trackEventView:(UIView *)view withIndexPath:(NSIndexPath *)indexPath {
    if (view.gravityEngineIgnoreView) {
        return;
    }
    
    NSString *elementId = nil;
    NSString *elementType = nil;
    NSString *elementContent = nil;
    NSString *elementPosition = nil;
    NSString *elementPageTitle = nil;
    NSString *elementScreenName = nil;
    NSMutableDictionary *yx_customProperties = [NSMutableDictionary dictionary];

    
    elementId = view.gravityEngineViewID;
    elementType = NSStringFromClass([view class]);
    
    
    NSMutableDictionary *properties = [[NSMutableDictionary alloc] init];
    properties[GE_EVENT_PROPERTY_ELEMENT_ID] = view.gravityEngineViewID;
    properties[GE_EVENT_PROPERTY_ELEMENT_TYPE] = NSStringFromClass([view class]);
    UIViewController *viewController = [self viewControllerForView:view];
    if (viewController != nil) {
        NSString *screenName = NSStringFromClass([viewController class]);
        properties[GE_EVENT_PROPERTY_SCREEN_NAME] = screenName;
        
        elementScreenName = screenName;

        NSString *controllerTitle = [self titleFromViewController:viewController];
        if (controllerTitle) {
            properties[GE_EVENT_PROPERTY_TITLE] = controllerTitle;
            
            elementPageTitle = controllerTitle;
        }
    }
    
    
    NSDictionary *propDict = view.gravityEngineViewProperties;
    if ([propDict isKindOfClass:[NSDictionary class]]) {
        [properties addEntriesFromDictionary:propDict];

        [yx_customProperties addEntriesFromDictionary:propDict];
    }
    
    UIView *contentView;
    NSDictionary *propertyWithAppid;
    if (indexPath) {
        if ([view isKindOfClass:[UITableView class]]) {
            UITableView *tableView = (UITableView *)view;
            contentView = [tableView cellForRowAtIndexPath:indexPath];
            if (!contentView) {
                [tableView layoutIfNeeded];
                contentView = [tableView cellForRowAtIndexPath:indexPath];
            }
            properties[GE_EVENT_PROPERTY_ELEMENT_POSITION] = [NSString stringWithFormat: @"%ld:%ld", (unsigned long)indexPath.section, (unsigned long)indexPath.row];
            
            elementPosition = [NSString stringWithFormat: @"%ld:%ld", (unsigned long)indexPath.section, (unsigned long)indexPath.row];
            
            if ([tableView.gravityEngineDelegate conformsToProtocol:@protocol(TDUIViewAutoTrackDelegate)]) {
                if ([tableView.gravityEngineDelegate respondsToSelector:@selector(gravityEngine_tableView:autoTrackPropertiesAtIndexPath:)]) {
                    NSDictionary *dic = [view.gravityEngineDelegate gravityEngine_tableView:tableView autoTrackPropertiesAtIndexPath:indexPath];
                    if ([dic isKindOfClass:[NSDictionary class]]) {
                        [properties addEntriesFromDictionary:dic];
                     
                        [yx_customProperties addEntriesFromDictionary:dic];

                    }
                }
                
                if ([tableView.gravityEngineDelegate respondsToSelector:@selector(gravityEngineWithAppid_tableView:autoTrackPropertiesAtIndexPath:)]) {
                    propertyWithAppid = [view.gravityEngineDelegate gravityEngineWithAppid_tableView:tableView autoTrackPropertiesAtIndexPath:indexPath];
                }
            }
        } else if ([view isKindOfClass:[UICollectionView class]]) {
            UICollectionView *collectionView = (UICollectionView *)view;
            contentView = [collectionView cellForItemAtIndexPath:indexPath];
            if (!contentView) {
                [collectionView layoutIfNeeded];
                contentView = [collectionView cellForItemAtIndexPath:indexPath];
            }
            properties[GE_EVENT_PROPERTY_ELEMENT_POSITION] = [NSString stringWithFormat: @"%ld:%ld", (unsigned long)indexPath.section, (unsigned long)indexPath.row];
            
            elementPosition = [NSString stringWithFormat: @"%ld:%ld", (unsigned long)indexPath.section, (unsigned long)indexPath.row];

            if ([collectionView.gravityEngineDelegate conformsToProtocol:@protocol(TDUIViewAutoTrackDelegate)]) {
                if ([collectionView.gravityEngineDelegate respondsToSelector:@selector(gravityEngine_collectionView:autoTrackPropertiesAtIndexPath:)]) {
                    NSDictionary *dic = [view.gravityEngineDelegate gravityEngine_collectionView:collectionView autoTrackPropertiesAtIndexPath:indexPath];
                    if ([dic isKindOfClass:[NSDictionary class]]) {
                        [properties addEntriesFromDictionary:dic];
                        
                        [yx_customProperties addEntriesFromDictionary:dic];

                    }
                }
                if ([collectionView.gravityEngineDelegate respondsToSelector:@selector(gravityEngineWithAppid_collectionView:autoTrackPropertiesAtIndexPath:)]) {
                    propertyWithAppid = [view.gravityEngineDelegate gravityEngineWithAppid_collectionView:collectionView autoTrackPropertiesAtIndexPath:indexPath];
                }
            }
        }
    } else {
        contentView = view;
        properties[GE_EVENT_PROPERTY_ELEMENT_POSITION] = [GEAutoTrackManager getPosition:contentView];
        
        elementPosition = [GEAutoTrackManager getPosition:contentView];

    }
    
    NSString *content = [GEAutoTrackManager getText:contentView];
    if (content.length > 0) {
        properties[GE_EVENT_PROPERTY_ELEMENT_CONTENT] = content;
        
        elementContent = content;

    }
    

    
    NSDate *trackDate = [NSDate date];
    for (NSString *appid in self.autoTrackOptions) {

        GravityEngineAutoTrackEventType type = (GravityEngineAutoTrackEventType)[self.autoTrackOptions[appid] integerValue];
        
        if (type & GravityEngineEventTypeAppClick) {
    
            
            
            
            GravityEngineSDK *instance = [GravityEngineSDK sharedInstanceWithAppid:appid];
            NSMutableDictionary *trackProperties = [properties mutableCopy];
            
            NSMutableDictionary *yx_trackProperties = [yx_customProperties mutableCopy];

            if ([instance isViewTypeIgnored:[view class]]) {
                continue;
            }
            NSDictionary *ignoreViews = view.gravityEngineIgnoreViewWithAppid;
            if (ignoreViews != nil && [[ignoreViews objectForKey:appid] isKindOfClass:[NSNumber class]]) {
                BOOL ignore = [[ignoreViews objectForKey:appid] boolValue];
                if (ignore)
                    continue;
            }
            
            if ([instance isViewControllerIgnored:viewController]) {
                continue;
            }
            
            NSDictionary *viewIDs = view.gravityEngineViewIDWithAppid;
            if (viewIDs != nil && [viewIDs objectForKey:appid]) {
                trackProperties[GE_EVENT_PROPERTY_ELEMENT_ID] = [viewIDs objectForKey:appid];
                
                elementId = [viewIDs objectForKey:appid];

            }
            
            NSDictionary *viewProperties = view.gravityEngineViewPropertiesWithAppid;
            if (viewProperties != nil && [viewProperties objectForKey:appid]) {
                NSDictionary *properties = [viewProperties objectForKey:appid];
                if ([properties isKindOfClass:[NSDictionary class]]) {
                    [trackProperties addEntriesFromDictionary:properties];

                    [yx_trackProperties addEntriesFromDictionary:properties];
                }
            }
            
            if (propertyWithAppid) {
                NSDictionary *autoTrackproperties = [propertyWithAppid objectForKey:appid];
                if ([autoTrackproperties isKindOfClass:[NSDictionary class]]) {
                    [trackProperties addEntriesFromDictionary:autoTrackproperties];
                    
                    [yx_trackProperties addEntriesFromDictionary:autoTrackproperties];
                }
            }
            
            GEAutoClickEvent *yx_event = [[GEAutoClickEvent alloc] initWithName:GE_APP_CLICK_EVENT];
            yx_event.time = trackDate;
            yx_event.elementId = elementId;
            yx_event.elementType = elementType;
            yx_event.elementContent = elementContent;
            yx_event.elementPosition = elementPosition;
            yx_event.pageTitle = elementPageTitle;
            yx_event.screenName = elementScreenName;
            
            [instance autoTrackWithEvent:yx_event properties:yx_trackProperties];
        }
    }
}

- (void)trackWithAppid:(NSString *)appid withOption:(GravityEngineAutoTrackEventType)type {
    GE_LOCK(self.trackOptionLock);
    self.autoTrackOptions[appid] = @(type);
    GE_UNLOCK(self.trackOptionLock);
    
    if (type & GravityEngineEventTypeAppClick || type & GravityEngineEventTypeAppViewScreen) {
        [self swizzleVC];
    }
    
    if (type & GravityEngineEventTypeAppInstall) {
        GEAutoTrackEvent *event = [[GEAutoTrackEvent alloc] initWithName:GE_APP_INSTALL_EVENT];
        event.time = [[NSDate date] dateByAddingTimeInterval: -1];
        [self.appInstallTracker trackWithInstanceTag:appid event:event params:nil];
    }
    
    if (type & GravityEngineEventTypeAppEnd) {
        GravityEngineSDK *instance = [GravityEngineSDK sharedInstanceWithAppid:appid];
        [instance timeEvent:GE_APP_END_EVENT];
    }

    if (type & GravityEngineEventTypeAppStart) {
        dispatch_block_t mainThreadBlock = ^(){
            NSString *eventName = [GEAppState shareInstance].relaunchInBackground ? GE_APP_START_BACKGROUND_EVENT : GE_APP_START_EVENT;
            GEAppStartEvent *event = [[GEAppStartEvent alloc] initWithName:eventName];
            event.resumeFromBackground = NO;
            if (![GEPresetProperties disableStartReason]) {
                NSString *reason = [GERunTime getAppLaunchReason];
                if (reason && reason.length) {
                    event.startReason = reason;
                }
            }
            [self.appColdStartTracker trackWithInstanceTag:appid event:event params:nil];
        };
        dispatch_async(dispatch_get_main_queue(), mainThreadBlock);
    }

    if (type & GravityEngineEventTypeAppViewCrash) {
        GravityEngineSDK *instance = [GravityEngineSDK sharedInstanceWithAppid:appid];
        [[GravityExceptionHandler sharedHandler] addGravityInstance :instance];
    }
}

- (void)viewControlWillAppear:(UIViewController *)controller {
    [self trackViewController:controller];
}

+ (UIViewController *)topPresentedViewController {
    UIWindow *keyWindow = [self findWindow];
    if (keyWindow != nil && !keyWindow.isKeyWindow) {
        [keyWindow makeKeyWindow];
    }
    
    UIViewController *topController = keyWindow.rootViewController;
    if ([topController isKindOfClass:[UINavigationController class]]) {
        topController = [(UINavigationController *)topController topViewController];
    }
    while (topController.presentedViewController) {
        topController = topController.presentedViewController;
    }
    return topController;
}

#pragma mark - Private

- (BOOL)isAutoTrackEventType:(GravityEngineAutoTrackEventType)eventType {
    BOOL isIgnored = YES;
    for (NSString *appid in self.autoTrackOptions) {
        GravityEngineAutoTrackEventType type = (GravityEngineAutoTrackEventType)[self.autoTrackOptions[appid] integerValue];
        isIgnored = !(type & eventType);
        if (isIgnored == NO)
            break;
    }
    return !isIgnored;
}

- (UIViewController *)viewControllerForView:(UIView *)view {
    UIResponder *responder = view.nextResponder;
    while (responder) {
        if ([responder isKindOfClass:[UIViewController class]]) {
            if ([responder isKindOfClass:[UINavigationController class]]) {
                responder = [(UINavigationController *)responder topViewController];
                continue;
            } else if ([responder isKindOfClass:UITabBarController.class]) {
                responder = [(UITabBarController *)responder selectedViewController];
                continue;
            }
            return (UIViewController *)responder;
        }
        responder = responder.nextResponder;
    }
    return nil;
}

- (void)trackViewController:(UIViewController *)controller {
    if (![self shouldTrackViewContrller:[controller class]]) {
        return;
    }
    
    NSString *pageUrl = nil;
    NSString *pageReferrer = nil;
    NSString *pageTitle = nil;
    NSString *pageScreenName = nil;
    NSMutableDictionary *yx_customProperties = [NSMutableDictionary dictionary];
    
    NSMutableDictionary *properties = [[NSMutableDictionary alloc] init];
    [properties setValue:NSStringFromClass([controller class]) forKey:GE_EVENT_PROPERTY_SCREEN_NAME];
    
    pageScreenName = NSStringFromClass([controller class]);
    
    NSString *controllerTitle = [self titleFromViewController:controller];
    if (controllerTitle) {
        [properties setValue:controllerTitle forKey:GE_EVENT_PROPERTY_TITLE];
        
        pageTitle = controllerTitle;
    }
    
    NSDictionary *autoTrackerAppidDic;
    if ([controller conformsToProtocol:@protocol(GEAutoTracker)]) {
        UIViewController<GEAutoTracker> *autoTrackerController = (UIViewController<GEAutoTracker> *)controller;
        NSDictionary *autoTrackerDic;
        if ([controller respondsToSelector:@selector(getTrackPropertiesWithAppid)])
            autoTrackerAppidDic = [autoTrackerController getTrackPropertiesWithAppid];
        if ([controller respondsToSelector:@selector(getTrackProperties)])
            autoTrackerDic = [autoTrackerController getTrackProperties];
        
        if ([autoTrackerDic isKindOfClass:[NSDictionary class]]) {
            [properties addEntriesFromDictionary:autoTrackerDic];
            
            [yx_customProperties addEntriesFromDictionary:autoTrackerDic];
        }
    }
    
    NSDictionary *screenAutoTrackerAppidDic;
    if ([controller conformsToProtocol:@protocol(GEScreenAutoTracker)]) {
        UIViewController<GEScreenAutoTracker> *screenAutoTrackerController = (UIViewController<GEScreenAutoTracker> *)controller;
        if ([screenAutoTrackerController respondsToSelector:@selector(getScreenUrlWithAppid)])
            screenAutoTrackerAppidDic = [screenAutoTrackerController getScreenUrlWithAppid];
        if ([screenAutoTrackerController respondsToSelector:@selector(getScreenUrl)]) {
            NSString *currentUrl = [screenAutoTrackerController getScreenUrl];
            [properties setValue:currentUrl forKey:GE_EVENT_PROPERTY_URL_PROPERTY];
            
            pageUrl = currentUrl;
            
            [properties setValue:_referrerViewControllerUrl forKey:GE_EVENT_PROPERTY_REFERRER_URL];
            
            pageReferrer = _referrerViewControllerUrl;
            
            _referrerViewControllerUrl = currentUrl;
        }
    }
    
    NSDate *trackDate = [NSDate date];
    for (NSString *appid in self.autoTrackOptions) {
        GravityEngineAutoTrackEventType type = [self.autoTrackOptions[appid] integerValue];
        if (type & GravityEngineEventTypeAppViewScreen) {
            
            
            
            GravityEngineSDK *instance = [GravityEngineSDK sharedInstanceWithAppid:appid];
            NSMutableDictionary *trackProperties = [properties mutableCopy];
            
            NSMutableDictionary *yx_trackProperties = [yx_customProperties mutableCopy];

            if ([instance isViewControllerIgnored:controller]
                || [instance isViewTypeIgnored:[controller class]]) {
                continue;
            }
            
            if (autoTrackerAppidDic && [autoTrackerAppidDic objectForKey:appid]) {
                NSDictionary *dic = [autoTrackerAppidDic objectForKey:appid];
                if ([dic isKindOfClass:[NSDictionary class]]) {
                    [trackProperties addEntriesFromDictionary:dic];
                    
                    [yx_trackProperties addEntriesFromDictionary:dic];
                }
            }
            
            if (screenAutoTrackerAppidDic && [screenAutoTrackerAppidDic objectForKey:appid]) {
                NSString *screenUrl = [screenAutoTrackerAppidDic objectForKey:appid];
                [trackProperties setValue:screenUrl forKey:GE_EVENT_PROPERTY_URL_PROPERTY];
                
                pageUrl = screenUrl;
            }
            
            GEAutoPageViewEvent *pageEvent = [[GEAutoPageViewEvent alloc] initWithName:GE_APP_VIEW_EVENT];
            pageEvent.time = trackDate;
            pageEvent.pageUrl = pageUrl;
            pageEvent.pageTitle = pageTitle;
            pageEvent.referrer = pageReferrer;
            pageEvent.screenName = pageScreenName;
            
            [instance autoTrackWithEvent:pageEvent properties:yx_trackProperties];
        }
    }
}

- (BOOL)shouldTrackViewContrller:(Class)aClass {
    return ![GEPublicConfig.controllers containsObject:NSStringFromClass(aClass)];
}

- (GravityEngineAutoTrackEventType)autoTrackOptionForAppid:(NSString *)appid {
    return (GravityEngineAutoTrackEventType)[[self.autoTrackOptions objectForKey:appid] integerValue];
}

- (void)swizzleSelected:(UIView *)view delegate:(id)delegate {
    if ([view isKindOfClass:[UITableView class]]
        && [delegate conformsToProtocol:@protocol(UITableViewDelegate)]) {
        void (^block)(id, SEL, id, id) = ^(id target, SEL command, UITableView *tableView, NSIndexPath *indexPath) {
            [self trackEventView:tableView withIndexPath:indexPath];
        };
        
        [GESwizzler swizzleSelector:@selector(tableView:didSelectRowAtIndexPath:)
                            onClass:[delegate class]
                          withBlock:block
                              named:@"ge_table_select"];
    }
    
    if ([view isKindOfClass:[UICollectionView class]]
        && [delegate conformsToProtocol:@protocol(UICollectionViewDelegate)]) {
        
        void (^block)(id, SEL, id, id) = ^(id target, SEL command, UICollectionView *collectionView, NSIndexPath *indexPath) {
            [self trackEventView:collectionView withIndexPath:indexPath];
        };
        [GESwizzler swizzleSelector:@selector(collectionView:didSelectItemAtIndexPath:)
                            onClass:[delegate class]
                          withBlock:block
                              named:@"ge_collection_select"];
    }
}

- (void)swizzleVC {
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        void (^tableViewBlock)(UITableView *tableView,
                               SEL cmd,
                               id<UITableViewDelegate> delegate) =
        ^(UITableView *tableView, SEL cmd, id<UITableViewDelegate> delegate) {
            if (!delegate) {
                return;
            }
            
            [self swizzleSelected:tableView delegate:delegate];
        };
        
        [GESwizzler swizzleSelector:@selector(setDelegate:)
                            onClass:[UITableView class]
                          withBlock:tableViewBlock
                              named:@"ge_table_delegate"];
        
        void (^collectionViewBlock)(UICollectionView *, SEL, id<UICollectionViewDelegate>) = ^(UICollectionView *collectionView, SEL cmd, id<UICollectionViewDelegate> delegate) {
            if (nil == delegate) {
                return;
            }
            
            [self swizzleSelected:collectionView delegate:delegate];
        };
        [GESwizzler swizzleSelector:@selector(setDelegate:)
                            onClass:[UICollectionView class]
                          withBlock:collectionViewBlock
                              named:@"ge_collection_delegate"];
        
        
        [UIViewController ge_swizzleMethod:@selector(viewWillAppear:)
                                withMethod:@selector(ge_autotrack_viewWillAppear:)
                                     error:NULL];
        
        [UIApplication ge_swizzleMethod:@selector(sendAction:to:from:forEvent:)
                             withMethod:@selector(ge_sendAction:to:from:forEvent:)
                                  error:NULL];
    });
}

+ (NSString *)getPosition:(UIView *)view {
    NSString *position = nil;
    if ([view isKindOfClass:[UIView class]] && view.gravityEngineIgnoreView) {
        return nil;
    }
    
    if ([view isKindOfClass:[UITabBar class]]) {
        UITabBar *tabbar = (UITabBar *)view;
        position = [NSString stringWithFormat: @"%ld", (long)[tabbar.items indexOfObject:tabbar.selectedItem]];
    } else if ([view isKindOfClass:[UISegmentedControl class]]) {
        UISegmentedControl *segment = (UISegmentedControl *)view;
        position = [NSString stringWithFormat:@"%ld", (long)segment.selectedSegmentIndex];
    } else if ([view isKindOfClass:[UIProgressView class]]) {
        UIProgressView *progress = (UIProgressView *)view;
        position = [NSString stringWithFormat:@"%f", progress.progress];
    } else if ([view isKindOfClass:[UIPageControl class]]) {
        UIPageControl *pageControl = (UIPageControl *)view;
        position = [NSString stringWithFormat:@"%ld", (long)pageControl.currentPage];
    }
    
    return position;
}

+ (NSString *)getText:(NSObject *)obj {
    NSString *text = nil;
    if ([obj isKindOfClass:[UIView class]] && [(UIView *)obj gravityEngineIgnoreView]) {
        return nil;
    }
    
    if ([obj isKindOfClass:[UIButton class]]) {
        text = ((UIButton *)obj).currentTitle;
    } else if ([obj isKindOfClass:[UITextView class]] ||
               [obj isKindOfClass:[UITextField class]]) {
        //ignore
    } else if ([obj isKindOfClass:[UILabel class]]) {
        text = ((UILabel *)obj).text;
    } else if ([obj isKindOfClass:[UIPickerView class]]) {
        UIPickerView *picker = (UIPickerView *)obj;
        NSInteger sections = picker.numberOfComponents;
        NSMutableArray *titles = [NSMutableArray array];
        
        for(NSInteger i = 0; i < sections; i++) {
            NSInteger row = [picker selectedRowInComponent:i];
            NSString *title;
            if ([picker.delegate
                 respondsToSelector:@selector(pickerView:titleForRow:forComponent:)]) {
                title = [picker.delegate pickerView:picker titleForRow:row forComponent:i];
            } else if ([picker.delegate
                        respondsToSelector:@selector(pickerView:attributedTitleForRow:forComponent:)]) {
                title = [picker.delegate
                         pickerView:picker
                         attributedTitleForRow:row forComponent:i].string;
            }
            [titles addObject:title ?: @""];
        }
        if (titles.count > 0) {
            text = [titles componentsJoinedByString:@","];
        }
    } else if ([obj isKindOfClass:[UIDatePicker class]]) {
        UIDatePicker *picker = (UIDatePicker *)obj;
        NSDateFormatter *formatter = [[NSDateFormatter alloc] init];
        formatter.dateFormat = kDefaultTimeFormat;
        text = [formatter stringFromDate:picker.date];
    } else if ([obj isKindOfClass:[UISegmentedControl class]]) {
        UISegmentedControl *segment = (UISegmentedControl *)obj;
        text =  [NSString stringWithFormat:@"%@", [segment titleForSegmentAtIndex:segment.selectedSegmentIndex]];
    } else if ([obj isKindOfClass:[UISwitch class]]) {
        UISwitch *switchItem = (UISwitch *)obj;
        text = switchItem.on ? @"on" : @"off";
    } else if ([obj isKindOfClass:[UISlider class]]) {
        UISlider *slider = (UISlider *)obj;
        text = [NSString stringWithFormat:@"%f", [slider value]];
    } else if ([obj isKindOfClass:[UIStepper class]]) {
        UIStepper *step = (UIStepper *)obj;
        text = [NSString stringWithFormat:@"%f", [step value]];
    } else {
        if ([obj isKindOfClass:[UIView class]]) {
            for(UIView *subView in [(UIView *)obj subviews]) {
                text = [GEAutoTrackManager getText:subView];
                if ([text isKindOfClass:[NSString class]] && text.length > 0) {
                    break;
                }
            }
        }
    }
    return text;
}

- (NSString *)titleFromViewController:(UIViewController *)viewController {
    if (!viewController) {
        return nil;
    }
    
    UIView *titleView = viewController.navigationItem.titleView;
    NSString *elementContent = nil;
    if (titleView) {
        elementContent = [GEAutoTrackManager getText:titleView];
    }
    
    return elementContent.length > 0 ? elementContent : viewController.navigationItem.title;
}

+ (UIWindow *)findWindow {
    UIApplication *application = [GEAppState sharedApplication];
    if (![application isKindOfClass:UIApplication.class]) {
        return nil;
    }
    
    UIWindow *window = application.keyWindow;
    if (window == nil || window.windowLevel != UIWindowLevelNormal) {
        for (window in application.windows) {
            if (window.windowLevel == UIWindowLevelNormal) {
                break;
            }
        }
    }
    
    if (@available(iOS 13.0, tvOS 13, *)) {
        NSSet *scenes = [[GEAppState sharedApplication] valueForKey:@"connectedScenes"];
        for (id scene in scenes) {
            if (window) {
                break;
            }
            
            id activationState = [scene valueForKeyPath:@"activationState"];
            BOOL isActive = activationState != nil && [activationState integerValue] == 0;
            if (isActive) {
                Class WindowScene = NSClassFromString(@"UIWindowScene");
                if ([scene isKindOfClass:WindowScene]) {
                    NSArray<UIWindow *> *windows = [scene valueForKeyPath:@"windows"];
                    for (UIWindow *w in windows) {
                        if (w.isKeyWindow) {
                            window = w;
                            break;
                        }
                    }
                }
            }
        }
    }

    return window;
}

//MARK: - App Life Cycle

- (void)registerAppLifeCycleListener {
    NSNotificationCenter *notificationCenter = [NSNotificationCenter defaultCenter];

    [notificationCenter addObserver:self selector:@selector(appStateWillChangeNotification:) name:kGEAppLifeCycleStateWillChangeNotification object:nil];
}

- (void)appStateWillChangeNotification:(NSNotification *)notification {
    GEAppLifeCycleState newState = [[notification.userInfo objectForKey:kGEAppLifeCycleNewStateKey] integerValue];
    GEAppLifeCycleState oldState = [[notification.userInfo objectForKey:kGEAppLifeCycleOldStateKey] integerValue];

    if (newState == GEAppLifeCycleStateStart) {
        for (NSString *appid in self.autoTrackOptions) {
            GravityEngineAutoTrackEventType type = (GravityEngineAutoTrackEventType)[self.autoTrackOptions[appid] integerValue];
            
            // Only open the start event of collecting hot start. Cold start event, reported when automatic collection is turned on
            if ((type & GravityEngineEventTypeAppStart) && oldState != GEAppLifeCycleStateInit) {
                NSString *eventName = [GEAppState shareInstance].relaunchInBackground ? GE_APP_START_BACKGROUND_EVENT : GE_APP_START_EVENT;
                GEAppStartEvent *event = [[GEAppStartEvent alloc] initWithName:eventName];
                event.resumeFromBackground = YES;
    
                if (![GEPresetProperties disableStartReason]) {
                    NSString *reason = [GERunTime getAppLaunchReason];
                    if (reason && reason.length) {
                        event.startReason = reason;
                    }
                }
                [self.appHotStartTracker trackWithInstanceTag:appid event:event params:@{}];
            }
            
            if (type & GravityEngineEventTypeAppEnd) {
            
                GravityEngineSDK *instance = [GravityEngineSDK sharedInstanceWithAppid:appid];
                [instance timeEvent:GE_APP_END_EVENT];
            }
        }
    } else if (newState == GEAppLifeCycleStateEnd) {
        for (NSString *appid in self.autoTrackOptions) {
            GravityEngineAutoTrackEventType type = (GravityEngineAutoTrackEventType)[self.autoTrackOptions[appid] integerValue];
            if (type & GravityEngineEventTypeAppEnd) {
                GEAppEndEvent *event = [[GEAppEndEvent alloc] initWithName:GE_APP_END_EVENT];
                ge_dispatch_main_sync_safe(^{
     
                    NSString *screenName = NSStringFromClass([[GEAutoTrackManager topPresentedViewController] class]);
                    event.screenName = screenName;
                    [self.appEndTracker trackWithInstanceTag:appid event:event params:@{}];
                });
            }
            
            if (type & GravityEngineEventTypeAppStart) {
                GravityEngineSDK *instance = [GravityEngineSDK sharedInstanceWithAppid:appid];
                [instance timeEvent:GE_APP_START_EVENT];
            }
        }
    }
}

//MARK: - Getter & Setter

- (GEHotStartTracker *)appHotStartTracker {
    if (!_appHotStartTracker) {
        _appHotStartTracker = [[GEHotStartTracker alloc] init];
    }
    return _appHotStartTracker;
}

- (GEColdStartTracker *)appColdStartTracker {
    if (!_appColdStartTracker) {
        _appColdStartTracker = [[GEColdStartTracker alloc] init];
    }
    return _appColdStartTracker;
}

- (GEInstallTracker *)appInstallTracker {
    if (!_appInstallTracker) {
        _appInstallTracker = [[GEInstallTracker alloc] init];
    }
    return _appInstallTracker;
}

- (GEAppEndTracker *)appEndTracker {
    if (!_appEndTracker) {
        _appEndTracker = [[GEAppEndTracker alloc] init];
    }
    return _appEndTracker;
}

@end
