using Jsonable;
using MessagePack;
using System.Collections.Generic;
using System.Text.Json.Serialization;

#pragma warning disable IDE1006  // Naming Styles
#pragma warning disable CS1591   // Missing XML comment for publicly visible type or member

namespace Sample.SampleData
{
    [JsonSerializable(typeof(Json_Twitter))]
    public partial class Json_Twitter_STJ : JsonSerializerContext { }

    [MessagePackObject(keyAsPropertyName: true)]
    [FromJson, ToJson(PreservePropertyOrder = true)]
    public partial class Json_Twitter
    {
        public Status[]? statuses { get; set; }
        public Search_Metadata? search_metadata { get; set; }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    [FromJson, ToJson(PreservePropertyOrder = true)]
    public partial class Json_TwitterReusable
    {
        public List<Status>? statuses { get; set; }
        public Search_Metadata? search_metadata { get; set; }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    [FromJson, ToJson(PreservePropertyOrder = true)]
    public partial class Search_Metadata
    {
        public float completed_in { get; set; }
        public long max_id { get; set; }
        public string? max_id_str { get; set; }
        public string? next_results { get; set; }
        public string? query { get; set; }
        public string? refresh_url { get; set; }
        public int count { get; set; }
        public int since_id { get; set; }
        public string? since_id_str { get; set; }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    [FromJson, ToJson(PreservePropertyOrder = true)]
    public partial class Status
    {
        public Status__Metadata? metadata { get; set; }
        public string? created_at { get; set; }
        public long id { get; set; }
        public string? id_str { get; set; }
        public string? text { get; set; }
        public string? source { get; set; }
        public bool truncated { get; set; }
        public long? in_reply_to_status_id { get; set; }
        public string? in_reply_to_status_id_str { get; set; }
        public long? in_reply_to_user_id { get; set; }
        public string? in_reply_to_user_id_str { get; set; }
        public string? in_reply_to_screen_name { get; set; }
        public Status__User? user { get; set; }
        public Json_Unknown? geo { get; set; }
        public Json_Unknown? coordinates { get; set; }
        public Json_Unknown? place { get; set; }
        public Json_Unknown? contributors { get; set; }
        public int retweet_count { get; set; }
        public int favorite_count { get; set; }
        public Status__Entities? entities { get; set; }
        public bool favorited { get; set; }
        public bool retweeted { get; set; }
        public string? lang { get; set; }
        public Status__Retweeted_Status? retweeted_status { get; set; }
        public bool possibly_sensitive { get; set; }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    [FromJson, ToJson(PreservePropertyOrder = true)]
    public partial class Status__Metadata
    {
        public string? result_type { get; set; }
        public string? iso_language_code { get; set; }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    [FromJson, ToJson(PreservePropertyOrder = true)]
    public partial class Status__User
    {
        public long id { get; set; }
        public string? id_str { get; set; }
        public string? name { get; set; }
        public string? screen_name { get; set; }
        public string? location { get; set; }
        public string? description { get; set; }
        public string? url { get; set; }
        public Status__User__Entities? entities { get; set; }
        public bool _protected { get; set; }
        public int followers_count { get; set; }
        public int friends_count { get; set; }
        public int listed_count { get; set; }
        public string? created_at { get; set; }
        public int favourites_count { get; set; }
        public int? utc_offset { get; set; }
        public string? time_zone { get; set; }
        public bool geo_enabled { get; set; }
        public bool verified { get; set; }
        public int statuses_count { get; set; }
        public string? lang { get; set; }
        public bool contributors_enabled { get; set; }
        public bool is_translator { get; set; }
        public bool is_translation_enabled { get; set; }
        public string? profile_background_color { get; set; }
        public string? profile_background_image_url { get; set; }
        public string? profile_background_image_url_https { get; set; }
        public bool profile_background_tile { get; set; }
        public string? profile_image_url { get; set; }
        public string? profile_image_url_https { get; set; }
        public string? profile_banner_url { get; set; }
        public string? profile_link_color { get; set; }
        public string? profile_sidebar_border_color { get; set; }
        public string? profile_sidebar_fill_color { get; set; }
        public string? profile_text_color { get; set; }
        public bool profile_use_background_image { get; set; }
        public bool default_profile { get; set; }
        public bool default_profile_image { get; set; }
        public bool following { get; set; }
        public bool follow_request_sent { get; set; }
        public bool notifications { get; set; }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    [FromJson, ToJson(PreservePropertyOrder = true)]
    public partial class Status__User__Entities
    {
        public Status__User__Entities__Description? description { get; set; }
        public Status__User__Entities__Url? url { get; set; }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    [FromJson, ToJson(PreservePropertyOrder = true)]
    public partial class Status__User__Entities__Description
    {
        public Status__User__Entities__Description__Urls[]? urls { get; set; }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    [FromJson, ToJson(PreservePropertyOrder = true)]
    public partial class Status__User__Entities__Description__Urls
    {
        public string? url { get; set; }
        public string? expanded_url { get; set; }
        public string? display_url { get; set; }
        public int[]? indices { get; set; }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    [FromJson, ToJson(PreservePropertyOrder = true)]
    public partial class Status__User__Entities__Url
    {
        public Status__User__Entities__Url__Urls[]? urls { get; set; }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    [FromJson, ToJson(PreservePropertyOrder = true)]
    public partial class Status__User__Entities__Url__Urls
    {
        public string? url { get; set; }
        public string? expanded_url { get; set; }
        public string? display_url { get; set; }
        public int[]? indices { get; set; }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    [FromJson, ToJson(PreservePropertyOrder = true)]
    public partial class Status__Entities
    {
        public Status__Entities__Hashtags[]? hashtags { get; set; }
        public Json_Unknown[]? symbols { get; set; }
        public Status__Entities__Urls[]? urls { get; set; }
        public Status__Entities__User_Mentions[]? user_mentions { get; set; }
        public Status__Entities__Media[]? media { get; set; }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    [FromJson, ToJson(PreservePropertyOrder = true)]
    public partial class Status__Entities__Hashtags
    {
        public string? text { get; set; }
        public int[]? indices { get; set; }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    [FromJson, ToJson(PreservePropertyOrder = true)]
    public partial class Status__Entities__Urls
    {
        public string? url { get; set; }
        public string? expanded_url { get; set; }
        public string? display_url { get; set; }
        public int[]? indices { get; set; }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    [FromJson, ToJson(PreservePropertyOrder = true)]
    public partial class Status__Entities__User_Mentions
    {
        public string? screen_name { get; set; }
        public string? name { get; set; }
        public long id { get; set; }
        public string? id_str { get; set; }
        public int[]? indices { get; set; }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    [FromJson, ToJson(PreservePropertyOrder = true)]
    public partial class Status__Entities__Media
    {
        public long id { get; set; }
        public string? id_str { get; set; }
        public int[]? indices { get; set; }
        public string? media_url { get; set; }
        public string? media_url_https { get; set; }
        public string? url { get; set; }
        public string? display_url { get; set; }
        public string? expanded_url { get; set; }
        public string? type { get; set; }
        public Status__Entities__Media__Sizes? sizes { get; set; }
        public long source_status_id { get; set; }
        public string? source_status_id_str { get; set; }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    [FromJson, ToJson(PreservePropertyOrder = true)]
    public partial class Status__Entities__Media__Sizes
    {
        public Status__Entities__Media__Sizes__Medium? medium { get; set; }
        public Status__Entities__Media__Sizes__Small? small { get; set; }
        public Status__Entities__Media__Sizes__Thumb? thumb { get; set; }
        public Status__Entities__Media__Sizes__Large? large { get; set; }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    [FromJson, ToJson(PreservePropertyOrder = true)]
    public partial class Status__Entities__Media__Sizes__Medium
    {
        public int w { get; set; }
        public int h { get; set; }
        public string? resize { get; set; }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    [FromJson, ToJson(PreservePropertyOrder = true)]
    public partial class Status__Entities__Media__Sizes__Small
    {
        public int w { get; set; }
        public int h { get; set; }
        public string? resize { get; set; }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    [FromJson, ToJson(PreservePropertyOrder = true)]
    public partial class Status__Entities__Media__Sizes__Thumb
    {
        public int w { get; set; }
        public int h { get; set; }
        public string? resize { get; set; }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    [FromJson, ToJson(PreservePropertyOrder = true)]
    public partial class Status__Entities__Media__Sizes__Large
    {
        public int w { get; set; }
        public int h { get; set; }
        public string? resize { get; set; }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    [FromJson, ToJson(PreservePropertyOrder = true)]
    public partial class Status__Retweeted_Status
    {
        public Status__Retweeted_Status__Metadata? metadata { get; set; }
        public string? created_at { get; set; }
        public long id { get; set; }
        public string? id_str { get; set; }
        public string? text { get; set; }
        public string? source { get; set; }
        public bool truncated { get; set; }
        public long? in_reply_to_status_id { get; set; }
        public string? in_reply_to_status_id_str { get; set; }
        public long? in_reply_to_user_id { get; set; }
        public string? in_reply_to_user_id_str { get; set; }
        public string? in_reply_to_screen_name { get; set; }
        public Status__Retweeted_Status__User? user { get; set; }
        public Json_Unknown? geo { get; set; }
        public Json_Unknown? coordinates { get; set; }
        public Json_Unknown? place { get; set; }
        public Json_Unknown? contributors { get; set; }
        public int retweet_count { get; set; }
        public int favorite_count { get; set; }
        public Status__Retweeted_Status__Entities? entities { get; set; }
        public bool favorited { get; set; }
        public bool retweeted { get; set; }
        public bool possibly_sensitive { get; set; }
        public string? lang { get; set; }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    [FromJson, ToJson(PreservePropertyOrder = true)]
    public partial class Status__Retweeted_Status__Metadata
    {
        public string? result_type { get; set; }
        public string? iso_language_code { get; set; }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    [FromJson, ToJson(PreservePropertyOrder = true)]
    public partial class Status__Retweeted_Status__User
    {
        public long id { get; set; }
        public string? id_str { get; set; }
        public string? name { get; set; }
        public string? screen_name { get; set; }
        public string? location { get; set; }
        public string? description { get; set; }
        public string? url { get; set; }
        public Status__Retweeted_Status__User__Entities? entities { get; set; }
        public bool _protected { get; set; }
        public int followers_count { get; set; }
        public int friends_count { get; set; }
        public int listed_count { get; set; }
        public string? created_at { get; set; }
        public int favourites_count { get; set; }
        public int? utc_offset { get; set; }
        public string? time_zone { get; set; }
        public bool geo_enabled { get; set; }
        public bool verified { get; set; }
        public int statuses_count { get; set; }
        public string? lang { get; set; }
        public bool contributors_enabled { get; set; }
        public bool is_translator { get; set; }
        public bool is_translation_enabled { get; set; }
        public string? profile_background_color { get; set; }
        public string? profile_background_image_url { get; set; }
        public string? profile_background_image_url_https { get; set; }
        public bool profile_background_tile { get; set; }
        public string? profile_image_url { get; set; }
        public string? profile_image_url_https { get; set; }
        public string? profile_banner_url { get; set; }
        public string? profile_link_color { get; set; }
        public string? profile_sidebar_border_color { get; set; }
        public string? profile_sidebar_fill_color { get; set; }
        public string? profile_text_color { get; set; }
        public bool profile_use_background_image { get; set; }
        public bool default_profile { get; set; }
        public bool default_profile_image { get; set; }
        public bool following { get; set; }
        public bool follow_request_sent { get; set; }
        public bool notifications { get; set; }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    [FromJson, ToJson(PreservePropertyOrder = true)]
    public partial class Status__Retweeted_Status__User__Entities
    {
        public Status__Retweeted_Status__User__Entities__Description? description { get; set; }
        public Status__Retweeted_Status__User__Entities__Url? url { get; set; }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    [FromJson, ToJson(PreservePropertyOrder = true)]
    public partial class Status__Retweeted_Status__User__Entities__Description
    {
        public Status__Retweeted_Status__User__Entities__Description__Urls[]? urls { get; set; }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    [FromJson, ToJson(PreservePropertyOrder = true)]
    public partial class Status__Retweeted_Status__User__Entities__Description__Urls
    {
        public string? url { get; set; }
        public string? expanded_url { get; set; }
        public string? display_url { get; set; }
        public int[]? indices { get; set; }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    [FromJson, ToJson(PreservePropertyOrder = true)]
    public partial class Status__Retweeted_Status__User__Entities__Url
    {
        public Status__Retweeted_Status__User__Entities__Url__Urls[]? urls { get; set; }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    [FromJson, ToJson(PreservePropertyOrder = true)]
    public partial class Status__Retweeted_Status__User__Entities__Url__Urls
    {
        public string? url { get; set; }
        public string? expanded_url { get; set; }
        public string? display_url { get; set; }
        public int[]? indices { get; set; }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    [FromJson, ToJson(PreservePropertyOrder = true)]
    public partial class Status__Retweeted_Status__Entities
    {
        public Status__Retweeted_Status__Entities__Hashtags[]? hashtags { get; set; }
        public Json_Unknown[]? symbols { get; set; }
        public Status__Retweeted_Status__Entities__Urls[]? urls { get; set; }
        public Status__Retweeted_Status__Entities__User_Mentions[]? user_mentions { get; set; }
        public Status__Retweeted_Status__Entities__Media[]? media { get; set; }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    [FromJson, ToJson(PreservePropertyOrder = true)]
    public partial class Status__Retweeted_Status__Entities__Hashtags
    {
        public string? text { get; set; }
        public int[]? indices { get; set; }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    [FromJson, ToJson(PreservePropertyOrder = true)]
    public partial class Status__Retweeted_Status__Entities__Urls
    {
        public string? url { get; set; }
        public string? expanded_url { get; set; }
        public string? display_url { get; set; }
        public int[]? indices { get; set; }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    [FromJson, ToJson(PreservePropertyOrder = true)]
    public partial class Status__Retweeted_Status__Entities__User_Mentions
    {
        public string? screen_name { get; set; }
        public string? name { get; set; }
        public long id { get; set; }
        public string? id_str { get; set; }
        public int[]? indices { get; set; }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    [FromJson, ToJson(PreservePropertyOrder = true)]
    public partial class Status__Retweeted_Status__Entities__Media
    {
        public long id { get; set; }
        public string? id_str { get; set; }
        public int[]? indices { get; set; }
        public string? media_url { get; set; }
        public string? media_url_https { get; set; }
        public string? url { get; set; }
        public string? display_url { get; set; }
        public string? expanded_url { get; set; }
        public string? type { get; set; }
        public Status__Retweeted_Status__Entities__Media__Sizes? sizes { get; set; }
        public long source_status_id { get; set; }
        public string? source_status_id_str { get; set; }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    [FromJson, ToJson(PreservePropertyOrder = true)]
    public partial class Status__Retweeted_Status__Entities__Media__Sizes
    {
        public Status__Retweeted_Status__Entities__Media__Sizes__Medium? medium { get; set; }
        public Status__Retweeted_Status__Entities__Media__Sizes__Small? small { get; set; }
        public Status__Retweeted_Status__Entities__Media__Sizes__Thumb? thumb { get; set; }
        public Status__Retweeted_Status__Entities__Media__Sizes__Large? large { get; set; }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    [FromJson, ToJson(PreservePropertyOrder = true)]
    public partial class Status__Retweeted_Status__Entities__Media__Sizes__Medium
    {
        public int w { get; set; }
        public int h { get; set; }
        public string? resize { get; set; }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    [FromJson, ToJson(PreservePropertyOrder = true)]
    public partial class Status__Retweeted_Status__Entities__Media__Sizes__Small
    {
        public int w { get; set; }
        public int h { get; set; }
        public string? resize { get; set; }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    [FromJson, ToJson(PreservePropertyOrder = true)]
    public partial class Status__Retweeted_Status__Entities__Media__Sizes__Thumb
    {
        public int w { get; set; }
        public int h { get; set; }
        public string? resize { get; set; }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    [FromJson, ToJson(PreservePropertyOrder = true)]
    public partial class Status__Retweeted_Status__Entities__Media__Sizes__Large
    {
        public int w { get; set; }
        public int h { get; set; }
        public string? resize { get; set; }
    }
}
