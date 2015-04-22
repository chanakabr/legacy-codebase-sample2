using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Models;

namespace WebAPI.App_Start
{
    public class AutoMapperConfig
    {
        public static void RegisterMappings()
        {
            Mapper.CreateMap<Users.UserResponseObject, User>().ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.m_user.m_sSiteGUID));

        }
    }
}