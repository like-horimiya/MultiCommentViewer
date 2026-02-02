using System;
using System.IO;
using System.Reflection;
using System.Windows;

namespace MultiCommentViewer
{
    /// <summary>
    /// メインウィンドウからサイトプラグイン選択を開くためのヘルパー
    /// </summary>
    public static class SitePluginMenuHelper
    {
        /// <summary>
        /// サイトプラグイン選択ウィンドウを開く
        /// </summary>
        public static void OpenSitePluginSelector()
        {
            try
            {
                var enabledPluginsPath = GetEnabledPluginsPath();
                var currentOptions = EnabledSitePluginsOptions.Load(enabledPluginsPath);

                var wizard = new SitePluginSelectorWizard(currentOptions)
                {
                    Title = "接続するサイトを変更"
                };

                var result = wizard.ShowDialog();

                if (result == true && wizard.Result != null)
                {
                    // 設定を保存
                    wizard.Result.Save(enabledPluginsPath);

                    // 再起動確認
                    ShowRestartDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"エラーが発生しました: {ex.Message}",
                    "エラー",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 現在有効なサイトの一覧を表示
        /// </summary>
        public static void ShowEnabledSites()
        {
            try
            {
                var enabledPluginsPath = GetEnabledPluginsPath();
                var options = EnabledSitePluginsOptions.Load(enabledPluginsPath);

                if (!options.IsInitialSetupCompleted || options.EnabledPlugins.Count == 0)
                {
                    MessageBox.Show(
                        "すべてのサイトが有効になっています。",
                        "有効なサイト",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return;
                }

                var enabledList = string.Join("\n• ", options.EnabledPlugins);
                MessageBox.Show(
                    $"現在、以下のサイトが有効です:\n\n• {enabledList}\n\n変更するには「接続するサイトを変更」メニューから設定してください。",
                    "有効なサイト",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"エラーが発生しました: {ex.Message}",
                    "エラー",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private static string GetEnabledPluginsPath()
        {
            var currentDir = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;
            return Path.Combine(currentDir, "settings", "enabled_plugins.json");
        }

        private static void ShowRestartDialog()
        {
            var result = MessageBox.Show(
                "設定を適用するには、アプリケーションを再起動する必要があります。\n\n今すぐ再起動しますか？",
                "再起動が必要",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                RestartApplication();
            }
            else
            {
                MessageBox.Show(
                    "次回起動時に新しい設定が適用されます。",
                    "確認",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private static void RestartApplication()
        {
            try
            {
                var exePath = Assembly.GetExecutingAssembly().Location;
                System.Diagnostics.Process.Start(exePath);
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"再起動に失敗しました: {ex.Message}\n\n手動でアプリケーションを再起動してください。",
                    "エラー",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
