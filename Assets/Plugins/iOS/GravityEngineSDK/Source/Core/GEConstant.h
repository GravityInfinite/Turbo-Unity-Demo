//
//  GEConstant.h
//  GravityEngineSDK
//
//  Copyright © 2020 gravityengine. All rights reserved.
//

#import <Foundation/Foundation.h>
/**
Debug Mode

- GravityEngineDebugOff : Not enabled by default
*/
typedef NS_OPTIONS(NSInteger, GravityEngineDebugMode) {
    /**
     Not enabled by default
     */
    GravityEngineDebugOff      = 0,
    
    
    /**
     Enable Debug Mode，Data will persist
     */
    GravityEngineDebugOn         = 1 << 1,
    
};


/**
 Log Level

 - GELoggingLevelNone : Not enabled by default
 */
typedef NS_OPTIONS(NSInteger, GELoggingLevel) {
    /**
     Not enabled by default
     */
    GELoggingLevelNone  = 0,
    
    /**
     Error Log
     */
    GELoggingLevelError = 1 << 0,
    
    /**
     Info  Log
     */
    GELoggingLevelInfo  = 1 << 1,
    
    /**
     Debug Log
     */
    GELoggingLevelDebug = 1 << 2,
};

/**
 Https Certificate Verification Mode
*/
typedef NS_OPTIONS(NSInteger, GESSLPinningMode) {
    /**
     The default authentication method will only verify the certificate returned by the server in the system's trusted certificate list
    */
    GESSLPinningModeNone          = 0,
    
    /**
     The public key of the verification certificate
    */
    GESSLPinningModePublicKey     = 1 << 0,
    
    /**
     Verify all contents of the certificate
    */
    GESSLPinningModeCertificate   = 1 << 1
};

/**
 Custom HTTPS Authentication
*/
typedef NSURLSessionAuthChallengeDisposition (^TDURLSessionDidReceiveAuthenticationChallengeBlock)(NSURLSession *_Nullable session, NSURLAuthenticationChallenge *_Nullable challenge, NSURLCredential *_Nullable __autoreleasing *_Nullable credential);



/**
 Network Type Enum

 - GENetworkTypeDefault :  3G、4G、WIFI
 */
typedef NS_OPTIONS(NSInteger, GravityEngineNetworkType) {
    
    /**
     3G、4G、WIFI
     */
    GENetworkTypeDefault  = 0,
    
    /**
     only WIFI
     */
    GENetworkTypeOnlyWIFI = 1 << 0,
    
    /**
     2G、3G、4G、WIFI
     */
    GENetworkTypeALL      = 1 << 1,
};

/**
 Auto-Tracking Enum

 - GravityEngineEventTypeNone           : auto-tracking is not enabled by default
 */
typedef NS_OPTIONS(NSInteger, GravityEngineAutoTrackEventType) {
    
    /**
     auto-tracking is not enabled by default
     */
    GravityEngineEventTypeNone          = 0,
    
    /*
     Active Events
     */
    GravityEngineEventTypeAppStart      = 1 << 0,
    
    /**
     Inactive Events
     */
    GravityEngineEventTypeAppEnd        = 1 << 1,
    
    /**
     Clicked events
     */
    GravityEngineEventTypeAppClick      = 1 << 2,
    
    /**
     View Page Events
     */
    GravityEngineEventTypeAppViewScreen = 1 << 3,
    
    /**
     Crash Events
     */
    GravityEngineEventTypeAppViewCrash  = 1 << 4,
    
    /**
     Installation Events
     */
    GravityEngineEventTypeAppInstall    = 1 << 5,
    /**
     All  Events
     */
    GravityEngineEventTypeAll    = GravityEngineEventTypeAppStart | GravityEngineEventTypeAppEnd | GravityEngineEventTypeAppClick | GravityEngineEventTypeAppInstall | GravityEngineEventTypeAppViewScreen

};

typedef NS_OPTIONS(NSInteger, GravityNetworkType) {
    GravityNetworkTypeNONE     = 0,
    GravityNetworkType2G       = 1 << 0,
    GravityNetworkType3G       = 1 << 1,
    GravityNetworkType4G       = 1 << 2,
    GravityNetworkTypeWIFI     = 1 << 3,
    GravityNetworkType5G       = 1 << 4,
    GravityNetworkTypeALL      = 0xFF,
};


typedef NS_OPTIONS(NSInteger, GEThirdPartyShareType) {
    GEThirdPartyShareTypeNONE               = 0,
    GEThirdPartyShareTypeAPPSFLYER          = 1 << 0,
    GEThirdPartyShareTypeIRONSOURCE         = 1 << 1,
    GEThirdPartyShareTypeADJUST             = 1 << 2,
    GEThirdPartyShareTypeBRANCH             = 1 << 3,
    GEThirdPartyShareTypeTOPON              = 1 << 4,
    GEThirdPartyShareTypeTRACKING           = 1 << 5,
    GEThirdPartyShareTypeTRADPLUS           = 1 << 6,
    GEThirdPartyShareTypeAPPLOVIN           = 1 << 7,
    GEThirdPartyShareTypeKOCHAVA            = 1 << 8,
    GEThirdPartyShareTypeTALKINGDATA        = 1 << 9,
    GEThirdPartyShareTypeFIREBASE           = 1 << 10,
    
    
    TDThirdPartyShareTypeNONE               = GEThirdPartyShareTypeNONE,
    TDThirdPartyShareTypeAPPSFLYER          = GEThirdPartyShareTypeAPPSFLYER,
    TDThirdPartyShareTypeIRONSOURCE         = GEThirdPartyShareTypeIRONSOURCE,
    TDThirdPartyShareTypeADJUST             = GEThirdPartyShareTypeADJUST,
    TDThirdPartyShareTypeBRANCH             = GEThirdPartyShareTypeBRANCH,
    TDThirdPartyShareTypeTOPON              = GEThirdPartyShareTypeTOPON,
    TDThirdPartyShareTypeTRACKING           = GEThirdPartyShareTypeTRACKING,
    TDThirdPartyShareTypeTRADPLUS           = GEThirdPartyShareTypeTRADPLUS,
    
};

//MARK: - Data reporting status
typedef NS_ENUM(NSInteger, GETrackStatus) {
    /// Suspend reporting
    GETrackStatusPause,
    /// Stop reporting and clear cache
    GETrackStatusStop,
    /// Suspend reporting and continue to persist data
    GETrackStatusSaveOnly,
    /// reset normal
    GETrackStatusNormal
};
