//=============================================================================
//  UnityADC.mm
//
//  iOS functionality for the Unity AdColony plug-in.
//
//  Copyright 2010 Jirbo, Inc.  All rights reserved.
//
//  ---------------------------------------------------------------------------
//
//  * Instructions *
//
//  Copy this file into your Unity project's Assets/Plugins/iOS folder.
//
//  Refer to the header comment in AdColony.cs for further instructions.
//
//=============================================================================

#import <AdColony/AdColony.h>

void UnityPause(bool pause);


@interface UnityADCIOSDelegate : NSObject<AdColonyDelegate,AdColonyAdDelegate>
{
}
// AdColonyDelegate
- (void) onAdColonyV4VCReward:(BOOL)success currencyName:(NSString *)currencyName currencyAmount:(int)amount inZone:(NSString *)zoneID;
- (void) onAdColonyAdAvailabilityChange:(BOOL)available inZone:(NSString *)zoneID;

// AdColonyAdDelegate
- (void) onAdColonyAdStartedInZone:(NSString *)zoneID;
- (void) onAdColonyAdFinishedWithInfo:( AdColonyAdInfo * )info;

@end

NSString*     adc_app_version = nil;
NSString*     adc_app_id = nil;
NSString*     adc_cur_zone = nil;
NSMutableArray* adc_zone_ids = nil;
UnityADCIOSDelegate* adc_ios_delegate = nil;
UIViewController* appViewController = nil;

NSString* set_adc_cur_zone( NSString* new_adc_cur_zone )
{
    if(new_adc_cur_zone && [new_adc_cur_zone length] > 0) {
#if __has_feature(objc_arc)
        adc_cur_zone = new_adc_cur_zone;
#else
        if (adc_cur_zone) {
            [adc_cur_zone release];
        }
        adc_cur_zone = [new_adc_cur_zone retain];
#endif
    }
    return adc_cur_zone;
}


@implementation UnityADCIOSDelegate
// AdColonyDelegate
- (void) onAdColonyV4VCReward:(BOOL)success currencyName:(NSString *)currencyName currencyAmount:(int)amount inZone:(NSString *)zoneID
{
    NSString* success_str = success ? @"true" : @"false";
    UnitySendMessage( "AdColony", "OnAdColonyV4VCResult",
                     [[NSString stringWithFormat:@"%@|%d|%@", success_str, amount, currencyName] UTF8String] );
}

- (void) onAdColonyAdAvailabilityChange:(BOOL)available inZone:(NSString *)zoneID
{
    NSString* available_str = available ? @"true" : @"false";
    UnitySendMessage( "AdColony", "OnAdColonyAdAvailabilityChange",
                     [[NSString stringWithFormat:@"%@|%@", available_str, zoneID] UTF8String] );
}

// AdColonyAdDelegate
- (void) onAdColonyAdStartedInZone:(NSString *)zoneID
{
    UnitySendMessage( "AdColony", "OnAdColonyVideoStarted", "" );
}

- (void) onAdColonyAdFinishedWithInfo:(AdColonyAdInfo *)info
{
    const char* message_info = [[NSString stringWithFormat:@"%@|%@|%lu|%@", info.shown ? @"true" : @"false", info.iapEnabled ? @"true" : @"false", (unsigned long)info.iapEngagementType, info.iapProductID ] UTF8String];
    NSLog(@"%s", message_info);
    UnitySendMessage( "AdColony", "OnAdColonyVideoFinished", message_info);
    [[UIApplication sharedApplication] keyWindow].rootViewController = appViewController;
}
@end

#include <iostream>
using namespace std;

//Important Note: Unity is going to try to free what we return from these
//We can't be sure how long [NSString* UTF8String] is going to stick around anyway
//So we use strdup

