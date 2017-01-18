using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.SearchObjects
{
    public static class ESMediaFields
    {
        public static readonly string DATE_FORMAT = "yyyyMMddHHmmss";

        public static readonly string ID = "_id";
        public static readonly string MEDIA = "media";

        public static readonly string IS_ACTIVE = "is_active";
        public static readonly string MEDIA_ID= "media_id";
        public static readonly string GROUP_ID = "group_id";
        public static readonly string START_DATE = "start_date";
        public static readonly string END_DATE = "end_date";
        public static readonly string MEDIA_TYPE_ID = "media_type_id";
        public static readonly string WP_TYPE_ID = "wp_type_id";
        public static readonly string DEVICE_RULE_ID = "device_rule_id";
        public static readonly string LIKE_COUNTER = "like_counter";
        public static readonly string VIEWS = "views";
        public static readonly string RATING = "rating";
        public static readonly string VOTES = "votes";
        public static readonly string FINAL_DATE = "final_date";
        public static readonly string CREATE_DATE = "create_date";
        public static readonly string UPDATE_DATE = "update_date";
        public static readonly string NAME = "name";
        public static readonly string NAME_ANALYZED = "name.analyzed";
        public static readonly string DESCRIPTION = "description";
        public static readonly string DESCRIPTION_ANALYZED = "description.analyzed";
        public static readonly string CACHE_DATE = "cache_date";
        public static readonly string MEDIA_FILE_TYPES = "media_file_types";
        public static readonly string USER_TYPES = "user_types";
        public static readonly string METAS = "metas";
        public static readonly string TAGS = "tags";
        public static readonly string METAS_FILL = "metas.{0}";
        public static readonly string TAGS_FILL = "tags.{0}";
        public static readonly string METAS_ANALYZED_FILL = "metas.{0}.analyzed";
        public static readonly string TAGS_ANALYZED_FILL = "tags.{0}.analyzed";


        public static string Fill(this string formatString, params object[] args)
        {
            return string.Format(formatString, args);
        }
      
    }
}


