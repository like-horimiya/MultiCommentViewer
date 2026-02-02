using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SitePlugin;
using Common;
using System.Windows.Threading;
using System.Windows.Controls;
using SitePluginCommon;

namespace MultiCommentViewer.Test
{
    public class SitePluginLoaderTest : ISitePluginLoader
    {
        private readonly EnabledSitePluginsOptions _enabledPluginsOptions;
        Dictionary<Guid, ISiteContext> _dict = new Dictionary<Guid, ISiteContext>();

        // デフォルトコンストラクタ（既存コードとの互換性のため）
        public SitePluginLoaderTest() : this(null)
        {
            System.Diagnostics.Debug.WriteLine("!!! SitePluginLoaderTest: Default constructor called !!!");
        }

        // 新しいコンストラクタ
        public SitePluginLoaderTest(EnabledSitePluginsOptions enabledPluginsOptions)
        {
            System.Diagnostics.Debug.WriteLine("!!! SitePluginLoaderTest: Constructor called with options !!!");
            _enabledPluginsOptions = enabledPluginsOptions ?? EnabledSitePluginsOptions.CreateAllEnabled();

            // デバッグ出力: 設定内容を確認
            System.Diagnostics.Debug.WriteLine("===============================================");
            System.Diagnostics.Debug.WriteLine("=== SitePluginLoader Constructor ===");
            System.Diagnostics.Debug.WriteLine($"IsInitialSetupCompleted: {_enabledPluginsOptions.IsInitialSetupCompleted}");
            System.Diagnostics.Debug.WriteLine($"EnabledPlugins count: {_enabledPluginsOptions.EnabledPlugins?.Count ?? 0}");
            if (_enabledPluginsOptions.EnabledPlugins != null)
            {
                foreach (var plugin in _enabledPluginsOptions.EnabledPlugins)
                {
                    System.Diagnostics.Debug.WriteLine($"  - Enabled: {plugin}");
                }
            }
            System.Diagnostics.Debug.WriteLine("===============================================");
        }

