using System;
using System.Collections.Generic;
using System.Windows.Threading;

namespace TwitchSitePlugin
{
    /// <summary>
    /// アニメーションエモートの同期を管理するシングルトンクラス
    /// 同じエモートIDのアニメーションを全チャットで完全同期させる
    /// </summary>
    public class AnimatedEmoteManager
    {
        private static AnimatedEmoteManager _instance;
        private static readonly object _lock = new object();

        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static AnimatedEmoteManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new AnimatedEmoteManager();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// エモートIDごとの開始時刻を記録
        /// 同じエモートIDは常に同じタイミングでアニメーション開始
        /// </summary>
        private readonly Dictionary<string, DateTime> _emoteStartTimes;

        private AnimatedEmoteManager()
        {
            _emoteStartTimes = new Dictionary<string, DateTime>();
        }

        /// <summary>
        /// エモートの開始時刻を登録または取得
        /// </summary>
        /// <param name="emoteId">エモートID</param>
        /// <returns>このエモートの開始時刻</returns>
        public DateTime GetOrCreateStartTime(string emoteId)
        {
            lock (_lock)
            {
                if (!_emoteStartTimes.ContainsKey(emoteId))
                {
                    // 新しいエモートの場合、現在時刻を開始時刻として登録
                    _emoteStartTimes[emoteId] = DateTime.Now;
                }
                return _emoteStartTimes[emoteId];
            }
        }

        /// <summary>
        /// エモートの経過時間を取得（ミリ秒）
        /// </summary>
        /// <param name="emoteId">エモートID</param>
        /// <returns>開始からの経過時間（ミリ秒）</returns>
        public double GetElapsedMilliseconds(string emoteId)
        {
            var startTime = GetOrCreateStartTime(emoteId);
            return (DateTime.Now - startTime).TotalMilliseconds;
        }

        /// <summary>
        /// 古いエモートをクリーンアップ（メモリ節約）
        /// 10分以上使われていないエモートを削除
        /// </summary>
        public void CleanupOldEmotes()
        {
            lock (_lock)
            {
                var now = DateTime.Now;
                var toRemove = new List<string>();

                foreach (var kvp in _emoteStartTimes)
                {
                    if ((now - kvp.Value).TotalMinutes > 10)
                    {
                        toRemove.Add(kvp.Key);
                    }
                }

                foreach (var key in toRemove)
                {
                    _emoteStartTimes.Remove(key);
                }
            }
        }

        /// <summary>
        /// 特定のエモートをリセット（テスト用）
        /// </summary>
        public void ResetEmote(string emoteId)
        {
            lock (_lock)
            {
                if (_emoteStartTimes.ContainsKey(emoteId))
                {
                    _emoteStartTimes[emoteId] = DateTime.Now;
                }
            }
        }

        /// <summary>
        /// すべてのエモートをクリア（テスト用）
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _emoteStartTimes.Clear();
            }
        }
    }
}
