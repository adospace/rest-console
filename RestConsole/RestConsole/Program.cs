using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace RestConsole
{
    internal class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args) => BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace();

        private static IServiceProvider InitizializeServices()
        {
            if (_serviceProvider != null)
                return _serviceProvider;

            var services = new ServiceCollection();

            services.AddSingleton<MainWindow>();
            services.AddSingleton<MainWindowViewModel>();

            services.AddScoped<InitialDocumentSettings>();
            services.AddScoped<DocumentEditor>();
            services.AddScoped<DocumentEditorViewModel>();
            services.AddScoped<Editor.CommandMargin>();
            services.AddScoped<Core.CommandRunner>();
            services.AddScoped<Core.IHttpResponseHandler>(sp => sp.GetRequiredService<DocumentEditorViewModel>());
            services.AddHttpClient<Core.CommandRunner>();


            return _serviceProvider = services.BuildServiceProvider();
        }

        private static IServiceProvider? _serviceProvider;

        public static IServiceProvider Services => InitizializeServices();

    }
}
