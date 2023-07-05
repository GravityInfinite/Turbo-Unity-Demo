//
//  UIView+GravityEngine.h
//  GravityEngineSDK
//
//

#import <UIKit/UIKit.h>

NS_ASSUME_NONNULL_BEGIN

@interface UIView (GravityEngine)

/**
 Set the control element ID
 */
@property (copy,nonatomic) NSString *gravityEngineViewID;

/**
 Configure the control element ID of APPID
 */
@property (strong,nonatomic) NSDictionary *gravityEngineViewIDWithAppid;

/**
 Ignore the click event of a control
 */
@property (nonatomic,assign) BOOL gravityEngineIgnoreView;

/**
 Configure APPID to ignore the click event of a control
 */
@property (strong,nonatomic) NSDictionary *gravityEngineIgnoreViewWithAppid;

/**
 Properties of custom control click event
 */
@property (strong,nonatomic) NSDictionary *gravityEngineViewProperties;

/**
 Configure the properties of the APPID custom control click event
 */
@property (strong,nonatomic) NSDictionary *gravityEngineViewPropertiesWithAppid;

/**
 gravityEngineDelegate
 */
@property (nonatomic, weak, nullable) id gravityEngineDelegate;

@end

NS_ASSUME_NONNULL_END
