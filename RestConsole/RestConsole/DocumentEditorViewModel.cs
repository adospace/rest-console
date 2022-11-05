using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using RestConsole.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RestConsole;

public partial class DocumentEditorViewModel : ObservableObject, IHttpResponseHandler
{
    private readonly IServiceProvider _serviceProvider;

    public DocumentEditorViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        var initialDocumentSettings = serviceProvider.GetRequiredService<InitialDocumentSettings>();
        _fileName = initialDocumentSettings.FileName;
        _title = initialDocumentSettings.Title ?? throw new InvalidOperationException();
    }

    [ObservableProperty] private string? _fileName;

    [ObservableProperty] private bool _isDirty;

    [ObservableProperty]
    private string _source = string.Empty;

    [ObservableProperty]
    private string _output = string.Empty;

    [ObservableProperty]
    private CommandViewModel[] _commands = Array.Empty<CommandViewModel>();

    [ObservableProperty]
    private string _title;

    public DocumentEditor View => _serviceProvider.GetRequiredService<DocumentEditor>();

    partial void OnSourceChanged(string value)
    {
        Commands = CommandParser.Parse(Source)
            .Select(_ => new CommandViewModel(_))
            .ToArray();
    }

    public async Task HandleResponse(HttpResponseMessage message)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"Request URL: {message.RequestMessage?.RequestUri}");
        sb.AppendLine($"Request Method: {message.RequestMessage?.Method}");
        sb.AppendLine($"Status Code: {message.StatusCode}");
        sb.AppendLine($"Reason Phrase: {message.ReasonPhrase}");

        sb.AppendLine();
        sb.AppendLine("Response Headers");
        foreach (var header in message.Headers)
        {
            sb.AppendLine($"{header.Key}: {string.Join(";", header.Value)}");
        }
        foreach (var header in message.Content.Headers)
        {
            sb.AppendLine($"{header.Key}: {string.Join(";", header.Value)}");
        }

        sb.AppendLine();
        sb.AppendLine("Request Headers");
        if (message.RequestMessage!= null)
        {
            foreach (var header in message.RequestMessage.Headers)
            {
                sb.AppendLine($"{header.Key}: {string.Join(";", header.Value)}");
            }

            if (message.RequestMessage.Content != null)
            {
                foreach (var header in message.RequestMessage.Content.Headers)
                {
                    sb.AppendLine($"{header.Key}: {string.Join(";", header.Value)}");
                }
            }
        }

        sb.AppendLine();
        sb.AppendLine("Request Payload:");
        if (message.RequestMessage?.Content is HttpContent requestContent)
        {
            sb.AppendLine(await requestContent.ReadAsStringAsync());
        }

        sb.AppendLine();
        sb.AppendLine("Response Response Content:");
        sb.AppendLine(await message.Content.ReadAsStringAsync());

        Output = sb.ToString();
    }
}

public partial class CommandViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isBusy;

    public CommandViewModel(Command command)
    {
        Command = command;
    }

    public Command Command { get; }   
}
