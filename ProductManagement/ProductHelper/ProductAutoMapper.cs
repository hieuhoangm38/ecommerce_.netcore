﻿using AutoMapper;
using WebApi.Login_Logout_forgotPass.Entities;
using WebApi.Login_Logout_forgotPass.Models.Users;
using WebApi.ProductManagement.ProductEntities;
using WebApi.ProductManagement.ProductModel;

namespace WebApi.ProductManagement.ProductHelper
{
    public class ProductAutoMapper : Profile
    {
        public ProductAutoMapper() 
        {
            CreateMap<AddProductRequest, Product>();
        }
    }
}
//public class AutoMapperProfile : Profile
//{
//    public AutoMapperProfile()
//    {
//        // User -> AuthenticateResponse
//        CreateMap<User, AuthenticateResponse>();
//        CreateMap<User, RefreshTokenResponse>();

//        // RegisterRequest -> User
//        CreateMap<RegisterRequest, User>();

//        //RegisterRequest -> Identify
//        CreateMap<RegisterRequest, Identify>();

//        // UpdateRequest -> User
//        CreateMap<UpdateRequest, User>()
//            .ForAllMembers(x => x.Condition(
//                (src, dest, prop) =>
//                {
//                    // ignore null & empty string properties
//                    if (prop == null) return false;
//                    if (prop.GetType() == typeof(string) && string.IsNullOrEmpty((string)prop)) return false;

//                    return true;
//                }
//            ));
//    }
//}