extern "C" {
    void SetCustomID( const char* custom_id ) {
        NSString* custom_id_nsstr = [NSString stringWithUTF8String:custom_id];
        [AdColony setCustomID:custom_id_nsstr];
    }

    const char* GetCustomID() {
        NSString* result_str = [AdColony getCustomID];
        return result_str ? strdup([result_str UTF8String]) : strdup("undefined");
    }

    void  IOSConfigure( const char* app_version, const char* app_id, int zone_id_count, const char* zone_ids[] ) {

#if __has_feature(objc_arc)
        adc_app_version = [NSString stringWithUTF8String:app_version];
        adc_app_id = [NSString stringWithUTF8String:app_id];
        adc_zone_ids = [[NSMutableArray alloc] initWithCapacity:zone_id_count];
        adc_ios_delegate = [[UnityADCIOSDelegate alloc] init];
#else
        adc_app_version = [[NSString stringWithUTF8String:app_version] retain];
        adc_app_id = [[NSString stringWithUTF8String:app_id] retain];
        adc_zone_ids = [[[NSMutableArray alloc] initWithCapacity:zone_id_count] retain];
        adc_ios_delegate = [[[UnityADCIOSDelegate alloc] init] retain];
#endif

        for (int i=0; i < zone_id_count; ++i) {
            NSString* zone_id_str = [NSString stringWithUTF8String:zone_ids[i]];
            [adc_zone_ids addObject:zone_id_str];
            if (i == 0) {
                set_adc_cur_zone( zone_id_str );
            }
        }

        [AdColony configureWithAppID:adc_app_id zoneIDs:adc_zone_ids delegate:adc_ios_delegate logging:NO];
    }

    bool  IsVideoAvailable( const char* zone_id ) {
        NSString* zid = adc_cur_zone;
        if (zone_id && zone_id[0] != 0) {
            zid = set_adc_cur_zone( [NSString stringWithUTF8String:zone_id] );
        }
        return [AdColony zoneStatusForZone:zid] == ADCOLONY_ZONE_STATUS_ACTIVE;
    }

    bool  IsV4VCAvailable( const char* zone_id ) {
        NSString* zid = adc_cur_zone;
        if (zone_id && zone_id[0] != 0) {
            zid = set_adc_cur_zone( [NSString stringWithUTF8String:zone_id] );
        }
        if ( !IsVideoAvailable(zone_id) ) {
            return false;
        }
        return [AdColony isVirtualCurrencyRewardAvailableForZone:zid];
    }

    const char* GetDeviceID()
    {
        NSString* result_str = [AdColony getUniqueDeviceID];
        return result_str ? strdup([result_str UTF8String]) : strdup("undefined");
    }

    const char* GetOpenUDID() {
        NSString* result_str = [AdColony getOpenUDID];
        return result_str ? strdup([result_str UTF8String]) : strdup("undefined");
    }

    const char* GetODIN1() {
        NSString* result_str = [AdColony getODIN1];
        return result_str ? strdup([result_str UTF8String]) : strdup("undefined");
    }

    int GetV4VCAmount( const char* zone_id ) {
        NSString* zid = adc_cur_zone;
        if (zone_id && zone_id[0] != 0) {
            zid = set_adc_cur_zone( [NSString stringWithUTF8String:zone_id] );
        }
        return [AdColony getVirtualCurrencyRewardAmountForZone:zid];
    }

    const char* GetV4VCName( const char* zone_id ) {
        NSString * zid = set_adc_cur_zone( [NSString stringWithUTF8String:zone_id] );
        NSString* result_str = [AdColony getVirtualCurrencyNameForZone:zid];
        return result_str ? strdup([result_str UTF8String]) : strdup("undefined");
    }

    const char* StatusForZone( const char* zone_id) {
        NSString* zid = set_adc_cur_zone( [NSString stringWithUTF8String:zone_id] );
        switch ([AdColony zoneStatusForZone:zid]) {
            case ADCOLONY_ZONE_STATUS_NO_ZONE:
                return strdup("invalid");
            case ADCOLONY_ZONE_STATUS_OFF:
                return strdup("off");
            case ADCOLONY_ZONE_STATUS_ACTIVE:
                return strdup("active");
            case ADCOLONY_ZONE_STATUS_LOADING:
                return strdup("loading");
            case ADCOLONY_ZONE_STATUS_UNKNOWN:
                return strdup("unknown");
        }
        return strdup("unknown");
    }

    bool ShowVideoAd( const char* zone_id ) {
        NSString* zid = set_adc_cur_zone( [NSString stringWithUTF8String:zone_id] );
        if ( !IsVideoAvailable(zone_id) ) {
            return false;
        }
        UIWindow* window = [[UIApplication sharedApplication] keyWindow];
        UIViewController* viewController = [UIViewController new];
        [viewController.view setBackgroundColor:[UIColor whiteColor]];
        appViewController = window.rootViewController;
        window.rootViewController = viewController;

        [AdColony playVideoAdForZone:zid withDelegate:adc_ios_delegate];
        return true;
    }

    bool  ShowV4VC( bool popup_result, const char* zone_id ) {
        NSString* zid = set_adc_cur_zone( [NSString stringWithUTF8String:zone_id] );
        if ( !IsV4VCAvailable(zone_id) ) {
            return false;
        }
        UIWindow* window = [[UIApplication sharedApplication] keyWindow];
        UIViewController* viewController = [UIViewController new];
        [viewController.view setBackgroundColor:[UIColor whiteColor]];
        appViewController = window.rootViewController;
        window.rootViewController = viewController;

        [AdColony playVideoAdForZone:zid withDelegate:adc_ios_delegate
                    withV4VCPrePopup:NO andV4VCPostPopup:popup_result];
        return true;
    }

    void  OfferV4VC( bool popup_result, const char* zone_id ) {
        NSString* zid = set_adc_cur_zone( [NSString stringWithUTF8String:zone_id] );
        if ( !IsV4VCAvailable(zone_id) ) {
            return;
        }
        UIWindow* window = [[UIApplication sharedApplication] keyWindow];
        UIViewController* viewController = [UIViewController new];
        [viewController.view setBackgroundColor:[UIColor whiteColor]];
        appViewController = window.rootViewController;
        window.rootViewController = viewController;

        [AdColony playVideoAdForZone:zid withDelegate:adc_ios_delegate
                    withV4VCPrePopup:YES andV4VCPostPopup:popup_result];
    }

    void NotifyIAPComplete( const char* product_id, const char* trans_id, const char* currency_code, double price, int quantity) {
        NSString* ns_trans_id = [NSString stringWithUTF8String:trans_id];
        NSString* ns_product_id = [NSString stringWithUTF8String:product_id];
        NSString* ns_currency_code = @"";
        if (currency_code != NULL) {
            ns_currency_code = [NSString stringWithUTF8String:currency_code];
        }
        NSNumber* ns_price = [NSNumber numberWithDouble:price];
        [AdColony notifyIAPComplete:ns_trans_id productID:ns_product_id quantity:quantity price:ns_price currencyCode:ns_currency_code];
    }

    void SetOption(const char* option, bool val) {
      [AdColony setOption:[NSString stringWithUTF8String:option] value:val];
    }
}

