using LLC_MOD_Toolbox.Interfaces;

namespace LLC_MOD_Toolbox.Services
{
    /// <summary>
    /// Mirror酱服务实现
    /// 负责Mirror酱模式的管理、配置和状态查询
    /// </summary>
    public class MirrorChyanService : IMirrorChyanService
    {
        private readonly IConfigService _configService;
        private readonly MirrorChyanApiClient _apiClient;
        private bool _isEnabled = false;
        private string? _token;

        /// <summary>
        /// 初始化MirrorChyanService
        /// </summary>
        public MirrorChyanService(
            IConfigService configService,
            MirrorChyanApiClient apiClient)
        {
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        }

        /// <summary>
        /// 是否启用MirrorChyan模式
        /// </summary>
        public bool IsEnabled => _isEnabled;

        /// <summary>
        /// Mirror酱Token
        /// </summary>
        public string? Token => _token;

        /// <summary>
        /// 初始化MirrorChyan服务
        /// </summary>
        public void Initialize()
        {
            try
            {
                // 从配置中读取启用状态
                var isEnabledConfig = _configService.AppSettings.mirrorChyan?.enable ?? false;

                if (!isEnabledConfig)
                {
                    Log.logger.Info("MirrorChyan模式未启用");
                    return;
                }

                // 尝试加载Token
                var savedToken = SecureStringStorage.LoadToken();
                if (string.IsNullOrWhiteSpace(savedToken))
                {
                    Log.logger.Warn("MirrorChyan配置为启用，但未找到保存的Token");
                    // 更新配置状态
                    _configService.Set<object>(config => { config.mirrorChyan.enable = false; });
                    _configService.Save();
                    return;
                }

                // 启用MirrorChyan模式
                _token = savedToken;
                _isEnabled = true;
                Log.logger.Info("MirrorChyan模式已启用");
            }
            catch (Exception ex)
            {
                Log.logger.Error("初始化MirrorChyan服务失败", ex);
                _isEnabled = false;
                _token = null;
            }
        }

        /// <summary>
        /// 启用MirrorChyan模式
        /// </summary>
        public bool Enable(string token, bool saveToConfig = true)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    Log.logger.Warn("尝试启用MirrorChyan，但Token为空");
                    return false;
                }

                // 保存Token到安全存储
                SecureStringStorage.SaveToken(token.Trim());

                // 更新状态
                _token = token.Trim();
                _isEnabled = true;

                // 保存到配置
                if (saveToConfig)
                {
                    _configService.Set<object>(config => { config.mirrorChyan.enable = true; });
                    _configService.Save();
                }

                Log.logger.Info("MirrorChyan模式已启用");
                return true;
            }
            catch (Exception ex)
            {
                Log.logger.Error("启用MirrorChyan模式失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 禁用MirrorChyan模式
        /// </summary>
        public void Disable(bool saveToConfig = true)
        {
            try
            {
                // 清除状态
                _isEnabled = false;
                _token = null;

                // 保存到配置
                if (saveToConfig)
                {
                    _configService.Set<object>(config => { config.mirrorChyan.enable = false; });
                    _configService.Save();
                }

                Log.logger.Info("MirrorChyan模式已禁用");
            }
            catch (Exception ex)
            {
                Log.logger.Error("禁用MirrorChyan模式失败", ex);
            }
        }

        /// <summary>
        /// 验证Token是否有效
        /// </summary>
        public async System.Threading.Tasks.Task<bool> ValidateTokenAsync(
            string token,
            System.Threading.CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    return false;
                }

                var isValid = await _apiClient.ValidateTokenAsync(token, cancellationToken);
                Log.logger.Info($"Token验证结果：{(isValid ? "有效" : "无效")}");

                return isValid;
            }
            catch (Exception ex)
            {
                Log.logger.Error("验证Token失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 检查是否有已保存的Token
        /// </summary>
        public bool HasSavedToken()
        {
            return SecureStringStorage.HasSavedData();
        }

        /// <summary>
        /// 清除已保存的Token
        /// </summary>
        public void ClearSavedToken()
        {
            try
            {
                SecureStringStorage.DeleteSecretFile();
                Log.logger.Info("已清除保存的MirrorChyan Token");
            }
            catch (Exception ex)
            {
                Log.logger.Error("清除Token失败", ex);
            }
        }
    }
}
