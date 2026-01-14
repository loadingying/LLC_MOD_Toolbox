using LLC_MOD_Toolbox.Interfaces;
using LLC_MOD_Toolbox.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace LLC_MOD_Toolbox.Services
{
    /// <summary>
    /// Loading文本服务
    /// 管理启动页面的Loading文本轮播
    /// </summary>
    public class LoadingTextService : ILoadingTextService
    {
        private readonly IConfigService _configService;
        private readonly OfficialApiClient _officialApiClient;
        private readonly Random _random = new Random();
        private List<string> _loadingTexts = new List<string>();
        private string? _loadingDate;

        /// <summary>
        /// 初始化LoadingTextService
        /// </summary>
        public LoadingTextService(
            IConfigService configService,
            OfficialApiClient officialApiClient)
        {
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));
            _officialApiClient = officialApiClient ?? throw new ArgumentNullException(nameof(officialApiClient));
        }

        /// <summary>
        /// 获取Loading文本列表
        /// </summary>
        public List<string> LoadingTexts => _loadingTexts;

        /// <summary>
        /// 获取Loading文本更新日期
        /// </summary>
        public string? LoadingDate => _loadingDate;

        /// <summary>
        /// 获取随机Loading文本
        /// </summary>
        public string GetRandomText()
        {
            if (_loadingTexts.Count == 0)
            {
                return "加载中...";
            }

            int index = _random.Next(_loadingTexts.Count);
            return _loadingTexts[index];
        }

        /// <summary>
        /// 获取权重随机Loading文本
        /// 模拟MainWindow中的权重逻辑
        /// </summary>
        public string GetWeightedRandomText()
        {
            if (_loadingTexts.Count == 0)
            {
                return "出现这个文本绝不是因为出了什么问题...";
            }

            int choice = _random.Next(0, 100);

            // 25%概率返回第二个文本
            if (choice < 25 && _loadingTexts.Count > 1)
            {
                return _loadingTexts[1];
            }
            // 10%概率返回第一个文本 (25-35)
            else if (choice < 35 && _loadingTexts.Count > 0)
            {
                return _loadingTexts[0];
            }
            // 65%概率返回随机文本
            else
            {
                int index = _random.Next(0, _loadingTexts.Count);
                return _loadingTexts[index];
            }
        }

        /// <summary>
        /// 从API加载Loading文本
        /// </summary>
        public async System.Threading.Tasks.Task LoadFromApiAsync(string apiEndpoint, System.Threading.CancellationToken cancellationToken = default)
        {
            try
            {
                var texts = await _officialApiClient.GetLoadingTextAsync(apiEndpoint, cancellationToken);

                if (texts != null && texts.Count > 0)
                {
                    _loadingTexts = texts;
                    _loadingDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                }
            }
            catch
            {
                // 如果API加载失败，使用默认文本
                _loadingTexts = GetDefaultTexts();
            }
        }

        /// <summary>
        /// 从本地文件加载Loading文本
        /// </summary>
        public void LoadFromFile(string filePath)
        {
            try
            {
                if (!System.IO.File.Exists(filePath))
                {
                    _loadingTexts = GetDefaultTexts();
                    _loadingDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                    return;
                }

                var json = System.IO.File.ReadAllText(filePath);
                var root = JsonConvert.DeserializeObject<RootModel>(json);

                if (root?.LoadingText != null && root.LoadingText.Count > 0)
                {
                    _loadingTexts = root.LoadingText;
                    _loadingDate = root.LoadingDate;
                }
                else
                {
                    _loadingTexts = GetDefaultTexts();
                    _loadingDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                }
            }
            catch
            {
                _loadingTexts = GetDefaultTexts();
                _loadingDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            }
        }

        /// <summary>
        /// 保存Loading文本到文件
        /// </summary>
        public void SaveToFile(string filePath)
        {
            try
            {
                var root = new RootModel
                {
                    LoadingText = _loadingTexts,
                    LoadingDate = _loadingDate ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm")
                };

                var json = JsonConvert.SerializeObject(root, Formatting.Indented);
                System.IO.File.WriteAllText(filePath, json);

                Log.logger.Info($"Loading文本已保存到：{filePath}");
            }
            catch (Exception ex)
            {
                Log.logger.Error("保存Loading文本失败", ex);
            }
        }

        /// <summary>
        /// 检查是否需要更新Loading文本（超过14天）
        /// </summary>
        public bool NeedsUpdate()
        {
            if (string.IsNullOrEmpty(_loadingDate))
            {
                return true;
            }

            if (DateTime.TryParseExact(_loadingDate, "yyyy-MM-dd HH:mm", null, DateTimeStyles.None, out DateTime parsedDate))
            {
                DateTime today = DateTime.Now;
                TimeSpan difference = today - parsedDate;
                return Math.Abs(difference.TotalDays) >= 14;
            }

            return false;
        }

        /// <summary>
        /// 下载新的Loading文本
        /// </summary>
        public async System.Threading.Tasks.Task<bool> DownloadNewTextsAsync(bool internationalMode)
        {
            try
            {
                string apiUrl = internationalMode
                    ? "https://cdn-api.zeroasso.top/v2/loading/get_loading"
                    : "https://api.zeroasso.top/v2/loading/get_loading";

                using var httpClient = new System.Net.Http.HttpClient();
                var response = await httpClient.GetStringAsync(apiUrl);

                if (string.IsNullOrEmpty(response))
                {
                    return false;
                }

                var newTexts = JArray.Parse(response);
                _loadingTexts = newTexts.ToObject<List<string>>() ?? new List<string>();
                _loadingDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

                Log.logger.Info($"成功下载{_loadingTexts.Count}条Loading文本");
                return true;
            }
            catch (Exception ex)
            {
                Log.logger.Error("下载Loading文本失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 获取默认Loading文本
        /// </summary>
        private List<string> GetDefaultTexts()
        {
            return new List<string>
            {
                "人无语言则茫然无依",
                "正在连接零协会服务器...",
                "正在检查汉化版本...",
                "正在为您加载工具箱...",
                "边狱公司，您的第二人格",
                "请稍候，即将开始..."
            };
        }
    }
}
