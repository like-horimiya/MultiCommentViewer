using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace MultiCommentViewer
{
    /// <summary>
    /// SitePluginSelectorWizard.xaml の相互作用ロジック
    /// </summary>
    public partial class SitePluginSelectorWizard : Window
    {
        private List<SitePluginViewModel> _sitePlugins;

        public EnabledSitePluginsOptions Result { get; private set; }

        public SitePluginSelectorWizard()
        {
            InitializeComponent();
            InitializeSiteList();
        }

        /// <summary>
        /// 既存の設定で初期化
        /// </summary>
        public SitePluginSelectorWizard(EnabledSitePluginsOptions existingOptions) : this()
        {
            if (existingOptions != null)
            {
                foreach (var site in _sitePlugins)
                {
                    site.IsEnabled = existingOptions.EnabledPlugins.Contains(site.Name);
                }
            }
        }

        private void InitializeSiteList()
        {
            _sitePlugins = EnabledSitePluginsOptions.AvailablePlugins
                .Select(p => new SitePluginViewModel { Name = p.Name, IsEnabled = false })
                .ToList();

            SiteList.ItemsSource = _sitePlugins;
        }

        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var site in _sitePlugins)
            {
                site.IsEnabled = true;
            }
        }

        private void DeselectAllButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var site in _sitePlugins)
            {
                site.IsEnabled = false;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedPlugins = _sitePlugins.Where(s => s.IsEnabled).Select(s => s.Name).ToList();

            // 最低1つは選択されているかチェック
            if (selectedPlugins.Count == 0)
            {
                MessageBox.Show(
                    "最低1つのサイトを選択してください。",
                    "警告",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            Result = new EnabledSitePluginsOptions
            {
                EnabledPlugins = new HashSet<string>(selectedPlugins),
                IsInitialSetupCompleted = true
            };

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // キャンセル時は全て有効にする
            Result = EnabledSitePluginsOptions.CreateAllEnabled();
            DialogResult = false;
            Close();
        }
    }

    /// <summary>
    /// サイトプラグインViewModel
    /// </summary>
    public class SitePluginViewModel : INotifyPropertyChanged
    {
        private string _name;
        private bool _isEnabled;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                OnPropertyChanged(nameof(IsEnabled));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
