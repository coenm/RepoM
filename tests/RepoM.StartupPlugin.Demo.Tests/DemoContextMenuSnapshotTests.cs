namespace RepoM.StartupPlugin.Demo.Tests;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using FakeItEasy;
using Meziantou.Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RepoM.ActionMenu.Core;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.Api;
using RepoM.Api.Common;
using RepoM.Api.Git;
using RepoM.Api.IO;
using RepoM.Api.QuickFilter;
using RepoM.Core.Plugin;
using RepoM.Core.Plugin.Common;
using RepoM.Core.Plugin.RepositoryActions;
using RepoM.Core.Plugin.RepositoryActions.Commands;
using RepoM.Core.Plugin.RepositoryFiltering;
using RepoM.Core.Plugin.RepositoryFiltering.Clause;
using RepoM.Core.Repositories;
using RepoM.Core.Repositories.Favorite;
using RepoM.Core.Repositories.Monitoring;
using RepoM.Core.Repositories.Store;
using RepoM.StartupPlugin.Demo;
using RepoM.StartupPlugin.Demo.Tests.TestFramework;
using RepoM.UI;
using RepoM.UI.Controls;
using RepoM.UI.Plugins;
using RepoM.UI.RepositoryActions;
using RepoM.UI.RepositoryFiltering;
using RepoM.UI.RepositoryOrdering;
using RepoM.UI.Services;
using RepoM.UI.ViewModels;
using SimpleInjector;
using VerifyTests;
using Xunit;

[DisableParallelization]
public class DemoContextMenuSnapshotTests
{
    private static readonly Brush _videoBackground = CreateFrozenBrush(Color.FromRgb(0x2D, 0x31, 0x38));
    private static readonly Geometry _cursor = CreateCursor();

