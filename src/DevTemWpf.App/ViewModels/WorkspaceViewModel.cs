using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevTemWpf.App.Services;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Abstractions.Controls;

namespace DevTemWpf.App.ViewModels;

/// <summary>
/// Document-workspace state for the Tabbed shell (1.4.0). Documents are
/// route-keyed pages shared with every other shell (no shell-specific page
/// rewrites): opening resolves the same page type the rail would navigate to.
/// Each open carries a generation token — completions from a superseded open
/// (rapid switching, cancellation, disposal) are ignored, so no stale async
/// work ever touches a document. Lifecycle hooks follow tab semantics:
/// <c>OnNavigatedToAsync</c> runs once when a document opens,
/// <c>OnNavigatedFromAsync</c> once when it closes; switching tabs only
/// changes selection (dormant tabs keep state — that is the point of tabs).
/// State persists to <see cref="ShellSettings.Workspace"/>; malformed/stale
/// state falls back to a single home document and never fails startup.
/// </summary>
public sealed partial class WorkspaceViewModel : ObservableObject, INavigationAware
{
    /// <summary>An open document tab: stable route identity + live page.</summary>
    public sealed record Document(string RouteKey, string Title, FrameworkElement Page);

    private readonly ISettingsService _settings;
    private readonly Func<Type, object> _pageFactory;
    private int _generation;
    private CancellationTokenSource? _openCts;
    private readonly List<string> _history = [];
    private int _historyIndex = -1;
    private bool _navigatingHistory;

    public WorkspaceViewModel(ISettingsService settings, Func<Type, object> pageFactory)
    {
        _settings = settings;
        _pageFactory = pageFactory;
        Documents = new ObservableCollection<Document>();
        RestorePersisted();
    }

    /// <summary>Alternate composition entry: resolve pages from DI.</summary>
    public WorkspaceViewModel(ISettingsService settings, IServiceProvider services)
        : this(settings, t => services.GetRequiredService(t))
    {
    }

    public ObservableCollection<Document> Documents { get; }

    [ObservableProperty]
    private Document? selectedDocument;

    [ObservableProperty]
    private double detailWidth = 280;

    [ObservableProperty]
    private bool isDetailOpen = true;

    public bool CanGoBack => _historyIndex > 0;

    public bool CanGoForward => _historyIndex >= 0 && _historyIndex < _history.Count - 1;

    public Task OnNavigatedToAsync() => Task.CompletedTask;

