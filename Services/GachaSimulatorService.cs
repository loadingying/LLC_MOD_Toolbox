using LLC_MOD_Toolbox;
using LLC_MOD_Toolbox.Interfaces;
using Newtonsoft.Json.Linq;
using System.Text;

namespace LLC_MOD_Toolbox.Services
{
    /// <summary>
    /// 抽卡模拟器服务实现
    /// 处理抽卡逻辑、人格管理、概率计算等
    /// </summary>
    public class GachaSimulatorService : IGachaSimulatorService
    {
        private readonly IFileUtilityService _fileUtilityService;
        private readonly Random _random = new();
        private List<PersonalInfo> _personalInfos1star = new();
        private List<PersonalInfo> _personalInfos2star = new();
        private List<PersonalInfo> _personalInfos3star = new();
        private bool _isInitialized = false;
        private int _gachaCount = 0;

        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool IsInitialized => _isInitialized;

        /// <summary>
        /// 初始化GachaSimulatorService
        /// </summary>
        public GachaSimulatorService(IFileUtilityService fileUtilityService)
        {
            _fileUtilityService = fileUtilityService;
        }

        /// <summary>
        /// 初始化抽卡模拟器
        /// </summary>
        public async System.Threading.Tasks.Task<bool> InitializeAsync(string gachaDataUrl)
        {
            try
            {
                string gachaText = await _fileUtilityService.GetURLText(gachaDataUrl);

                if (string.IsNullOrEmpty(gachaText))
                {
                    Log.logger.Error("初始化抽卡模拟器失败：无法获取数据");
                    return false;
                }

                List<PersonalInfo> personalInfos = TransformTextToList(gachaText);
                Log.logger.Info($"人格数量：{personalInfos.Count}");

                _personalInfos1star = personalInfos.Where(p => p.Unique == 1).ToList();
                _personalInfos2star = personalInfos.Where(p => p.Unique == 2).ToList();
                _personalInfos3star = personalInfos.Where(p => p.Unique == 3).ToList();

                _isInitialized = true;
                Log.logger.Info("抽卡模拟器初始化完成");
                return true;
            }
            catch (Exception ex)
            {
                Log.logger.Error("初始化抽卡模拟器失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 执行抽卡（10连）
        /// </summary>
        public async System.Threading.Tasks.Task<List<GachaResult>> Pull10Async()
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("抽卡模拟器未初始化");
            }

            _gachaCount++;
            return await System.Threading.Tasks.Task.Run(() =>
            {
                List<GachaResult> results = new();

                for (int i = 0; i < 10; i++)
                {
                    GachaResult result = PullSingle(i == 9);
                    results.Add(result);
                }

                return results;
            });
        }

        /// <summary>
        /// 单次抽卡
        /// </summary>
        private GachaResult PullSingle(bool isPityPull)
        {
            int chance = _random.Next(1, 101);
            int unique;
            PersonalInfo? personal;

            // 第10发保底2星以上
            if (isPityPull)
            {
                if (chance <= 84)
                {
                    // 2星
                    unique = 2;
                    personal = SelectPersonal(_personalInfos2star) ?? SelectPersonal(_personalInfos3star);
                }
                else
                {
                    // 3星
                    unique = 3;
                    personal = SelectPersonal(_personalInfos3star);
                }
            }
            else
            {
                // 普通抽卡
                if (chance <= 84)
                {
                    // 1星
                    unique = 1;
                    personal = SelectPersonal(_personalInfos1star)
                        ?? SelectPersonal(_personalInfos2star)
                        ?? SelectPersonal(_personalInfos3star);
                }
                else if (chance <= 97)
                {
                    // 2星
                    unique = 2;
                    personal = SelectPersonal(_personalInfos2star)
                        ?? SelectPersonal(_personalInfos3star);
                }
                else
                {
                    // 3星
                    unique = 3;
                    personal = SelectPersonal(_personalInfos3star);
                }
            }

            if (personal == null)
            {
                // Fallback：创建默认人格
                personal = new PersonalInfo { Name = "未知人格", Unique = unique };
            }

            return FormatPersonalResult(personal);
        }

        /// <summary>
        /// 从列表中随机选择一个人格
        /// </summary>
        private PersonalInfo? SelectPersonal(List<PersonalInfo> list)
        {
            if (list.Count == 0)
                return null;

            int index = _random.Next(list.Count);
            return list[index];
        }

        /// <summary>
        /// 格式化人格结果
        /// </summary>
        private GachaResult FormatPersonalResult(PersonalInfo personal)
        {
            string prefix = personal.Unique switch
            {
                1 => "[★]",
                2 => "[★★]",
                3 => "[★★★]",
                _ => "[?]"
            };

            string color = personal.Unique switch
            {
                1 => "#B88345",
                2 => "#CA1400",
                3 => "#FCC404",
                _ => "#000000"
            };

            return new GachaResult
            {
                Name = personal.Name,
                Unique = personal.Unique,
                FormattedText = $"{prefix}{personal.Name}",
                Color = color
            };
        }

        /// <summary>
        /// 格式化人格显示文本
        /// </summary>
        public string FormatPersonalText(GachaResult personal)
        {
            return personal.FormattedText;
        }

        /// <summary>
        /// 获取抽卡统计
        /// </summary>
        public int[] GetPersonalStats(List<GachaResult> personals)
        {
            int[] stats = new int[3]; // 1星、2星、3星

            foreach (var personal in personals)
            {
                if (personal.Unique >= 1 && personal.Unique <= 3)
                {
                    stats[personal.Unique - 1]++;
                }
            }

            return stats;
        }

        /// <summary>
        /// 将文本转换为人格列表
        /// </summary>
        private List<PersonalInfo> TransformTextToList(string gachaText)
        {
            Log.logger.Info("开始转换文本。");

            try
            {
                var gachaObject = JObject.Parse(gachaText);
                List<PersonalInfo> personalInfoList = new();

                for (int i = 0; i < gachaObject["data"]?.Count(); i++)
                {
                    var dataItem = gachaObject["data"]?[i];
                    string characterName = BeautifyText(
                        dataItem?[0]?.Value<string>() ?? string.Empty,
                        dataItem?[1]?.Value<string>() ?? string.Empty);

                    if (!string.IsNullOrWhiteSpace(characterName))
                    {
                        PersonalInfo personalInfo = new()
                        {
                            Name = characterName,
                            Unique = dataItem?[7]?.Value<int>() ?? 1,
                        };
                        personalInfoList.Add(personalInfo);
                    }
                }

                return personalInfoList;
            }
            catch (Exception ex)
            {
                Log.logger.Error("转换人格数据失败", ex);
                return new List<PersonalInfo>();
            }
        }

        /// <summary>
        /// 美化人格名称
        /// </summary>
        private string BeautifyText(string input, string prefix)
        {
            if (input.StartsWith(prefix))
            {
                string title = input.Substring(prefix.Length);
                return $"{title} {prefix}";
            }
            else
            {
                return input;
            }
        }
    }

    /// <summary>
    /// 人格信息
    /// </summary>
    public class PersonalInfo
    {
        /// <summary>
        /// 人格名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 稀有度（1-3）
        /// </summary>
        public int Unique { get; set; }
    }
}
