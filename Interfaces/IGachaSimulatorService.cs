namespace LLC_MOD_Toolbox.Interfaces
{
    /// <summary>
    /// 抽卡模拟器服务接口
    /// 处理抽卡逻辑、人格管理、概率计算等
    /// </summary>
    public interface IGachaSimulatorService
    {
        /// <summary>
        /// 初始化抽卡模拟器
        /// </summary>
        /// <param name="gachaDataUrl">抽卡数据URL</param>
        System.Threading.Tasks.Task<bool> InitializeAsync(string gachaDataUrl);

        /// <summary>
        /// 是否已初始化
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// 执行抽卡（10连）
        /// </summary>
        /// <returns>抽到的人格列表</returns>
        System.Threading.Tasks.Task<List<GachaResult>> Pull10Async();

        /// <summary>
        /// 格式化人格显示文本
        /// </summary>
        /// <param name="personal">人格信息</param>
        /// <returns>格式化后的文本</returns>
        string FormatPersonalText(GachaResult personal);

        /// <summary>
        /// 获取抽卡统计
        /// </summary>
        /// <param name="personals">人格列表</param>
        /// <returns>各星级数量（1星、2星、3星）</returns>
        int[] GetPersonalStats(List<GachaResult> personals);
    }

    /// <summary>
    /// 抽卡结果
    /// </summary>
    public class GachaResult
    {
        /// <summary>
        /// 人格名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 稀有度（1-3）
        /// </summary>
        public int Unique { get; set; }

        /// <summary>
        /// 格式化显示文本
        /// </summary>
        public string FormattedText { get; set; } = string.Empty;

        /// <summary>
        /// 前景色（十六进制）
        /// </summary>
        public string Color { get; set; } = "#000000";
    }
}
