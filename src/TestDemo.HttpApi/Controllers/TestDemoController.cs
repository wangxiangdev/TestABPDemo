using TestDemo.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace TestDemo.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class TestDemoController : AbpControllerBase
{
    protected TestDemoController()
    {
        LocalizationResource = typeof(TestDemoResource);
    }
}
