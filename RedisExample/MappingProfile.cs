using AutoMapper;
using RedisExample.DTO;
using RedisExample.Model;

namespace RedisExample
{
   
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ProductDto, Product>();
            CreateMap<Product, ProductDto>();
            // Category mappings
            CreateMap<CategoryDto, Category>();
            CreateMap<Category, CategoryDto>();
        }
    }

}
