//using BenchmarkDotNet.Attributes;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Runtime.CompilerServices;
//using System.Text;
//using System.Threading.Tasks;

//namespace UnlimitSoft.Benchmark.UnlimitSoft.Logger
//{
//    [MemoryDiagnoser]
//    public class LoggerTest
//    {
//        private readonly ILogger _logger;
//        private readonly IServiceProvider _provider;
//        private const string template = "Test {Id}";

//        public LoggerTest()
//        {
//            var services = new ServiceCollection();
//            services.AddLogging(setup =>
//            {
//                setup.SetMinimumLevel(LogLevel.Warning);
//                setup.AddSimpleConsole();
//            });

//            _provider = services.BuildServiceProvider();
//            _logger = _provider.GetService<ILogger<LoggerTest>>();
//        }


//        [Benchmark]
//        public void LogMethod()
//        {
//            _logger.LogInformation(template, args: 3);
//        }
//        [Benchmark]
//        public void LogWithIfMethod()
//        {
//            if (_logger.IsEnabled(LogLevel.Information))
//                _logger.LogInformation(template, args: new object[] { 3 });
//        }
//        [Benchmark]
//        public void LogWithExtensionMethod()
//        {
//            _logger.LogInformation(template, 3);
//        }
//    }
//}