    public Task OnNavigatedFromAsync()
    {
        _openCts?.Cancel();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Opens the document for <paramref name="routeKey"/> (or focuses it when
    /// already open). Unknown keys fall back to home so stale persisted state
    /// (e.g. a preset without that page) can never break the workspace.
    /// </summary>
    public async Task OpenDocumentAsync(string routeKey, CancellationToken cancellationToken = default)
    {
        var route = NavigationRoutes.Find(routeKey) ?? NavigationRoutes.Find("home")!;
        var existing = Documents.FirstOrDefault(d => d.RouteKey == route.Key);
        if (existing is not null)
        {
            SelectDocument(existing);
            return;
        }

        var generation = ++_generation;
        _openCts?.Cancel();
        _openCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var token = _openCts.Token;

        FrameworkElement page;
        try
        {
            page = (FrameworkElement)_pageFactory(route.PageType);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Unresolvable page (preset without that file): fall back to home
            // instead of breaking the workspace.
            if (!string.Equals(route.Key, "home", StringComparison.Ordinal))
            {
                await OpenDocumentAsync("home", cancellationToken).ConfigureAwait(false);
            }

            return;
        }

        var aware = TryGetNavigationAware(page);
        if (aware is not null)
        {
            try
            {
                await aware.OnNavigatedToAsync().WaitAsync(token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                return;
            }
        }

        if (generation != _generation || token.IsCancellationRequested)
        {
            // Superseded by a newer open (rapid switching): never surface a
            // stale document.
            return;
        }

        var document = new Document(route.Key, route.Title(), page);
        Documents.Add(document);
        PushHistory(route.Key);
        SelectDocument(document);
        await PersistAsync().ConfigureAwait(false);
    }

    /// <summary>Closes a document; selection moves to a neighbor.</summary>
    public async Task CloseDocumentAsync(Document document)
    {
        var index = Documents.IndexOf(document);
        if (index < 0)
        {
            return;
        }

        _openCts?.Cancel();
        var page = document.Page;
        Documents.RemoveAt(index);
        if (ReferenceEquals(SelectedDocument, document))
        {
            SelectedDocument = Documents.Count == 0
                ? null
                : Documents[Math.Min(index, Documents.Count - 1)];
        }

        try
        {
            await (TryGetNavigationAware(page)?.OnNavigatedFromAsync() ?? Task.CompletedTask).ConfigureAwait(false);
        }
        catch
        {
            // Close must never fail because a page hook threw.
        }

        await PersistAsync().ConfigureAwait(false);
    }

    [RelayCommand]
    private async Task CloseSelectedAsync()
    {
        if (SelectedDocument is not null)
        {
            await CloseDocumentAsync(SelectedDocument).ConfigureAwait(false);
        }
    }

    /// <summary>Keyboard alternative to drag-reorder (T06 ships the drag).</summary>
    [RelayCommand]
    private async Task MoveSelectedLeftAsync()
    {
        if (SelectedDocument is not null)
        {
            await MoveAsync(SelectedDocument, -1).ConfigureAwait(false);
        }
    }

    /// <summary>Keyboard alternative to drag-reorder (T06 ships the drag).</summary>
    [RelayCommand]
    private async Task MoveSelectedRightAsync()
    {
        if (SelectedDocument is not null)
        {
            await MoveAsync(SelectedDocument, 1).ConfigureAwait(false);
        }
    }

    private async Task MoveAsync(Document document, int delta)
    {
        var from = Documents.IndexOf(document);
        var to = from + delta;
        if (from < 0 || to < 0 || to >= Documents.Count)
        {
            return;
        }

        Documents.Move(from, to);
        await PersistAsync().ConfigureAwait(false);
    }

    [RelayCommand]
    private void GoBack()
    {
        if (!CanGoBack)
        {
            return;
        }

        _navigatingHistory = true;
        try
        {
            SelectByKey(_history[--_historyIndex]);
        }
        finally
        {
            _navigatingHistory = false;
        }

        OnPropertyChanged(nameof(CanGoBack));
        OnPropertyChanged(nameof(CanGoForward));
    }

    [RelayCommand]
    private void GoForward()
    {
        if (!CanGoForward)
        {
            return;
        }

        _navigatingHistory = true;
        try
        {
            SelectByKey(_history[++_historyIndex]);
        }
        finally
        {
            _navigatingHistory = false;
        }

        OnPropertyChanged(nameof(CanGoBack));
        OnPropertyChanged(nameof(CanGoForward));
    }

    /// <summary>Persists workspace state (also called by the detail splitter).</summary>
    public async Task PersistAsync()
    {
        var state = new WorkspaceState
        {
            OpenDocuments = Documents.Select(d => d.RouteKey).ToArray(),
            SelectedDocument = SelectedDocument?.RouteKey,
            DetailWidth = DetailWidth,
            IsDetailOpen = IsDetailOpen,
        };
        await _settings.UpdateAsync(s => s with { Workspace = state }).ConfigureAwait(false);
    }

    /// <summary>Resolves the navigation-aware VM behind a page, if any.</summary>
    public static INavigationAware? TryGetNavigationAware(object page)
        => page.GetType().GetProperty("ViewModel")?.GetValue(page) as INavigationAware;

    partial void OnSelectedDocumentChanged(Document? value)
    {
        if (value is null)
        {
            return;
        }

        if (!_navigatingHistory)
        {
            PushHistory(value.RouteKey);
        }

        _ = PersistAsync();
    }

    partial void OnDetailWidthChanged(double value) => _ = PersistAsync();

    partial void OnIsDetailOpenChanged(bool value) => _ = PersistAsync();

    private void SelectDocument(Document document)
    {
        SelectedDocument = document;
        OnPropertyChanged(nameof(CanGoBack));
        OnPropertyChanged(nameof(CanGoForward));
    }

    private void SelectByKey(string key)
    {
        var document = Documents.FirstOrDefault(d => d.RouteKey == key);
        if (document is not null)
        {
            SelectedDocument = document;
        }
    }

    private void PushHistory(string key)
    {
        if (_history.Count > 0 && _history[^1] == key)
        {
            return;
        }

        // Truncate forward history on a new branch, then cap.
        if (_historyIndex < _history.Count - 1)
        {
            _history.RemoveRange(_historyIndex + 1, _history.Count - _historyIndex - 1);
        }

        _history.Add(key);
        if (_history.Count > 50)
        {
            _history.RemoveAt(0);
        }

        _historyIndex = _history.Count - 1;
    }

    private void RestorePersisted()
    {
        var state = _settings.Current.Workspace;
        var keys = state.OpenDocuments
            .Where(k => NavigationRoutes.Find(k) is not null)
            .Distinct(StringComparer.Ordinal)
            .ToList();
        if (keys.Count == 0)
        {
            keys.Add("home");
        }

        DetailWidth = state.DetailWidth is > 80 and < 1200 ? state.DetailWidth : 280;
        IsDetailOpen = state.IsDetailOpen;

        foreach (var key in keys)
        {
            var route = NavigationRoutes.Find(key)!;
            FrameworkElement page;
            try
            {
                page = (FrameworkElement)_pageFactory(route.PageType);
            }
            catch
            {
                continue;
            }

            var document = new Document(key, route.Title(), page);
            Documents.Add(document);
            _history.Add(key);
        }

        _historyIndex = _history.Count - 1;
        SelectedDocument = Documents.FirstOrDefault(d => d.RouteKey == state.SelectedDocument)
            ?? Documents.FirstOrDefault();
    }
}
