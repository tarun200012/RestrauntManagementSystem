using AutoMapper;
using RestaurantAPI.Dtos;
using RestaurantAPI.Models;

namespace RestaurantAPI.Mapping
{
    // Renamed the class to avoid conflict with another 'MappingProfile' in the same namespace  
    public class RestaurantMappingProfile : Profile
    {
        public RestaurantMappingProfile()
        {
            CreateMap<Restaurant, RestaurantWithLocationDto>().ReverseMap();
            CreateMap<Location, LocationDto>().ReverseMap();
        }
    }
}
