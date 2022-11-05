using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace RestConsole.Core;

partial class CommandRunner : ObservableObject
{
    private readonly HttpClient _httpClient;
    private readonly IHttpResponseHandler _httpResponseHandler;
    [ObservableProperty]
    private bool _isBusy;

    public CommandRunner(HttpClient httpClient, IHttpResponseHandler httpResponseHandler) 
    {
        _httpClient = httpClient;
        _httpResponseHandler = httpResponseHandler;
    }

    public async void Execute(CommandViewModel command)
    {
        IsBusy = true;
        command.IsBusy = true;

        try
        {
            if (command.Command is SetupCommand setupCommand)
            {
                await Execute(setupCommand);
            }
            else if (command.Command is GetCommand getCommand)
            {
                await Execute(getCommand);
            }
        }
        finally
        {
            command.IsBusy = false;
            IsBusy = false;
        }
    }

    private Task Execute(SetupCommand setupCommand)
    {
        _httpClient.BaseAddress = new Uri(setupCommand.Path ?? throw new InvalidOperationException());

        return Task.CompletedTask;
    }

    private async Task Execute(GetCommand getCommand)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            //RequestUri = new Uri(getCommand.Path),
            //Content = new StringContent("some json", Encoding.UTF8, MediaTypeNames.Application.Json /* or "application/json" in older versions */),
        };

        if (!string.IsNullOrWhiteSpace(getCommand.Path))
        {
            request.RequestUri = new Uri(getCommand.Path, UriKind.Relative);
        }

        //if (!string.IsNullOrWhiteSpace(getCommand.Body))
        //{
        //    request.Content = new StringContent(getCommand.Body);
        //}

        var response = await _httpClient.SendAsync(request);

        await _httpResponseHandler.HandleResponse(response);
    }
}
