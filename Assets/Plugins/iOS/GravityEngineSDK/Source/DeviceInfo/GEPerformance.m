//
//  GEPerformance.m
//  GravityEngineSDK
//
//

#import "GEPerformance.h"
#import "GEFPSMonitor.h"
#include <mach/mach.h>
#include <malloc/malloc.h>
#import <sys/sysctl.h>
#include <mach-o/arch.h>
#import <objc/message.h>
#import "GEPresetProperties+GEDisProperties.h"

typedef GEPresetProperties TDAPMPresetProperty;

static const NSString *kGEPerformanceRAM  = @"$ram";
static const NSString *kGEPerformanceDISK = @"$disk";
static const NSString *kGEPerformanceSIM  = @"$simulator";
static const NSString *kGEPerformanceFPS  = @"$fps";

#define GE_MAIM_INFO_PLIST_DISPRESTPRO_KEY @"GEDisPresetProperties"

#define GE_PM_UNIT_KB 1024.0
#define GE_PM_UNIT_MB (1024.0 * GE_PM_UNIT_KB)
#define GE_PM_UNIT_GB (1024.0 * GE_PM_UNIT_MB)

GEFPSMonitor *geFpsMonitor;

@implementation GEPerformance

+ (NSDictionary *)getPresetProperties {

    NSMutableDictionary *dic = [NSMutableDictionary dictionary];
    
//    if (![TDAPMPresetProperty disableRAM]) {
//        NSString *ram = [NSString stringWithFormat:@"%.1f/%.1f",
//                         [GEPerformance ge_pm_func_getFreeMemory]*1.0/GE_PM_UNIT_GB,
//                         [GEPerformance ge_pm_func_getRamSize]*1.0/GE_PM_UNIT_GB];
//        if (ram && ram.length) {
//            [dic setObject:ram forKey:kGEPerformanceRAM];
//        }
//    }
    
//    if (![TDAPMPresetProperty disableDisk]) {
//        NSString *disk = [NSString stringWithFormat:@"%.1f/%.1f",
//                          [GEPerformance ge_get_disk_free_size]*1.0/GE_PM_UNIT_GB,
//                          [GEPerformance ge_get_storage_size]*1.0/GE_PM_UNIT_GB];
//        if (disk && disk.length) {
//            [dic setObject:disk forKey:kGEPerformanceDISK];
//        }
//    }
    
    if (![TDAPMPresetProperty disableSimulator]) {
        
#ifdef TARGET_OS_IPHONE
    #if TARGET_IPHONE_SIMULATOR
        [dic setObject:@(YES) forKey:kGEPerformanceSIM];
    #elif TARGET_OS_SIMULATOR
        [dic setObject:@(YES) forKey:kGEPerformanceSIM];
    #else
        [dic setObject:@(NO) forKey:kGEPerformanceSIM];
    #endif
#else
        [dic setObject:@(YES) forKey:kGEPerformanceSIM];
#endif
    }
    
//    if (![TDAPMPresetProperty disableFPS]) {
//        if (!geFpsMonitor) {
//            geFpsMonitor = [[GEFPSMonitor alloc] init];
//            [geFpsMonitor setEnable:YES];
//            [dic setObject:[geFpsMonitor getPFS] forKey:kGEPerformanceFPS];
//        } else {
//            [dic setObject:[geFpsMonitor getPFS] forKey:kGEPerformanceFPS];
//        }
//    }
    return dic;
}

#pragma mark - memory

+ (int64_t)ge_pm_func_getFreeMemory {
    size_t length = 0;
    int mib[6] = {0};
    
    int pagesize = 0;
    mib[0] = CTL_HW;
    mib[1] = HW_PAGESIZE;
    length = sizeof(pagesize);
    if (sysctl(mib, 2, &pagesize, &length, NULL, 0) < 0){
        return -1;
    }
    mach_msg_type_number_t count = HOST_VM_INFO_COUNT;
    vm_statistics_data_t vmstat;
    if (host_statistics(mach_host_self(), HOST_VM_INFO, (host_info_t)&vmstat, &count) != KERN_SUCCESS){
        return -1;
    }
    
    int64_t freeMem = vmstat.free_count * pagesize;
    int64_t inactiveMem = vmstat.inactive_count * pagesize;
    return freeMem + inactiveMem;
}

+ (int64_t)ge_pm_func_getRamSize{
    int mib[2];
    size_t length = 0;
    
    mib[0] = CTL_HW;
    mib[1] = HW_MEMSIZE;
    long ram;
    length = sizeof(ram);
    if (sysctl(mib, 2, &ram, &length, NULL, 0) < 0) {
        return -1;
    }
    return ram;
}

#pragma mark - disk

+ (NSDictionary *)ge_pm_getFileAttributeDic {
    NSError *error;
    NSDictionary *directory = [[NSFileManager defaultManager] attributesOfFileSystemForPath:NSHomeDirectory() error:&error];
    if (error) {
        return nil;
    }
    return directory;
}

+ (long long)ge_get_disk_free_size {
    NSDictionary<NSFileAttributeKey, id> *directory = [self ge_pm_getFileAttributeDic];
    if (directory) {
        return [[directory objectForKey:NSFileSystemFreeSize] unsignedLongLongValue];
    }
    return -1;
}

+ (long long)ge_get_storage_size {
    NSDictionary<NSFileAttributeKey, id> *directory = [self ge_pm_getFileAttributeDic];
    return directory ? ((NSNumber *)[directory objectForKey:NSFileSystemSize]).unsignedLongLongValue:-1;
}

@end


@implementation GEPerformance (PresetProperty)

+ (NSArray*)disPerformancePresetProperties {
    static NSArray *arr;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        arr = (NSArray *)[[[NSBundle mainBundle] infoDictionary] objectForKey:GE_MAIM_INFO_PLIST_DISPRESTPRO_KEY];
    });
    return arr;
}

+ (BOOL)needFPS {
    return ![[self disPerformancePresetProperties] containsObject:kGEPerformanceFPS];
}

+ (BOOL)needRAM {
    return ![[self disPerformancePresetProperties] containsObject:kGEPerformanceRAM];
}

+ (BOOL)needDisk {
    return ![[self disPerformancePresetProperties] containsObject:kGEPerformanceDISK];
}

+ (BOOL)needSimulator {
    return ![[self disPerformancePresetProperties] containsObject:kGEPerformanceSIM];
}


@end
