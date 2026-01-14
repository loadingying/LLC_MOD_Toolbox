using LLC_MOD_Toolbox.Models;

namespace LLC_MOD_Toolbox.Interfaces
{
    /// <summary>
    /// 配置服务接口
    /// 提供配置的读取、保存和管理功能
    /// </summary>
    public interface IConfigService
    {
        /// <summary>
        /// 应用程序配置对象
        /// </summary>
        AppSettings AppSettings { get; }

        /// <summary>
        /// 获取配置值
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="selector">配置选择器</param>
        /// <returns>配置值</returns>
        T Get<T>(System.Func<AppSettings, T> selector);

        /// <summary>
        /// 设置配置值
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="action">配置设置动作</param>
        void Set<T>(System.Action<AppSettings> action);

        /// <summary>
        /// 保存配置到文件
        /// </summary>
        void Save();

        /// <summary>
        /// 在同一锁内更新并保存配置
        /// </summary>
        void UpdateAndSave(System.Action<AppSettings> action);

        /// <summary>
        /// 重新加载配置文件
        /// </summary>
        void Reload();
    }
}