        public IEnumerable<(string displayName, Guid guid)> LoadSitePlugins(ICommentOptions options, ILogger logger, IUserStoreManager userStoreManager, string userAgent)
        {
            var list = new List<ISiteContext>();

            System.Diagnostics.Debug.WriteLine("===============================================");
            System.Diagnostics.Debug.WriteLine("=== LoadSitePlugins START ===");
            System.Diagnostics.Debug.WriteLine("===============================================");

            // 各サイトプラグインを条件付きで読み込み
            if (ShouldLoadPlugin("YouTube Live"))
            {
                System.Diagnostics.Debug.WriteLine(">>> LOADING: YouTube Live");
                list.Add(new YouTubeLiveSitePlugin.Test2.YouTubeLiveSiteContext(options, new YouTubeLiveSitePlugin.Test2.YouTubeLiveServer(), logger, userStoreManager));
            }

            if (ShouldLoadPlugin("OPENREC"))
            {
                System.Diagnostics.Debug.WriteLine(">>> LOADING: OPENREC");
                list.Add(new OpenrecSitePlugin.OpenrecSiteContext(options, logger, userStoreManager));
            }

            if (ShouldLoadPlugin("Mixch"))
            {
                System.Diagnostics.Debug.WriteLine(">>> LOADING: Mixch");
                list.Add(new MixchSitePlugin.MixchSiteContext(options, logger, userStoreManager));
            }

            if (ShouldLoadPlugin("Twitch"))
            {
                System.Diagnostics.Debug.WriteLine(">>> LOADING: Twitch");
                list.Add(new TwitchSitePlugin.TwitchSiteContext(options, new TwitchSitePlugin.TwitchServer(), logger, userStoreManager));
            }

            if (ShouldLoadPlugin("ニコ生"))
            {
                System.Diagnostics.Debug.WriteLine(">>> LOADING: ニコ生");
                list.Add(new NicoSitePlugin.NicoSiteContext(options, new NicoSitePlugin.DataSource(userAgent), logger, userStoreManager));
            }

            if (ShouldLoadPlugin("ツイキャス"))
            {
                System.Diagnostics.Debug.WriteLine(">>> LOADING: ツイキャス");
                list.Add(new TwicasSitePlugin.TwicasSiteContext(options, logger, userStoreManager));
            }

            if (ShouldLoadPlugin("LINE LIVE"))
            {
                System.Diagnostics.Debug.WriteLine(">>> LOADING: LINE LIVE");
                list.Add(new LineLiveSitePlugin.LineLiveSiteContext(options, new LineLiveSitePlugin.LineLiveServer(), logger, userStoreManager));
            }

            if (ShouldLoadPlugin("ふわっち"))
            {
                System.Diagnostics.Debug.WriteLine(">>> LOADING: ふわっち");
                list.Add(new WhowatchSitePlugin.WhowatchSiteContext(options, logger, userStoreManager));
            }

            if (ShouldLoadPlugin("Mirrativ"))
            {
                System.Diagnostics.Debug.WriteLine(">>> LOADING: Mirrativ");
                list.Add(new MirrativSitePlugin.MirrativSiteContext(options, new MirrativSitePlugin.MirrativServer(), logger, userStoreManager));
            }

            if (ShouldLoadPlugin("Periscope"))
            {
                System.Diagnostics.Debug.WriteLine(">>> LOADING: Periscope");
                list.Add(new PeriscopeSitePlugin.PeriscopeSiteContext(options, new PeriscopeSitePlugin.PeriscopeServer(), logger, userStoreManager));
            }

            if (ShouldLoadPlugin("SHOWROOM"))
            {
                System.Diagnostics.Debug.WriteLine(">>> LOADING: SHOWROOM");
                list.Add(new ShowRoomSitePlugin.ShowRoomSiteContext(options, new ShowRoomSitePlugin.ShowRoomServer(), logger, userStoreManager));
            }

            if (ShouldLoadPlugin("Mildom"))
            {
                System.Diagnostics.Debug.WriteLine(">>> LOADING: Mildom");
                list.Add(new MildomSitePlugin.MildomSiteContext(options, new MildomSitePlugin.MildomServer(), logger, userStoreManager));
            }

            if (ShouldLoadPlugin("BIGO LIVE"))
            {
                System.Diagnostics.Debug.WriteLine(">>> LOADING: BIGO LIVE");
                list.Add(new BigoSitePlugin.BigoSiteContext(options, new BigoSitePlugin.BigoServer(), logger, userStoreManager));
            }

            // TestSitePluginは完全に除外（DEBUGモードでも読み込まない）
            System.Diagnostics.Debug.WriteLine(">>> TestSitePlugin: EXCLUDED (not loaded in any build configuration)");

            System.Diagnostics.Debug.WriteLine($">>> Total plugins to initialize: {list.Count}");

            foreach (var site in list)
            {
                try
                {
                    site.Init();
                    _dict.Add(site.Guid, site);
                    System.Diagnostics.Debug.WriteLine($">>> Initialized successfully: {site.DisplayName}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($">>> ERROR initializing {site.DisplayName}: {ex.Message}");
                }
            }

            System.Diagnostics.Debug.WriteLine("===============================================");
            System.Diagnostics.Debug.WriteLine($"=== LoadSitePlugins END: Total {_dict.Count} plugins loaded ===");
            System.Diagnostics.Debug.WriteLine("===============================================");

            return _dict.Select(kv => (kv.Value.DisplayName, kv.Key));
        }

        /// <summary>
        /// 指定されたプラグインを読み込むべきかチェック
        /// </summary>
        private bool ShouldLoadPlugin(string pluginName)
        {
            // 初回設定が完了していない場合は全て読み込み
            if (!_enabledPluginsOptions.IsInitialSetupCompleted)
            {
                System.Diagnostics.Debug.WriteLine($"[{pluginName}] -> Loading (initial setup not completed)");
                return true;
            }

            // EnabledPluginsがnullまたは空の場合は全て読み込み
            if (_enabledPluginsOptions.EnabledPlugins == null || _enabledPluginsOptions.EnabledPlugins.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine($"[{pluginName}] -> Loading (no plugins configured, loading all)");
                return true;
            }

            // 設定で有効になっているかチェック
            bool isEnabled = _enabledPluginsOptions.EnabledPlugins.Contains(pluginName);
            System.Diagnostics.Debug.WriteLine($"[{pluginName}] -> {(isEnabled ? "LOADING" : "SKIPPING")}");
            return isEnabled;
        }

        public void Save()
        {
            foreach (var siteContext in GetSiteContexts())
            {
                siteContext.Save();
            }
        }

        private IEnumerable<ISiteContext> GetSiteContexts()
        {
            return _dict.Select(s => s.Value);
        }

        public ISiteContext GetSiteContext(Guid guid)
        {
            return _dict[guid];
        }

        public ICommentProvider CreateCommentProvider(Guid guid)
        {
            var siteContext = GetSiteContext(guid);
            return siteContext.CreateCommentProvider();
        }

        public Guid GetValidSiteGuid(string input)
        {
            foreach (var siteContext in GetSiteContexts())
            {
                var b = siteContext.IsValidInput(input);
                if (b)
                {
                    return siteContext.Guid;
                }
            }
            return Guid.Empty;
        }

        public UserControl GetCommentPostPanel(Guid guid, ICommentProvider commentProvider)
        {
            var site = _dict[guid];
            return site.GetCommentPostPanel(commentProvider);
        }
    }
}