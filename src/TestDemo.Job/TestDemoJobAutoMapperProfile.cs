using AutoMapper;
using TestDemo.Products;

namespace TestDemo.Job;

public class TestDemoJobAutoMapperProfile : Profile
{
    public TestDemoJobAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */

        CreateMap<ProductEto, Product>();
    }
}