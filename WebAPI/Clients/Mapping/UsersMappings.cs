using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Models;
using WebAPI.Utils;

namespace WebAPI.Clients.Mapping
{
    public class UsersMappings
    {
        public static void RegisterMappings()
        {
            //User 
            Mapper.CreateMap<Users.UserResponseObject, User>()
                .ForMember(dest => dest.ID, opt => opt.MapFrom(src => SerializationUtils.MaskSensitiveObject(src.m_user.m_sSiteGUID)))
                .ForMember(dest => dest.DomainID, opt => opt.MapFrom(src => SerializationUtils.MaskSensitiveObject(src.m_user.m_domianID.ToString())))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.m_user.m_oBasicData.m_sFirstName));


        }
    }
}