using AutoMapper;
using TestDemo.Products;

namespace TestDemo;

public class TestDemoApplicationAutoMapperProfile : Profile
{
    public TestDemoApplicationAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */

        CreateMap<ProductUpdateDto, Product>();
        CreateMap<Product, ProductEto>();
    }
}