    // FluentWPF caches thread-bound chrome for AcrylicWindow, so every MainWindow must be created on the same STA thread.
    // That is why the screenshots and the video are produced by a single test.
    [WpfFact]
    public async Task DemoMenu_ShouldMatchSnapshots()
    {
        // WPF permits only one Application per test process; shutting it down also disables later resource loads.
        _ = Application.Current ?? new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        using var alignment = new LeftOpeningMenus();
        // Menus highlight the item under the real mouse pointer, so keep it away from RepoM.
        Rect workArea = SystemParameters.WorkArea;
        SetCursorPos((int)workArea.Left, (int)workArea.Top);
        var failures = new List<Exception>();
        var repositoryImages = new Dictionary<string, BitmapSource>();
        foreach (string menuView in new[] { "Repository", "Git", "Checkout" })
        {
            foreach (string theme in new[] { "Dark", "Light" })
            {
                await CollectFailureAsync(failures, $"Snapshot {menuView}.{theme}", async () =>
                {
                    BitmapSource image = await CaptureScreenshotAsync(theme, menuView);
                    if (menuView == "Repository")
                    {
                        repositoryImages.Add(theme, image);
                    }

                    await VerifyImageAsync("png", EncodePng(image), $"{menuView}.{theme}");
                });
            }
        }

        if (repositoryImages.TryGetValue("Dark", out BitmapSource? dark) &&
            repositoryImages.TryGetValue("Light", out BitmapSource? light))
        {
            await CollectFailureAsync(failures, "Snapshot Repository.DarkLight", async () =>
            {
                var layers = new[]
                {
                    new Layer(dark, new Rect(0, 0, dark.PixelWidth, dark.PixelHeight)),
                    new Layer(light, new Rect(dark.PixelWidth, 0, light.PixelWidth, light.PixelHeight)),
                };
                BitmapSource image = Compose(layers, GetBounds(layers), background: null, cursor: null);
                await VerifyImageAsync("png", EncodePng(image), "Repository.DarkLight");
            });
        }

        await CollectFailureAsync(failures, "Video.Dark", () => VerifyVideoAsync("Dark"));

        if (failures.Count > 0)
        {
            throw new AggregateException(failures);
        }
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool SetCursorPos(int x, int y);

    private static async Task CollectFailureAsync(List<Exception> failures, string name, Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (Exception exception)
        {
            failures.Add(new InvalidOperationException($"{name} failed.", exception));
        }
    }

    private static async Task<BitmapSource> CaptureScreenshotAsync(string theme, string menuView)
    {
        await using var session = await DemoSession.StartAsync(theme);
        session.List.SelectedItem = session.Repository;
        await session.OpenContextMenuAsync();
        if (menuView is "Git" or "Checkout")
        {
            await session.OpenSubmenuAsync(session.ContextMenu.Items, "Git", "Checkout");
        }

        if (menuView == "Checkout")
        {
            await session.OpenSubmenuAsync(GetMenuItem(session.ContextMenu.Items, "Git").Items, "Checkout", "main");
        }

        await session.SettleAsync();
        IReadOnlyList<Layer> layers = await session.CaptureStableLayersAsync(expectContextMenu: true);
        return Compose(layers, GetBounds(layers), background: null, cursor: null);
    }

    // Right click on RepoM, then walk through Git > Checkout, as a user would.
    private static async Task VerifyVideoAsync(string theme)
    {
        await using var session = await DemoSession.StartAsync(theme);
        session.List.SelectedItem = null;
        await session.SettleAsync();
        IReadOnlyList<Layer> idle = await session.CaptureStableLayersAsync(expectContextMenu: false);
        Point click = session.ClickPoint;
        Point start = new(click.X + 120, click.Y - 220);

        session.List.SelectedItem = session.Repository;
        await session.OpenContextMenuAsync();
        await session.SettleAsync();
        IReadOnlyList<Layer> menu = await session.CaptureStableLayersAsync(expectContextMenu: true);
        AcrylicMenuItem gitItem = GetMenuItem(session.ContextMenu.Items, "Git");
        Point git = session.CenterOf(gitItem);

        await session.OpenSubmenuAsync(session.ContextMenu.Items, "Git", "Checkout");
        await session.SettleAsync();
        IReadOnlyList<Layer> gitOpen = await session.CaptureStableLayersAsync(expectContextMenu: true);
        Point checkout = session.CenterOf(GetMenuItem(gitItem.Items, "Checkout"));

        AcrylicMenuItem checkoutItem = await session.OpenSubmenuAsync(gitItem.Items, "Checkout", "main");
        await session.SettleAsync();
        IReadOnlyList<Layer> checkoutOpen = await session.CaptureStableLayersAsync(expectContextMenu: true);
        Point main = session.CenterOf(GetMenuItem(checkoutItem.Items, "main"));

        var frames = new List<(IReadOnlyList<Layer> Layers, Point Cursor, ushort Delay)> { (idle, start, 80) };
        AddMove(frames, idle, start, click);
        frames.Add((menu, click, 90));
        AddMove(frames, menu, click, git);
        frames.Add((gitOpen, git, 90));
        AddMove(frames, gitOpen, git, checkout);
        frames.Add((checkoutOpen, checkout, 90));
        AddMove(frames, checkoutOpen, checkout, main);
        frames.Add((checkoutOpen, main, 250));

        Rect canvas = GetBounds(frames.SelectMany(frame => frame.Layers).ToList());
        var gifFrames = new List<(BitmapSource Image, Int32Rect Region, ushort DelayCentiseconds)>();
        byte[]? previous = null;
        foreach (var (layers, cursor, delay) in frames)
        {
            RenderTargetBitmap image = Compose(layers, canvas, _videoBackground, cursor);
            int stride = image.PixelWidth * 4;
            byte[] pixels = new byte[stride * image.PixelHeight];
            image.CopyPixels(pixels, stride, 0);
            Int32Rect changed = previous == null
                ? new Int32Rect(0, 0, image.PixelWidth, image.PixelHeight)
                : GetChangedRegion(previous, pixels, image.PixelWidth, image.PixelHeight);
            previous = pixels;
            gifFrames.Add((image, changed, delay));
        }

        byte[] gif = AnimatedGifWriter.Write((int)canvas.Width, (int)canvas.Height, gifFrames);
        await VerifyImageAsync("gif", gif, $"Video.{theme}");
    }

    private static Int32Rect GetChangedRegion(byte[] previous, byte[] current, int width, int height)
    {
        int left = width, top = height, right = -1, bottom = -1;
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                int offset = ((y * width) + x) * 4;
                if (BitConverter.ToInt32(previous, offset) == BitConverter.ToInt32(current, offset))
                {
                    continue;
                }

                left = Math.Min(left, x);
                top = Math.Min(top, y);
                right = Math.Max(right, x);
                bottom = Math.Max(bottom, y);
            }
        }

