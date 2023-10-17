using Volo.Abp.Settings;

namespace TestDemo.Settings;

public class TestDemoSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(TestDemoSettings.MySetting1));
    }
}
