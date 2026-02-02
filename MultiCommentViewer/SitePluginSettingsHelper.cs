using System;
using System.IO;
using System.Reflection;
using System.Windows;

namespace MultiCommentViewer
{
    /// <summary>
    /// 設定画面からサイトプラグイン選択を開くためのヘルパークラス
    /// </summary>
    public static class SitePluginSettingsHelper
    {
        /// <summary>
        /// サイトプラグイン選択画面を開く
        /// </summary>
        public static void OpenSitePluginSelector()
        {
            try
            {
                var enabledPluginsPath = GetEnabledPluginsPath();
                var currentOptions = EnabledSitePluginsOptions.Load(enabledPluginsPath);

                var wizard = new SitePluginSelectorWizard(currentOptions)
                {
                    Title = "使用するサイトを変更"
                };

                var result = wizard.ShowDialog();

                if (result == true && wizard.Result != null)
                {
                    wizard.Result.Save(enabledPluginsPath);

                    // 変更を適用するには再起動が必要
                    var restartResult = MessageBox.Show(
                        "設定を適用するには、アプリケーションを再起動する必要があります。\n今すぐ再起動しますか？",
                        "再起動が必要",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Information);

                    if (restartResult == MessageBoxResult.Yes)
                    {
                        RestartApplication();
                    }
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

        private static string GetEnabledPluginsPath()
        {
            var currentDir = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;
            return Path.Combine(currentDir, "settings", "enabled_plugins.json");
        }

        private static void RestartApplication()
        {
            var exePath = Assembly.GetExecutingAssembly().Location;
            System.Diagnostics.Process.Start(exePath);
            Application.Current.Shutdown();
        }
    }
}
