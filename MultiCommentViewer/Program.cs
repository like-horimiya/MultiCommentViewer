using CommentViewerCommon;
using Common;
using MultiCommentViewer.Test;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace MultiCommentViewer
{
    class Program
    {
        static ILogger _logger;

        // 診断用：起動時にメッセージボックスを表示
        private static void ShowDiagnosticMessage(string message)
        {
            Console.WriteLine($"[DIAGNOSTIC] {message}");
            System.Diagnostics.Debug.WriteLine($"[DIAGNOSTIC] {message}");
        }

        [STAThread]
        static void Main()
        {
            // 起動直後の診断メッセージ
            ShowDiagnosticMessage("Program.Main() started - Modified version");

            _logger = new Common.LoggerTest();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            var app = new AppNoStartupUri
            {
                ShutdownMode = ShutdownMode.OnExplicitShutdown
            };
            app.InitializeComponent();
            SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext());

            var p = new Program();
            p.ExitRequested += (sender, e) =>
            {
                app.Shutdown();
            };

            var t = p.StartAsync();
            Handle(t);
            app.Run();
        }

        static async void Handle(Task t)
        {
            try
            {
                await t;
            }
            catch (Exception ex)
            {
                ShowDiagnosticMessage($"Exception in Handle: {ex.Message}");
                Debug.WriteLine(ex.Message);
                _logger.LogException(ex);
            }
        }

        private string GetOptionsPath()
        {
            var currentDir = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;
            return Path.Combine(currentDir, "settings", "options.txt");
        }

        private string GetEnabledPluginsPath()
        {
            var currentDir = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;
            var path = Path.Combine(currentDir, "settings", "enabled_plugins.json");
            ShowDiagnosticMessage($"GetEnabledPluginsPath: {path}");
            return path;
        }

        public async Task StartAsync()
        {
            ShowDiagnosticMessage("StartAsync() called");

            var io = new Test.IOTest();
            const string SettingsDirName = "settings";
            if (!Directory.Exists(SettingsDirName))
            {
                Directory.CreateDirectory(SettingsDirName);
                ShowDiagnosticMessage("Created settings directory");
            }

            var options = new DynamicOptionsTest();
            try
            {
                var s = io.ReadFile(GetOptionsPath());
                options.Deserialize(s);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }

            // サイトプラグイン設定を読み込み
            var enabledPluginsPath = GetEnabledPluginsPath();
            EnabledSitePluginsOptions enabledPluginsOptions;

            ShowDiagnosticMessage($"Checking file: {enabledPluginsPath}");
            ShowDiagnosticMessage($"File exists: {File.Exists(enabledPluginsPath)}");

            if (!File.Exists(enabledPluginsPath))
            {
                ShowDiagnosticMessage("Settings file NOT found. Showing wizard...");

                // ウィザードを表示
                var wizard = new SitePluginSelectorWizard()
                {
                    Title = "接続するサイトを選択（初回起動）"
                };

                var result = wizard.ShowDialog();

                if (result == true && wizard.Result != null)
                {
                    enabledPluginsOptions = wizard.Result;

                    // settingsフォルダを確実に作成
                    var settingsDir = Path.GetDirectoryName(enabledPluginsPath);
                    if (!Directory.Exists(settingsDir))
                    {
                        Directory.CreateDirectory(settingsDir);
                        ShowDiagnosticMessage($"Created directory: {settingsDir}");
                    }

                    enabledPluginsOptions.Save(enabledPluginsPath);
                    ShowDiagnosticMessage($"Wizard completed. Saved to: {enabledPluginsPath}");
                    ShowDiagnosticMessage($"Selected {enabledPluginsOptions.EnabledPlugins.Count} sites");
                }
                else
                {
                    ShowDiagnosticMessage("Wizard cancelled. Loading all sites.");
                    enabledPluginsOptions = EnabledSitePluginsOptions.CreateAllEnabled();

                    var settingsDir = Path.GetDirectoryName(enabledPluginsPath);
                    if (!Directory.Exists(settingsDir))
                    {
                        Directory.CreateDirectory(settingsDir);
                    }
                    enabledPluginsOptions.Save(enabledPluginsPath);
                }
            }
            else
            {
                ShowDiagnosticMessage("Settings file found. Loading...");
                enabledPluginsOptions = EnabledSitePluginsOptions.Load(enabledPluginsPath);

                ShowDiagnosticMessage($"IsInitialSetupCompleted: {enabledPluginsOptions.IsInitialSetupCompleted}");
                ShowDiagnosticMessage($"EnabledPlugins count: {enabledPluginsOptions.EnabledPlugins?.Count ?? 0}");

                // 設定が不正な場合は全サイトを有効にする
                if (enabledPluginsOptions.EnabledPlugins == null || enabledPluginsOptions.EnabledPlugins.Count == 0)
                {
                    ShowDiagnosticMessage("WARNING: EnabledPlugins is empty. Loading all sites as fallback.");
                    enabledPluginsOptions = EnabledSitePluginsOptions.CreateAllEnabled();
                }
            }

            ShowDiagnosticMessage($"Creating SitePluginLoaderTest with {enabledPluginsOptions.EnabledPlugins.Count} enabled plugins");

            // SitePluginLoaderに設定を渡す
            ISitePluginLoader sitePluginLoader = new Test.SitePluginLoaderTest(enabledPluginsOptions);
            IBrowserLoader browserLoader = new BrowserLoader(_logger);

            var viewModel = new MainViewModel(io, _logger, options, sitePluginLoader, browserLoader);
            viewModel.CloseRequested += ViewModel_CloseRequested;

            void windowClosed(object sender, EventArgs e)
            {
                viewModel.RequestClose();

                try
                {
                    var s = options.Serialize();
                    io.WriteFile(GetOptionsPath(), s);
                }
                catch (Exception ex)
                {
                    _logger.LogException(ex);
                }
            }

            await viewModel.InitializeAsync();

            var mainForm = new MainWindow();
            mainForm.Closed += windowClosed;
            mainForm.DataContext = viewModel;
            mainForm.Show();

            ShowDiagnosticMessage("MainWindow shown");
        }

        public event EventHandler<EventArgs> ExitRequested;
        void ViewModel_CloseRequested(object sender, EventArgs e)
        {
            OnExitRequested(EventArgs.Empty);
        }

        protected virtual void OnExitRequested(EventArgs e)
        {
            ExitRequested?.Invoke(this, e);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;

            try
            {
                _logger.LogException(ex, "UnhandledException");
                var s = _logger.GetExceptions();
                using (var sw = new System.IO.StreamWriter("error.txt", true))
                {
                    sw.WriteLine(s);
                }
            }
            catch { }
        }
    }
}