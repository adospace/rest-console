using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using AvaloniaEdit;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace RestConsole;

public partial class DocumentEditor : UserControl
{
    public DocumentEditor(IServiceProvider services)
    {
        DataContext = services.GetRequiredService<DocumentEditorViewModel>();

        InitializeComponent();

        Observable
            .FromEventPattern<EventArgs>(_sourceTextEditor, nameof(TextEditor.TextChanged))
            .Throttle(TimeSpan.FromMilliseconds(1000))
            .Subscribe(eventPattern => Dispatcher.UIThread.Post(()=>SourceTextEditorTextChanged(eventPattern.Sender, eventPattern.EventArgs)));

        _sourceTextEditor.TextArea.LeftMargins.Add(services.GetRequiredService<Editor.CommandMargin>());

        Observable
            .FromEventPattern<PropertyChangedEventArgs>(ViewModel, nameof(DocumentEditorViewModel.PropertyChanged))
            .Where(_ => _.EventArgs.PropertyName == nameof(DocumentEditorViewModel.Output))
            .Subscribe(eventPattern => _outputTextEditor.Text = ViewModel.Output);

    }

    public DocumentEditorViewModel ViewModel =>
        (DataContext as DocumentEditorViewModel) ?? throw new InvalidOperationException();


    private void SourceTextEditorTextChanged(object? sender, EventArgs e)
    {
        ViewModel.Source = _sourceTextEditor.Text;
    }

}