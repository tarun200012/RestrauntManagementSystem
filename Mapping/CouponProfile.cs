using AutoMapper;
using RestaurantAPI.Dtos;
using RestaurantAPI.DTOs.RestaurantAPI.DTOs.Coupon;
using RestaurantAPI.Models;

namespace RestaurantAPI.Mapping
{
    // Renamed the class to avoid conflict with another 'MappingProfile' in the same namespace  
    public class CouponProfile : Profile
    {
        public CouponProfile()
        {
            CreateMap<Coupon, CouponResponseDto>()
                .ForMember(dest => dest.CustomerIds,
                           opt => opt.MapFrom(src => src.CouponCustomers.Select(cc => cc.CustomerId)))
                .ForMember(dest => dest.RestaurantIds,
                           opt => opt.MapFrom(src => src.CouponRestaurants.Select(cr => cr.RestaurantId)));
        }
    }
}
