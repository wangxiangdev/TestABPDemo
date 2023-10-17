using Microsoft.Extensions.Hosting;
using Volo.Abp;

namespace TestDemo.Job
{
    public class TestDemoJobHostedService : IHostedService
    {
        private readonly IAbpApplicationWithExternalServiceProvider _abpApplication;
        //private readonly TestWorker _testWorker;
        //private readonly ISCSettingUpdaterWorker _iscSettingUpdaterWorker;

        public TestDemoJobHostedService(IAbpApplicationWithExternalServiceProvider abpApplication)
        {
            _abpApplication = abpApplication;
            //_testWorker = testWorker;
            //_iscSettingUpdaterWorker = iscSettingUpdaterWorker;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            //Console.WriteLine("====================job worker starting...====================");
            //await _testWorker.StartAsync(cancellationToken);
            //await _iscSettingUpdaterWorker.StartAsync(cancellationToken);

            //Console.WriteLine("====================job worker started...====================");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            //await _abpApplication.ShutdownAsync();
        }
    }
}