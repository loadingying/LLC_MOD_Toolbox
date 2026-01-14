using LLC_MOD_Toolbox.Interfaces;
using LLC_MOD_Toolbox.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.IO;
using System.Reflection;

namespace LLC_MOD_Toolbox.Services
{
    /// <summary>
    /// 配置服务实现
    /// 封装ConfigurationManager，提供配置的读取、保存和管理功能
    /// </summary>
    public class ConfigService : IConfigService
    {
        private readonly ConfigurationManager _configurationManager;
        private readonly object _lock = new();

        /// <summary>
        /// 初始化配置服务（用于测试）
        /// </summary>
        /// <param name="configPath">配置文件路径</param>
        public ConfigService(string configPath)
        {
            _configurationManager = ConfigurationManager.GetInstance(configPath);
        }

        /// <summary>
        /// 初始化配置服务（使用默认路径）
        /// </summary>
        public ConfigService() : this(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "config.json"))
        {
        }

        /// <summary>
        /// 应用程序配置对象
        /// </summary>
        public AppSettings AppSettings => _configurationManager.Settings;

        /// <summary>
        /// 获取配置值（线程安全）
        /// </summary>
        public T Get<T>(System.Func<AppSettings, T> selector)
        {
            lock (_lock)
            {
                return _configurationManager.GetConfigSection(selector);
            }
        }

        /// <summary>
        /// 设置配置值（线程安全）
        /// </summary>
        public void Set<T>(System.Action<AppSettings> action)
        {
            lock (_lock)
            {
                action(_configurationManager.Settings);
            }
        }

        /// <summary>
        /// 保存配置到文件
        /// </summary>
        public void Save()
        {
            lock (_lock)
            {
                _configurationManager.SaveConfig();
            }
        }

        /// <summary>
        /// 在同一锁内更新并保存配置
        /// </summary>
        public void UpdateAndSave(System.Action<AppSettings> action)
        {
            lock (_lock)
            {
                _configurationManager.UpdateConfig(action);
            }
        }

        /// <summary>
        /// 重新加载配置文件
        /// </summary>
        public void Reload()
        {
            // ConfigurationManager在每次访问时已经自动处理了加载
            // 这里我们可以触发一个配置重新验证
            lock (_lock)
            {
                // 可以添加重新加载逻辑
            }
        }
    }
}
