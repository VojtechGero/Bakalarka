using BakalarkaWpf.Contracts.Services;
using BakalarkaWpf.Contracts.Views;
using BakalarkaWpf.ViewModels;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BakalarkaWpf.Services
{
    public class ApplicationHostService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly INavigationService _navigationService;
        private IShellWindow _shellWindow;

        public ApplicationHostService(IServiceProvider serviceProvider, INavigationService navigationService)
        {
            _serviceProvider = serviceProvider;
            _navigationService = navigationService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await InitializeAsync();

            await HandleActivationAsync();

            await StartupAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }

        private async Task InitializeAsync()
        {
            await Task.CompletedTask;
        }

        private async Task StartupAsync()
        {
            await Task.CompletedTask;
        }

        private async Task HandleActivationAsync()
        {
            if (App.Current.Windows.OfType<IShellWindow>().Count() == 0)
            {
                // Default activation that navigates to the apps default page
                _shellWindow = _serviceProvider.GetService(typeof(IShellWindow)) as IShellWindow;
                _navigationService.Initialize(_shellWindow.GetNavigationFrame());
                _shellWindow.ShowWindow();
                _navigationService.NavigateTo(typeof(MainViewModel).FullName);
                await Task.CompletedTask;
            }
        }
    }
}
