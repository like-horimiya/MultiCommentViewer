using Common;
using SitePlugin;
using System;

namespace TwitchSitePlugin
{
    /// <summary>
    /// アニメーション対応のエモート画像クラス
    /// 既存のMessageImageを拡張して、同期情報を追加
    /// </summary>
    public class AnimatedMessageImage : MessageImage
    {
        /// <summary>
        /// エモートID（同期に使用）
        /// </summary>
        public string EmoteId { get; set; }

        /// <summary>
        /// アニメーションGIFかどうか
        /// </summary>
        public bool IsAnimated { get; set; }

        /// <summary>
        /// アニメーション開始時刻（同期用）
        /// </summary>
        public DateTime AnimationStartTime
        {
            get
            {
                if (IsAnimated && !string.IsNullOrEmpty(EmoteId))
                {
                    return AnimatedEmoteManager.Instance.GetOrCreateStartTime(EmoteId);
                }
                return DateTime.Now;
            }
        }

        /// <summary>
        /// 現在のアニメーション経過時間（ミリ秒）
        /// </summary>
        public double ElapsedMilliseconds
        {
            get
            {
                if (IsAnimated && !string.IsNullOrEmpty(EmoteId))
                {
                    return AnimatedEmoteManager.Instance.GetElapsedMilliseconds(EmoteId);
                }
                return 0;
            }
        }
    }
}
