using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.Catalog
{
    [Serializable]
    public enum KalturaBookmarkActionType
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
}