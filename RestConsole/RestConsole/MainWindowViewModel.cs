using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Microsoft.Extensions.DependencyInjection;

namespace RestConsole;

public partial class MainWindowViewModel : ObservableObject
{
    public MainWindowViewModel()
    {
    }

    private readonly ObservableCollection<DocumentEditorViewModel> _documents = new();

    public ObservableCollection<DocumentEditorViewModel> Documents
    {
        get => _documents;
    }

    [RelayCommand]
    public void CreateNewDocument()
    {
        var documentScope = Program.Services.CreateScope();
        var initialDocumentSettings = documentScope.ServiceProvider.GetRequiredService<InitialDocumentSettings>();
        
        initialDocumentSettings.Title = "Document " + (_documents.Count + 1);

        var documentView = documentScope.ServiceProvider.GetRequiredService<DocumentEditor>();
        _documents.Add(documentView.ViewModel);
    }

    [RelayCommand]
    public void OpenDocument(string fileName)
    {
        var documentScope = Program.Services.CreateScope();
        var documentView = documentScope.ServiceProvider.GetRequiredService<DocumentEditor>();
        var initialDocumentSettings = documentScope.ServiceProvider.GetRequiredService<InitialDocumentSettings>();

        initialDocumentSettings.FileName = fileName;


        _documents.Add(documentView.ViewModel);
    }

    [RelayCommand]
    public void Exit()
    {
        (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Shutdown();
    }

}