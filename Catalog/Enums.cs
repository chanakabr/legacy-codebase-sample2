using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Catalog
{  
    public enum MetasEnum
    {
        META1_STR = 1,
        META2_STR = 2,
        META3_STR = 3,
        META4_STR = 4,
        META5_STR = 5,
        META6_STR = 6,
        META7_STR = 7,
        META8_STR = 8,
        META9_STR = 9,
        META10_STR = 10,
        META11_STR = 11,
        META12_STR = 12,
        META13_STR = 13,
        META14_STR = 14,
        META15_STR = 15,
        META16_STR = 16,
        META17_STR = 17,
        META18_STR = 18,
        META19_STR = 19,
        META20_STR = 20,
        META1_DOUBLE = 21,
        META2_DOUBLE = 22,
        META3_DOUBLE = 23,
        META4_DOUBLE = 24,
        META5_DOUBLE = 25,
        META6_DOUBLE = 26,
        META7_DOUBLE = 27,
        META8_DOUBLE = 28,
        META9_DOUBLE = 29,
        META10_DOUBLE = 30,
        META1_BOOL = 31,
        META2_BOOL = 32,
        META3_BOOL = 33,
        META4_BOOL = 34,
        META5_BOOL = 35,
        META6_BOOL = 36,
        META7_BOOL = 37,
        META8_BOOL = 38,
        META9_BOOL = 39,
        META10_BOOL = 40
    }

    public enum MediaPlayActions
    {
        HIT = 0,
        PLAY = 1,
        STOP = 2,
        PAUSE = 3,
        FIRST_PLAY = 4,
        SWOOSH = 5,
        FULL_SCREEN = 6,
        SEND_TO_FRIEND = 7,
        LOAD = 8,
        FULL_SCREEN_EXIT = 9,
        FINISH = 10,
        BITRATE_CHANGE = 40,
        ERROR = 18,
        NONE = 99
    }

    public enum MediaPlayResponse
    {
        MEDIA_MARK,
        CONCURRENT,
        HIT,
        OK,
        ACTION_NOT_RECOGNIZED,
        ERROR
    }

    public enum StatusComment
    {
        SUCCESS = 1,
        FAIL = 2
    }

    //public enum eIngestType
    //{
    //    Tvinci = 0,
    //    Adi = 1,
    //    KalturaEpg = 2
    //}

    public enum eBundleType
    {
        SUBSCRIPTION = 0,
        COLLECTION = 1
    }

    public enum eUserType
    {
        HOUSEHOLD = 0,
        PERSONAL = 1
    }

}
