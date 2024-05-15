using eVR.Reader.Demo.Services;
using eVR.Reader.Demo.ViewModels;
using eVR.Reader.PCSC.Services;
using eVR.Reader.PCSC;
using eVR.Reader.Services;
using Microsoft.Extensions.DependencyInjection;
using PCSC;
using System.Windows;
using PCSC.Monitoring;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using eVR.Reader.Validators;

namespace eVR.Reader.Demo
{
    /// <summary>
    /// Interaction logic for App.xaml, entry point of the application.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Setup the application on start up
        /// </summary>
        /// <param name="e">The startup event arguments</param>
        protected override async void OnStartup(StartupEventArgs e)
        {
            var host = Host.CreateDefaultBuilder()
               .ConfigureAppConfiguration((context, builder) =>
               {
                   var env = context.HostingEnvironment;
                   builder.SetBasePath(env.ContentRootPath);
                   builder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
               })
               .ConfigureServices((context, services) =>
               {
                   ConfigureServices(context.Configuration, services);
               })
               .Build();
            var viewmodel = host.Services.GetRequiredService<MainViewModel>();
            var view = host.Services.GetRequiredService<MainWindow>();
            await viewmodel.Initialize();
            view.DataContext = viewmodel;
            view.Show();
        }

        /// <summary>
        /// Configure the services for dependency injection.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="services"></param>
        private void ConfigureServices(IConfiguration configuration, IServiceCollection services)
        {
            var configSection = configuration.GetSection(nameof(Configuration));

            services
                .AddSingleton<MainWindow>()
                .AddSingleton<MainViewModel>()
                .AddSingleton<ISCardReader, CardReaderDecorator>()
                .AddSingleton(s => ContextFactory.Instance.Establish(SCardScope.System))
                .AddSingleton(s => MonitorFactory.Instance.Create(SCardScope.System))
                .AddSingleton<IParserTlv, ParserTlv>()
                .AddSingleton<IRandomGenerator, RandomGenerator>()
                .AddSingleton<eVRCardReader>()
                .AddSingleton<IReader>(s => s.GetRequiredService<eVRCardReader>())
                .AddSingleton<ICardReaderService, CardReaderService>()
                .AddSingleton<CsCaCache>()
                .AddSingleton<IValidator, AtrCheck>()
                .AddSingleton<IValidator, SecurityInfosCheck>()
                .AddSingleton<IValidator, PassiveAuthenticationSOdCheck>()
                .AddSingleton<IValidator, PassiveAuthenticationAACheck>()
                .AddSingleton<IValidator, ActiveAuthenticationAACheck>()
                .AddSingleton<IValidator, PassiveAuthenticationRegistrationACheck>()
                .AddSingleton<IValidator, PassiveAuthenticationRegistrationBCheck>()
                .AddSingleton<IValidator, PassiveAuthenticationRegistrationCCheck>()
                .AddOptions()
                .AddLogging(l =>
                {
                    // clear out any default configuration
                    l.ClearProviders();
                    l.AddFile(configuration.GetSection("Logging"));
                })
                .Configure<Configuration>(configSection);
        }
    }

}