        // A GIF frame cannot be empty, so an unchanged frame still writes a single pixel.
        return right < 0
            ? new Int32Rect(0, 0, 1, 1)
            : new Int32Rect(left, top, right - left + 1, bottom - top + 1);
    }

    private static void AddMove(
        List<(IReadOnlyList<Layer> Layers, Point Cursor, ushort Delay)> frames,
        IReadOnlyList<Layer> layers,
        Point from,
        Point to)
    {
        const int steps = 6;
        for (var step = 1; step <= steps; step++)
        {
            double t = (double)step / steps;
            double eased = t * t * (3 - (2 * t));
            frames.Add((layers, new Point(from.X + ((to.X - from.X) * eased), from.Y + ((to.Y - from.Y) * eased)), 4));
        }
    }

    private static async Task VerifyImageAsync(
        string extension,
        byte[] bytes,
        string snapshotName,
        [CallerFilePath] string sourceFile = "")
    {
        using var verifier = new InnerVerifier(
            Path.GetDirectoryName(sourceFile)!,
            $"{nameof(DemoContextMenuSnapshotTests)}.{snapshotName}");
        using var stream = new MemoryStream(bytes);
        await verifier.Verify(new Target(extension, stream, snapshotName));
    }

    private static byte[] EncodePng(BitmapSource image)
    {
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(image));
        using var stream = new MemoryStream();
        encoder.Save(stream);
        return stream.ToArray();
    }

    private static Rect GetBounds(IReadOnlyList<Layer> layers) =>
        layers.Select(layer => layer.Bounds).Aggregate(Rect.Union);

    // Popups are separate native windows, so each layer is rendered on its own and placed at its screen offset.
    private static RenderTargetBitmap Compose(IReadOnlyList<Layer> layers, Rect canvas, Brush? background, Point? cursor)
    {
        var composition = new DrawingVisual();
        using (DrawingContext context = composition.RenderOpen())
        {
            if (background != null)
            {
                context.DrawRectangle(background, null, new Rect(0, 0, canvas.Width, canvas.Height));
            }

            foreach (var (image, bounds) in layers)
            {
                context.DrawImage(image, new Rect(bounds.X - canvas.X, bounds.Y - canvas.Y, bounds.Width, bounds.Height));
            }

            if (cursor is Point position)
            {
                context.PushTransform(new TranslateTransform(
                    Math.Round(position.X - canvas.X), Math.Round(position.Y - canvas.Y)));
                context.DrawGeometry(Brushes.White, new Pen(Brushes.Black, 1), _cursor);
                context.Pop();
            }
        }

        var bitmap = new RenderTargetBitmap((int)canvas.Width, (int)canvas.Height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(composition);
        bitmap.Freeze();
        return bitmap;
    }

    private static Geometry CreateCursor()
    {
        Geometry geometry = Geometry.Parse("M0,0 L0,17 L4,13 L7,20 L9.5,19 L6.5,12 L12,12 Z");
        geometry.Freeze();
        return geometry;
    }

    private static SolidColorBrush CreateFrozenBrush(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }

    private static AcrylicMenuItem GetMenuItem(ItemCollection items, string header)
    {
        return items.OfType<AcrylicMenuItem>().Single(item => Equals(item.Header, header));
    }

    private static async Task WaitUntilAsync(Func<bool> condition, string failureMessage)
    {
        for (var attempt = 0; attempt < 120; attempt++)
        {
            await Dispatcher.CurrentDispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
            if (condition())
            {
                return;
            }

            await Task.Delay(25);
        }

        throw new TimeoutException(failureMessage);
    }

    private readonly record struct Layer(BitmapSource Image, Rect Bounds);

    /// <summary>The real RepoM main window running the demo profile, plus the steps the screenshots and video share.</summary>
    private sealed class DemoSession : IAsyncDisposable
    {
        private readonly DirectoryInfo _directory;
        private readonly RepositoryStore _store;
        private readonly Container _container;
        private readonly List<Visual> _openedSubmenus = [];
        private MainWindow? _window;
        private RepositoryList _repositoryList = null!;

        private DemoSession(DirectoryInfo directory, RepositoryStore store, Container container)
        {
            _directory = directory;
            _store = store;
            _container = container;
        }

        public MainWindow Window => _window!;

        public ListBox List { get; private set; } = null!;

        public RepositoryViewModel Repository { get; private set; } = null!;

        public ListBoxItem Row { get; private set; } = null!;

        public AcrylicContextMenu ContextMenu { get; private set; } = null!;

        private const double MenuAnchorX = 180;

        // A fixed spot inside the RepoM row, so screenshots and video are deterministic.
        public Point ClickPoint => ToWindow(Row, new Point(MenuAnchorX, Row.ActualHeight / 2));

        public static async Task<DemoSession> StartAsync(string theme)
        {
            var store = new RepositoryStore();
            var monitoring = new RepositoryMonitoringStateService();
            var session = new DemoSession(
                Directory.CreateTempSubdirectory("RepoM-demo-menu-"), store, CreateContainer(store, monitoring));
            try
            {
                await session.InitializeAsync(theme, monitoring);
                return session;
            }
            catch
            {
                await session.DisposeAsync();
                throw;
            }
        }

        public async Task OpenContextMenuAsync()
        {
            var viewModel = (RepositoryListViewModel)_repositoryList.DataContext;
            var entries = await viewModel.CreateContextMenuEntriesAsync(CancellationToken.None);
            ContextMenu.Items.Clear();
            _repositoryList.PopulateMenuItems(ContextMenu.Items, entries);
            ContextMenu.Resources.MergedDictionaries.Add(Window.Resources);
            ContextMenu.Placement = PlacementMode.Custom;
            ContextMenu.PlacementTarget = Row;
            ContextMenu.CustomPopupPlacementCallback = static (popupSize, targetSize, _) =>
            [
                new CustomPopupPlacement(
                    new Point(MenuAnchorX - popupSize.Width, (targetSize.Height / 2) - popupSize.Height),
                    PopupPrimaryAxis.None),
            ];
            // These captures open the menu programmatically; don't let mouse input dismiss it.
            ContextMenu.StaysOpen = true;
            ContextMenu.IsOpen = true;
            await WaitUntilAsync(
                () => ContextMenu.IsOpen && PresentationSource.FromVisual(ContextMenu) != null &&
                    ContextMenu.Items.OfType<AcrylicMenuItem>().Any(),
                "The repository context menu did not open.");
        }

        public async Task<AcrylicMenuItem> OpenSubmenuAsync(ItemCollection items, string header, string expectedChild)
        {
            AcrylicMenuItem item = GetMenuItem(items, header);
            item.ApplyTemplate();
            var popup = (Popup)item.Template.FindName("PART_Popup", item);
            popup.PopupAnimation = PopupAnimation.None;
            item.IsSubmenuOpen = true;
            await WaitUntilAsync(
                () => item.Items.OfType<AcrylicMenuItem>().Any(child => Equals(child.Header, expectedChild)),
                $"The {header} submenu did not load.");
            _openedSubmenus.Add(popup.Child);
            return item;
        }

        public async Task SettleAsync()
        {
            Window.UpdateLayout();
            ContextMenu.UpdateLayout();
            // The root context menu popup uses the system menu animation; let it finish before rendering.
            await Task.Delay(500);
            await Window.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
        }

        // Transitions can still be running right after startup, so wait until two consecutive captures are identical.
        public async Task<IReadOnlyList<Layer>> CaptureStableLayersAsync(bool expectContextMenu)
        {
            IReadOnlyList<Layer> previous = CaptureLayers(expectContextMenu);
            for (var attempt = 0; attempt < 40; attempt++)
            {
                await Task.Delay(100);
                await Window.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
                IReadOnlyList<Layer> current = CaptureLayers(expectContextMenu);
                if (current.Count == previous.Count && current.Zip(previous).All(pair => HasSamePixels(pair.First, pair.Second)))
                {
                    return current;
                }

                previous = current;
            }

            throw new TimeoutException("The RepoM window did not stop changing.");
        }

        private static bool HasSamePixels(Layer left, Layer right)
        {
            if (left.Bounds != right.Bounds)
            {
                return false;
            }

            int stride = left.Image.PixelWidth * 4;
            var leftPixels = new byte[stride * left.Image.PixelHeight];
            var rightPixels = new byte[leftPixels.Length];
            left.Image.CopyPixels(leftPixels, stride, 0);
            right.Image.CopyPixels(rightPixels, stride, 0);
            return leftPixels.AsSpan().SequenceEqual(rightPixels);
        }

        private IReadOnlyList<Layer> CaptureLayers(bool expectContextMenu)
        {
            if (ContextMenu.IsOpen != expectContextMenu)
            {
                throw new InvalidOperationException(expectContextMenu
                    ? "The repository context menu closed before the capture finished."
                    : "The repository context menu was unexpectedly open during the idle capture.");
            }

            var layers = new List<Visual> { Window };
            if (expectContextMenu)
            {
                layers.Add(ContextMenu);
                layers.AddRange(_openedSubmenus);
            }

            var captures = layers.Select(visual =>
            {
                var root = (FrameworkElement)(PresentationSource.FromVisual(visual)?.RootVisual
                    ?? throw new InvalidOperationException("The popup is not displayed."));
                Point topLeft = ToWindow(root, new Point(0, 0));
                var bounds = new Rect(
                    Math.Round(topLeft.X),
                    Math.Round(topLeft.Y),
                    Math.Ceiling(root.ActualWidth),
                    Math.Ceiling(root.ActualHeight));
                var bitmap = new RenderTargetBitmap((int)bounds.Width, (int)bounds.Height, 96, 96, PixelFormats.Pbgra32);
                bitmap.Render(root);
                bitmap.Freeze();
                return new Layer(bitmap, bounds);
            }).ToList();

            if (expectContextMenu)
            {
                Rect menu = captures[1].Bounds;
                Point anchor = ClickPoint;
                if (menu.Right > anchor.X + 1 || menu.Bottom > anchor.Y + 1)
                {
                    throw new InvalidOperationException($"The context menu ({menu}) did not open above and left of the click point ({anchor}).");
                }
            }

            return captures;
        }

        public Point CenterOf(FrameworkElement element) =>
            ToWindow(element, new Point(element.ActualWidth * 0.4, element.ActualHeight / 2));

        public ValueTask DisposeAsync()
        {
            if (ContextMenu != null)
            {
                ContextMenu.IsOpen = false;
            }

            _window?.Shutdown();
            _container.Dispose();
            _store.Dispose();
            _directory.Delete(recursive: true);
            return ValueTask.CompletedTask;
        }

        private Point ToWindow(Visual visual, Point point)
        {
            Matrix fromDevice = PresentationSource.FromVisual(Window)!.CompositionTarget!.TransformFromDevice;
            Point origin = fromDevice.Transform(Window.PointToScreen(new Point(0, 0)));
            Point screen = fromDevice.Transform(visual.PointToScreen(point));
            return new Point(screen.X - origin.X, screen.Y - origin.Y);
        }

        private async Task InitializeAsync(string theme, RepositoryMonitoringStateService monitoring)
        {
            var files = new FileSystem();
            var paths = new AppDataPathProvider(new AppDataPathConfig { AppSettingsPath = _directory.FullName }, files);
            await DemoProfile.PrepareAsync(files, paths);
            _container.RegisterInstance<IFileSystem>(files);
            RepoM.ActionMenu.Core.Bootstrapper.RegisterServices(_container);
            await new DemoPackage().RegisterServicesAsync(_container, A.Fake<IPackageConfiguration>());

            var monitor = _container.GetInstance<IRepositoryMonitorService>();
            await monitor.StartAsync();
            var menuFactory = new UserMenuActionMenuFactory(
                files, paths, _container.GetInstance<IUserInterfaceActionMenuFactory>());
            _window = CreateMainWindow(theme, _container, monitor, _store, monitoring, paths, files, menuFactory);

            _window.SetReady();
            _window.Height = 560;
            Rect workArea = SystemParameters.WorkArea;
            _window.Left = workArea.Right - _window.Width;
            _window.Top = workArea.Bottom - _window.Height;
            _window.Show();

            _repositoryList = (RepositoryList)_window.FindName("repositoryList");
            List = (ListBox)_repositoryList.FindName("lstRepositories");
            ContextMenu = (AcrylicContextMenu)_repositoryList.FindName("mnuRepositoryContext");
            await WaitUntilAsync(
                () => List.Items.Count == _store.Count,
                "The demo repositories were not shown in the main window.");
            Repository = List.Items.Cast<RepositoryViewModel>().Single(item => item.Name == "RepoM");
            List.ScrollIntoView(Repository);
            await WaitUntilAsync(
                () => List.ItemContainerGenerator.ContainerFromItem(Repository) is ListBoxItem,
                "The RepoM repository row was not rendered.");
            Row = (ListBoxItem)List.ItemContainerGenerator.ContainerFromItem(Repository);
        }
    }

    private static MainWindow CreateMainWindow(
        string theme,
        Container container,
        IRepositoryMonitorService monitor,
        RepositoryStore store,
        RepositoryMonitoringStateService monitoring,
        IAppDataPathProvider paths,
        IFileSystem files,
        IUserMenuActionMenuFactory menuFactory)
    {
        var settings = A.Fake<IAppSettingsService>();
        A.CallTo(() => settings.Theme).Returns(theme == "Light" ? AppTheme.Light : AppTheme.Dark);
        A.CallTo(() => settings.EnableAnimations).Returns(false);
        A.CallTo(() => settings.ContextMenuLoadingIndicatorDelayMilliseconds).Returns(-1);
        A.CallTo(() => settings.MenuPrefetchHoverDelayMilliseconds).Returns(-1);
        var quickFilters = A.Fake<IQuickFilterService>();
        A.CallTo(() => quickFilters.GetAll()).Returns([]);
        A.CallTo(() => quickFilters.FindByQuery(A<IQuery>._)).Returns(null);

        var window = new MainWindow(
            monitor,
            store,
            A.Fake<IFavoriteService>(),
            monitoring,
            monitoring,
            A.Fake<IRepositoryIgnoreStore>(),
            settings,
            A.Fake<ITranslationService>(),
            paths,
            files,
            new ActionExecutor(container, NullLogger.Instance),
            CreateOrdering(),
            A.Fake<IThreadDispatcher>(),
            CreateFiltering(),
            quickFilters,
            [A.Fake<INamedQueryParser>()],
            A.Fake<IModuleManager>(),
            NullLogger.Instance,
            menuFactory,
            container.GetInstance<IApplicationPresentation>(),
            A.Fake<IRepositoryMatcher>(),
            new ApplicationUpdate());
        window.Resources.MergedDictionaries.Add(new ResourceDictionary
        {
            Source = new Uri("/RepoM.UI;component/i18n/en-us.xaml", UriKind.Relative),
        });
        return window;
    }

    private static Container CreateContainer(RepositoryStore store, RepositoryMonitoringStateService monitoring)
    {
        var container = new Container();
        var production = new ProductionServices();
        container.RegisterInstance(production);
        container.RegisterInstance(production.Monitor);
        container.RegisterInstance(production.Favorites);
        container.RegisterInstance(production.GitExecutor);
        container.RegisterInstance(production.ProcessExecutor);
        container.RegisterInstance<IRepositoryStore>(store);
        container.RegisterInstance<IRepositoryMonitoringService>(monitoring);
        container.RegisterInstance<IApplicationPresentation>(new DefaultPresentation());
        container.RegisterInstance<ILogger>(NullLogger.Instance);
        container.Collection.Append<IModule>(() => container.GetInstance<IRepositoryMonitorService>(), Lifestyle.Singleton);
        return container;
    }

    private static IRepositoryComparerManager CreateOrdering()
    {
        var manager = A.Fake<IRepositoryComparerManager>();
        A.CallTo(() => manager.SortObservable).Returns(Observable.Return(
            Comparer<RepositoryViewModel>.Create((left, right) => StringComparer.Ordinal.Compare(left.Name, right.Name))));
        return manager;
    }

    private static IRepositoryFilteringManager CreateFiltering()
    {
        var manager = A.Fake<IRepositoryFilteringManager>();
        A.CallTo(() => manager.CreateFilterObservable(A<IObservable<string>>._))
            .Returns(Observable.Return<Func<RepositoryViewModel, bool>>(_ => true));
        return manager;
    }

    // Windows' "menu drop alignment" decides whether popups open left or right; fix it so snapshots do not depend on the machine.
    private sealed class LeftOpeningMenus : IDisposable
    {
        private static readonly FieldInfo _value = GetField("_menuDropAlignment");
        private static readonly FieldInfo _cacheValid = GetField("_cacheValid");
        private static readonly int _slot = (int)Enum.Parse(
            typeof(SystemParameters).GetNestedType("CacheSlot", BindingFlags.NonPublic)!, "MenuDropAlignment");
        private readonly bool _original = SystemParameters.MenuDropAlignment;

        public LeftOpeningMenus() => Set(true);

        public void Dispose() => Set(_original);

        private static void Set(bool value)
        {
            var cacheValid = (BitArray)_cacheValid.GetValue(null)!;
            lock (cacheValid)
            {
                _value.SetValue(null, value);
                cacheValid[_slot] = true;
            }
        }

        private static FieldInfo GetField(string name) =>
            typeof(SystemParameters).GetField(name, BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException($"SystemParameters.{name} was not found.");
    }

    private sealed class ProductionServices
    {
        public IRepositoryMonitorService Monitor { get; } = A.Fake<IRepositoryMonitorService>();
        public IFavoriteStore Favorites { get; } = A.Fake<IFavoriteStore>();
        public ICommandExecutor<GitRepositoryCommand> GitExecutor { get; } = A.Fake<ICommandExecutor<GitRepositoryCommand>>();
        public ICommandExecutor<StartProcessRepositoryCommand> ProcessExecutor { get; } = A.Fake<ICommandExecutor<StartProcessRepositoryCommand>>();
    }

    private sealed class DefaultPresentation : IApplicationPresentation
    {
        public string WindowTitle => "RepoM";

        public bool UseTray => true;
    }
}
