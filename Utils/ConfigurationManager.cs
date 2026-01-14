using LLC_MOD_Toolbox.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.IO;
using System.Reflection;

namespace LLC_MOD_Toolbox
{
    /// <summary>
    /// 配置管理器（单例模式）
    /// 负责加载、保存和访问应用程序配置
    /// </summary>
    public sealed class ConfigurationManager
    {
        private static readonly object InstanceLock = new();
        private static readonly Dictionary<string, ConfigurationManager> Instances = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 线程安全锁
        /// </summary>
        private readonly object _lock = new();

        /// <summary>
        /// 配置文件路径
        /// </summary>
        private readonly string _configFilePath;

        /// <summary>
        /// 应用程序配置对象
        /// </summary>
        public AppSettings Settings { get; private set; } = new();

        /// <summary>
        /// 获取指定路径的配置管理器单例
        /// </summary>
        public static ConfigurationManager GetInstance(string configFile)
        {
            var normalizedPath = Path.GetFullPath(configFile);
            lock (InstanceLock)
            {
                if (!Instances.TryGetValue(normalizedPath, out var instance))
                {
                    instance = new ConfigurationManager(normalizedPath);
                    Instances[normalizedPath] = instance;
                }
                return instance;
            }
        }

        /// <summary>
        /// 初始化配置管理器
        /// </summary>
        /// <param name="configFile">配置文件完整路径</param>
        public ConfigurationManager(string configFile)
        {
            _configFilePath = configFile;
            LoadConfiguration();
        }

        /// <summary>
        /// 从文件加载配置
        /// </summary>
        private void LoadConfiguration()
        {
            lock (_lock)
            {
                try
                {
                    if (!File.Exists(_configFilePath))
                    {
                        File.WriteAllText(_configFilePath, JsonConvert.SerializeObject(new AppSettings(), Formatting.Indented));
                    }
                    var json = File.ReadAllText(_configFilePath);
                    var serializerSettings = new JsonSerializerSettings
                    {
                        ContractResolver = new IgnoreCommentsResolver(),
                        MissingMemberHandling = MissingMemberHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.Populate,
                    };
                    Settings = JsonConvert.DeserializeObject<AppSettings>(json, serializerSettings) ?? new AppSettings();
                }
                catch (FileNotFoundException)
                {
                    Console.WriteLine("主配置文件未找到，使用默认配置。");
                    Settings = new AppSettings();
                }
                catch (JsonException ex)
                {
                    throw new InvalidOperationException($"JSON解析错误: {ex.Message}", ex);
                }
            }
        }

        /// <summary>
        /// 保存配置到文件
        /// </summary>
        public void SaveConfig()
        {
            lock (_lock)
            {
                File.WriteAllText(_configFilePath, JsonConvert.SerializeObject(Settings, Formatting.Indented));
            }
        }

        /// <summary>
        /// 在同一锁内更新配置并保存
        /// </summary>
        public void UpdateConfig(System.Action<AppSettings> action)
        {
            lock (_lock)
            {
                action(Settings);
                File.WriteAllText(_configFilePath, JsonConvert.SerializeObject(Settings, Formatting.Indented));
            }
        }

        /// <summary>
        /// 获取配置节（线程安全）
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="selector">配置选择器函数</param>
        /// <returns>配置值</returns>
        public T GetConfigSection<T>(Func<AppSettings, T> selector)
        {
            lock (_lock)
            {
                return selector(Settings);
            }
        }
    }

    /// <summary>
    /// 忽略下划线开头属性的JSON解析器
    /// 继承自驼峰命名解析器，同时忽略以_开头的属性
    /// </summary>
    public class IgnoreCommentsResolver : CamelCasePropertyNamesContractResolver
    {
        /// <summary>
        /// 创建属性描述
        /// </summary>
        protected override JsonProperty CreateProperty(
            MemberInfo member,
            MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            // 忽略以_开头的属性
            property.ShouldSerialize = _ => !property.PropertyName.StartsWith("_");
            return property;
        }
    }
}
