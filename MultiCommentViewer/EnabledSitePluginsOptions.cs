using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MultiCommentViewer
{
    /// <summary>
    /// 有効にするサイトプラグインの設定
    /// </summary>
    public class EnabledSitePluginsOptions
    {
        /// <summary>
        /// 有効なサイトプラグイン名のセット
        /// </summary>
        public HashSet<string> EnabledPlugins { get; set; } = new HashSet<string>();

        /// <summary>
        /// 初回設定済みかどうか
        /// </summary>
        public bool IsInitialSetupCompleted { get; set; } = false;

        /// <summary>
        /// 利用可能な全サイトプラグイン（元のSitePluginLoaderTestの順序に合わせる）
        /// </summary>
        public static readonly List<SitePluginInfo> AvailablePlugins = new List<SitePluginInfo>
        {
            new SitePluginInfo("YouTube Live", "YouTubeLiveSitePlugin.dll", "YouTubeLiveSitePlugin.YouTubeLiveSitePlugin"),
            new SitePluginInfo("OPENREC", "OpenrecSitePlugin.dll", "OpenrecSitePlugin.OpenrecSiteContext"),
            new SitePluginInfo("Mixch", "MixchSitePlugin.dll", "MixchSitePlugin.MixchSiteContext"),
            new SitePluginInfo("Twitch", "TwitchSitePlugin.dll", "TwitchSitePlugin.TwitchSiteContext"),
            new SitePluginInfo("ニコ生", "NicoSitePlugin2.dll", "NicoSitePlugin.NicoSiteContext"),
            new SitePluginInfo("ツイキャス", "TwicasSitePlugin.dll", "TwicasSitePlugin.TwicasSiteContext"),
            new SitePluginInfo("LINE LIVE", "LineLiveSitePlugin.dll", "LineLiveSitePlugin.LineLiveSiteContext"),
            new SitePluginInfo("ふわっち", "WhowatchSitePlugin.dll", "WhowatchSitePlugin.WhowatchSiteContext"),
            new SitePluginInfo("Mirrativ", "MirrativSitePlugin.dll", "MirrativSitePlugin.MirrativSiteContext"),
            new SitePluginInfo("Periscope", "PeriscopeSitePlugin.dll", "PeriscopeSitePlugin.PeriscopeSiteContext"),
            new SitePluginInfo("SHOWROOM", "ShowRoomSitePlugin.dll", "ShowRoomSitePlugin.ShowRoomSiteContext"),
            new SitePluginInfo("Mildom", "MildomSitePlugin.dll", "MildomSitePlugin.MildomSiteContext"),
            new SitePluginInfo("BIGO LIVE", "BigoSitePlugin.dll", "BigoSitePlugin.BigoSiteContext"),
        };

        /// <summary>
        /// 設定をファイルから読み込み
        /// </summary>
        public static EnabledSitePluginsOptions Load(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(filePath);
                    return JsonConvert.DeserializeObject<EnabledSitePluginsOptions>(json);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading enabled plugins: {ex.Message}");
            }
            return new EnabledSitePluginsOptions();
        }

        /// <summary>
        /// 設定をファイルに保存
        /// </summary>
        public void Save(string filePath)
        {
            System.Diagnostics.Debug.WriteLine($"=== Saving EnabledSitePluginsOptions to: {filePath} ===");
            System.Diagnostics.Debug.WriteLine($"IsInitialSetupCompleted: {IsInitialSetupCompleted}");
            System.Diagnostics.Debug.WriteLine($"EnabledPlugins count: {EnabledPlugins?.Count ?? 0}");
            if (EnabledPlugins != null)
            {
                foreach (var plugin in EnabledPlugins)
                {
                    System.Diagnostics.Debug.WriteLine($"  - {plugin}");
                }
            }
            
            try
            {
                var json = JsonConvert.SerializeObject(this, Formatting.Indented);
                System.Diagnostics.Debug.WriteLine($"JSON to save:\n{json}");
                
                var directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    System.Diagnostics.Debug.WriteLine($"Created directory: {directory}");
                }
                
                File.WriteAllText(filePath, json);
                System.Diagnostics.Debug.WriteLine($"Saved successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving enabled plugins: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// デフォルト設定（全て無効）
        /// </summary>
        public static EnabledSitePluginsOptions CreateDefault()
        {
            return new EnabledSitePluginsOptions
            {
                EnabledPlugins = new HashSet<string>(),
                IsInitialSetupCompleted = false
            };
        }

        /// <summary>
        /// 全て有効な設定
        /// </summary>
        public static EnabledSitePluginsOptions CreateAllEnabled()
        {
            return new EnabledSitePluginsOptions
            {
                EnabledPlugins = new HashSet<string>(AvailablePlugins.Select(p => p.Name)),
                IsInitialSetupCompleted = true
            };
        }
    }

    /// <summary>
    /// サイトプラグイン情報
    /// </summary>
    public class SitePluginInfo
    {
        /// <summary>
        /// 表示名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// DLLファイル名
        /// </summary>
        public string DllFileName { get; set; }

        /// <summary>
        /// 完全修飾型名
        /// </summary>
        public string FullTypeName { get; set; }

        public SitePluginInfo(string name, string dllFileName, string fullTypeName)
        {
            Name = name;
            DllFileName = dllFileName;
            FullTypeName = fullTypeName;
        }
    }
}
