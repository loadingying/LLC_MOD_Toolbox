using LLC_MOD_Toolbox.Interfaces;
using LLC_MOD_Toolbox.Models;
using System.Collections.ObjectModel;
using System.Text;

namespace LLC_MOD_Toolbox.ViewModels
{
    /// <summary>
    /// 抽卡模拟器页面ViewModel
    /// 处理抽卡逻辑和结果显示
    /// </summary>
    public class GachaSimulatorPageViewModel : ViewModelBase
    {
        private readonly IConfigService _configService;
        private readonly IDialogService _dialogService;
        private readonly Random _random = new Random();

        private ObservableCollection<GachaResult> _gachaResults;
        private bool _isGachaInProgress;
        private string _gachaButtonText;
        private int _totalPulls;
        private int _star3Count;
        private int _star2Count;
        private int _star1Count;

        /// <summary>
        /// 初始化GachaSimulatorPageViewModel
        /// </summary>
        public GachaSimulatorPageViewModel(
            IConfigService configService,
            IDialogService dialogService)
        {
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

            // 初始化命令
            GachaCommand = new AsyncRelayCommand(
                ExecuteGachaAsync,
                CanExecuteGacha);

            ClearResultsCommand = new RelayCommand(
                ExecuteClearResults);

            // 初始化数据
            _gachaResults = new ObservableCollection<GachaResult>();
            _gachaButtonText = "十连抽 (x1300)";
            _totalPulls = 0;
            _star3Count = 0;
            _star2Count = 0;
            _star1Count = 0;
        }

        #region 属性

        /// <summary>
        /// 抽卡结果列表
        /// </summary>
        public ObservableCollection<GachaResult> GachaResults
        {
            get => _gachaResults;
            set => SetProperty(ref _gachaResults, value);
        }

