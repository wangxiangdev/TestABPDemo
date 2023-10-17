using System;
using System.Collections.Generic;
using System.Text;
using TestDemo.Localization;
using Volo.Abp.Application.Services;

namespace TestDemo;

/* Inherit your application services from this class.
 */
public abstract class TestDemoAppService : ApplicationService
{
    protected TestDemoAppService()
    {
        LocalizationResource = typeof(TestDemoResource);
    }
}
