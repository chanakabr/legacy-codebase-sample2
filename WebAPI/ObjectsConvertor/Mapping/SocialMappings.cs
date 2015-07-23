using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;

namespace WebAPI.ObjectsConvertor.Mapping
{
    public class SocialMappings
    {
        public static void RegisterMappings()
        {
            // FacebookResponse to ClientFacebookResponse
            Mapper.CreateMap<WebAPI.Social.FacebookResponseObject, WebAPI.Models.Social.KalturaFacebookResponse>()
                .ForMember(dest => dest.Data, opt => opt.MapFrom(src => src.data))
                .ForMember(dest => dest.FacebookName, opt => opt.MapFrom(src => src.facebookName))
                .ForMember(dest => dest.FacebookUser, opt => opt.MapFrom(src => src.fbUser))
                .ForMember(dest => dest.KalturaName, opt => opt.MapFrom(src => src.tvinciName))
                .ForMember(dest => dest.MinFriends, opt => opt.MapFrom(src => src.minFriends))
                .ForMember(dest => dest.Pic, opt => opt.MapFrom(src => src.pic))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.status))
                .ForMember(dest => dest.Token, opt => opt.MapFrom(src => src.token))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.siteGuid));

            // FBUser to FacebookUser
            Mapper.CreateMap<WebAPI.Social.FBUser, WebAPI.Models.Social.KalturaFacebookUser>()
                .ForMember(dest => dest.Birthday, opt => opt.MapFrom(src => src.Birthday))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.email))
                .ForMember(dest => dest.FacebookId, opt => opt.MapFrom(src => src.id))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.first_name))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.gender))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.last_name))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.name))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.m_sSiteGuid));
        }

        public static WebAPI.Social.KeyValuePair[] ConvertDictionaryToKeyValue(Dictionary<string, string> dictionary)
        {
            if (dictionary != null && dictionary.Count > 0)
            {
                WebAPI.Social.KeyValuePair[] keyValuePair = new Social.KeyValuePair[dictionary.Count];

                for (int i = 0; i < dictionary.Count; i++)
                {
                    keyValuePair[i] = new Social.KeyValuePair()
                    {
                        key = dictionary.ElementAt(i).Key,
                        value = dictionary.ElementAt(i).Value
                    };
                }

                return keyValuePair;
            }

            return null;
        }
    }
}