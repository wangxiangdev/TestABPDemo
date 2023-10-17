using Volo.Abp.Ui.Branding;
using Volo.Abp.DependencyInjection;

namespace TestDemo;

[Dependency(ReplaceServices = true)]
public class TestDemoBrandingProvider : DefaultBrandingProvider
{
    public override string AppName => "TestDemo";
}