        /// <summary>
        /// 抽卡是否进行中
        /// </summary>
        public bool IsGachaInProgress
        {
            get => _isGachaInProgress;
            set
            {
                if (SetProperty(ref _isGachaInProgress, value))
                {
                    GachaCommand.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// 抽卡按钮文本
        /// </summary>
        public string GachaButtonText
        {
            get => _gachaButtonText;
            set => SetProperty(ref _gachaButtonText, value);
        }

        /// <summary>
        /// 总抽取次数
        /// </summary>
        public int TotalPulls
        {
            get => _totalPulls;
            set => SetProperty(ref _totalPulls, value);
        }

        /// <summary>
        /// 3星数量
        /// </summary>
        public int Star3Count
        {
            get => _star3Count;
            set => SetProperty(ref _star3Count, value);
        }

        /// <summary>
        /// 2星数量
        /// </summary>
        public int Star2Count
        {
            get => _star2Count;
            set => SetProperty(ref _star2Count, value);
        }

        /// <summary>
        /// 1星数量
        /// </summary>
        public int Star1Count
        {
            get => _star1Count;
            set => SetProperty(ref _star1Count, value);
        }

        /// <summary>
        /// 3星概率
        /// </summary>
        public string Star3Rate => TotalPulls > 0 ? $"{(Star3Count * 100.0 / TotalPulls):F2}%" : "0.00%";

        /// <summary>
        /// 2星概率
        /// </summary>
        public string Star2Rate => TotalPulls > 0 ? $"{(Star2Count * 100.0 / TotalPulls):F2}%" : "0.00%";

        /// <summary>
        /// 1星概率
        /// </summary>
        public string Star1Rate => TotalPulls > 0 ? $"{(Star1Count * 100.0 / TotalPulls):F2}%" : "0.00%";

        #endregion

        #region 命令

        /// <summary>
        /// 抽卡命令
        /// </summary>
        public AsyncRelayCommand GachaCommand { get; }

        /// <summary>
        /// 清空结果命令
        /// </summary>
        public RelayCommand ClearResultsCommand { get; }

        #endregion

        #region 命令实现

        private async System.Threading.Tasks.Task ExecuteGachaAsync()
        {
            try
            {
                IsGachaInProgress = true;

                // 模拟抽卡动画延迟
                await System.Threading.Tasks.Task.Delay(500);

                // 执行十连抽
                var results = new List<GachaResult>();
                for (int i = 0; i < 10; i++)
                {
                    results.Add(SinglePull());
                }

                // 更新结果
                foreach (var result in results)
                {
                    GachaResults.Insert(0, result);
                }

                // 只保留最近100条记录
                while (GachaResults.Count > 100)
                {
                    GachaResults.RemoveAt(GachaResults.Count - 1);
                }

                // 更新统计
                UpdateStatistics(results);

                // 通知UI更新
                OnPropertyChanged(nameof(Star3Rate));
                OnPropertyChanged(nameof(Star2Rate));
                OnPropertyChanged(nameof(Star1Rate));

                // 显示抽卡结果趣味消息
                ShowGachaResultMessage(results);
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"抽卡失败：{ex.Message}");
            }
            finally
            {
                IsGachaInProgress = false;
            }
        }

        private bool CanExecuteGacha()
        {
            return !IsGachaInProgress;
        }

        private void ExecuteClearResults()
        {
            GachaResults.Clear();
            TotalPulls = 0;
            Star3Count = 0;
            Star2Count = 0;
            Star1Count = 0;

            OnPropertyChanged(nameof(Star3Rate));
            OnPropertyChanged(nameof(Star2Rate));
            OnPropertyChanged(nameof(Star1Rate));
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 单次抽卡
        /// </summary>
        private GachaResult SinglePull()
        {
            // 模拟边狱公司的抽卡概率
            // 3星: 约2.5%
            // 2星: 约10%
            // 1星: 约87.5%
            double rand = _random.NextDouble();

            int rarity;
            string color;
            string personality;

            if (rand < 0.025) // 3星
            {
                rarity = 3;
                color = "#CA1400"; // 红色
                personality = GetRandomPersonality(3);
            }
            else if (rand < 0.125) // 2星
            {
                rarity = 2;
                color = "#FCC404"; // 金色
                personality = GetRandomPersonality(2);
            }
            else // 1星
            {
                rarity = 1;
                color = "#B88345"; // 棕色
                personality = GetRandomPersonality(1);
            }

            return new GachaResult
            {
                Rarity = rarity,
                Personality = personality,
                Color = color,
                Timestamp = DateTime.Now
            };
        }

        /// <summary>
        /// 获取随机人格
        /// </summary>
        private string GetRandomPersonality(int rarity)
        {
            // 简化版人格列表
            var personalities = new List<string>
            {
                "李箱", "浮士德", "但丁", "良秀", "默尔索",
                "鸿露", "希斯克利夫", "以实玛丽", "罗佳", "辛克莱",
                "奥提斯", "格里高尔", "卡夫卡", "堂吉诃德", "亨克尔",
                "伊索尔德", "罗兰", "天使尤里", "魔鬼尤里", "菲利普"
            };

            int index = _random.Next(personalities.Count);
            return $"边狱公司EGO::罪人 {personalities[index]}";
        }

        /// <summary>
        /// 更新统计数据
        /// </summary>
        private void UpdateStatistics(List<GachaResult> results)
        {
            foreach (var result in results)
            {
                switch (result.Rarity)
                {
                    case 3:
                        Star3Count++;
                        break;
                    case 2:
                        Star2Count++;
                        break;
                    case 1:
                        Star1Count++;
                        break;
                }
            }

            TotalPulls += results.Count;
        }

        /// <summary>
        /// 显示抽卡结果趣味消息
        /// </summary>
        private void ShowGachaResultMessage(List<GachaResult> results)
        {
            // 统计本次抽卡的3星数量
            int star3CountThisPull = results.Count(r => r.Rarity == 3);

            string[] messages;
            if (star3CountThisPull == 1)
            {
                messages = new[]
                {
                    "单黄蛋出来了，希望你瓦夜的时候也能这样。",
                    "恭喜恭喜~不知道抽了多少次了？",
                    "ALL IN！"
                };
            }
            else if (star3CountThisPull == 2)
            {
                messages = new[]
                {
                    "双黄蛋？希望你瓦夜的时候也能这样。",
                    "100碎片而已，我一点都不羡慕！",
                    "恭喜恭喜~"
                };
            }
            else if (star3CountThisPull == 3)
            {
                messages = new[]
                {
                    "真的假的三黄。。？",
                    "你平时运气也这么好？！",
                    "爽了，再来再来！"
                };
            }
            else if (star3CountThisPull >= 4)
            {
                messages = new[]
                {
                    "不可能……不可能啊？！",
                    "欧吃矛！",
                    "再抽池子就要空了！"
                };
            }
            else
            {
                messages = new[]
                {
                    "怎么样？再来一次么？",
                    "冷知识：概率真的完全真实。",
                    "你平时抽卡也这个结果吗？"
                };
            }

            // 随机选择一条消息
            string message = messages[_random.Next(messages.Length)];
            _dialogService.ShowMessage(message, "提示");
        }

        #endregion
    }

    /// <summary>
    /// 抽卡结果模型
    /// </summary>
    public class GachaResult
    {
        /// <summary>
        /// 稀有度（1-3星）
        /// </summary>
        public int Rarity { get; set; }

        /// <summary>
        /// 人格名称
        /// </summary>
        public string Personality { get; set; } = string.Empty;

        /// <summary>
        /// 显示颜色
        /// </summary>
        public string Color { get; set; } = string.Empty;

        /// <summary>
        /// 抽取时间
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// 显示文本（带星标）
        /// </summary>
        public string DisplayText
        {
            get
            {
                var stars = new string('★', Rarity);
                return $"[{stars}] {Personality}";
            }
        }
    }
}
