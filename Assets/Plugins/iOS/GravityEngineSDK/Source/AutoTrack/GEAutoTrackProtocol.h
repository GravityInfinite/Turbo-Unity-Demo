//
//  GEAutoTrackProtocol.h
//  GravityEngineSDK
//
//

#ifndef GEAutoTrackProtocol_h
#define GEAutoTrackProtocol_h

#import <UIKit/UIKit.h>

@protocol TDUIViewAutoTrackDelegate

@optional

/**
 UITableView event properties

 @return event properties
 */
- (NSDictionary *)gravityEngine_tableView:(UITableView *)tableView autoTrackPropertiesAtIndexPath:(NSIndexPath *)indexPath;

/**
 APPID UITableView event properties
 
 @return event properties
 */
- (NSDictionary *)gravityEngineWithAppid_tableView:(UITableView *)tableView autoTrackPropertiesAtIndexPath:(NSIndexPath *)indexPath;

@optional

/**
 UICollectionView event properties

 @return event properties
 */
- (NSDictionary *)gravityEngine_collectionView:(UICollectionView *)collectionView autoTrackPropertiesAtIndexPath:(NSIndexPath *)indexPath;

/**
 APPID UICollectionView event properties

 @return event properties
 */
- (NSDictionary *)gravityEngineWithAppid_collectionView:(UICollectionView *)collectionView autoTrackPropertiesAtIndexPath:(NSIndexPath *)indexPath;

@end


@protocol GEAutoTracker

@optional

- (NSDictionary *)getTrackProperties;


- (NSDictionary *)getTrackPropertiesWithAppid;

@end

/**
 Automatically track the page
 */
@protocol GEScreenAutoTracker <GEAutoTracker>

@optional

/**
 Attributes for custom page view events
 */
- (NSString *)getScreenUrl;

/**
 Configure the properties of the APPID custom page view event
 */
- (NSDictionary *)getScreenUrlWithAppid;

@end

#endif /* GEAutoTrackProtocol_h */
