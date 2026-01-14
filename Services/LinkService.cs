using LLC_MOD_Toolbox.Interfaces;
using System.Collections.Generic;

namespace LLC_MOD_Toolbox.Services
{
    /// <summary>
    /// 链接服务实现
    /// 管理外部链接的配置和跳转
    /// </summary>
    public class LinkService : ILinkService
    {
        private readonly Dictionary<string, string> _links = new();

        /// <summary>
        /// 获取所有已配置的链接
        /// </summary>
        public Dictionary<string, string> GetLinks()
        {
            return new Dictionary<string, string>(_links);
        }

        /// <summary>
        /// 根据控件名获取URL
        /// </summary>
        public string? GetUrl(string controlName)
        {
            if (string.IsNullOrEmpty(controlName))
            {
                return null;
            }

            _links.TryGetValue(controlName, out string? url);
            return url;
        }

        /// <summary>
        /// 添加链接
        /// </summary>
        public void AddLink(string controlName, string url)
        {
            if (string.IsNullOrEmpty(controlName) || string.IsNullOrEmpty(url))
            {
                Log.logger.Warn($"尝试添加无效链接：控件名={controlName}, URL={url}");
                return;
            }

            _links[controlName] = url;
            Log.logger.Debug($"添加链接：{controlName} -> {url}");
        }

        /// <summary>
        /// 移除链接
        /// </summary>
        public bool RemoveLink(string controlName)
        {
            if (string.IsNullOrEmpty(controlName))
            {
                return false;
            }

            var removed = _links.Remove(controlName);
            if (removed)
            {
                Log.logger.Debug($"移除链接：{controlName}");
            }
            return removed;
        }

        /// <summary>
        /// 初始化默认链接
        /// </summary>
        public void InitializeDefaultLinks()
        {
            _links.Clear();

            // 添加所有默认链接
            AddLink("LinkButton1", "https://www.zeroasso.top");
            AddLink("LinkButton2", "https://space.bilibili.com/3632319835409017");
            AddLink("LinkButton3", "https://github.com/LocalizeLimbusCompany");
            AddLink("LinkButton4", "https://afdian.com/a/Limbus_zero");
            AddLink("LinkButton5", "https://paratranz.cn/projects/6860/leaderboard");
            AddLink("LinkButton6", "https://paratranz.cn");
            AddLink("LinkButton7", "https://weidian.com/?userid=1655827241");
            AddLink("LinkButton8", "https://limbuscompany.huijiwiki.com");
            AddLink("LinkButton9", "https://simpfun.cn");
            AddLink("LinkButton10", "https://mirrorchyan.com/");

            Log.logger.Info($"已初始化{_links.Count}个默认链接");
        }
    }
}
