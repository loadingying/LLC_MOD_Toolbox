using LLC_MOD_Toolbox.Interfaces;
using LLC_MOD_Toolbox.Models;
using System.Windows;

namespace LLC_MOD_Toolbox.ViewModels
{
    /// <summary>
    /// 灰度测试页面ViewModel
    /// 处理灰度测试Token验证和模式切换
    /// </summary>
    public class GreytestPageViewModel : ViewModelBase
    {
        private readonly IGreytestService _greytestService;
        private readonly IConfigService _configService;
        private readonly IDialogService _dialogService;

        private string _token = string.Empty;
        private bool _isValidating;
        private bool _isEnabled;

        /// <summary>
        /// 初始化GreytestPageViewModel
        /// </summary>
        public GreytestPageViewModel(
            IGreytestService greytestService,
            IConfigService configService,
            IDialogService dialogService)
        {
            _greytestService = greytestService ?? throw new ArgumentNullException(nameof(greytestService));
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

            // 初始化命令
            ValidateTokenCommand = new AsyncRelayCommand(
                ExecuteValidateTokenAsync,
                CanExecuteValidateToken);

            ShowTokenInfoCommand = new RelayCommand(ExecuteShowTokenInfo);

            // 初始化数据
            _isEnabled = _greytestService.IsEnabled;
            if (_isEnabled)
            {
                _token = _greytestService.CurrentToken ?? "请输入秘钥";
            }
        }

        #region 属性

        /// <summary>
        /// Token输入
        /// </summary>
        public string Token
        {
            get => _token;
            set
            {
                if (SetProperty(ref _token, value))
                {
                    ValidateTokenCommand.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// 是否正在验证
        /// </summary>
        public bool IsValidating
        {
            get => _isValidating;
            set
            {
                if (SetProperty(ref _isValidating, value))
                {
                    ValidateTokenCommand.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// 是否已启用灰度测试模式
        /// </summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        #endregion

        #region 命令

        /// <summary>
        /// 验证Token命令
        /// </summary>
        public AsyncRelayCommand ValidateTokenCommand { get; }

        /// <summary>
        /// 显示Token信息命令
        /// </summary>
        public RelayCommand ShowTokenInfoCommand { get; }

        #endregion

        #region 命令实现

        private async System.Threading.Tasks.Task ExecuteValidateTokenAsync()
        {
            try
            {
                if (_greytestService.IsEnabled)
                {
                    _dialogService.ShowMessage(
                        "灰度测试模式已开启。\n请在自动安装安装此秘钥对应版本汉化。\n若需要正常使用或更换秘钥，请重启工具箱。",
                        "提示");
                    return;
                }

                var node = _configService.AppSettings.nodeSelect.defaultApiNode;
                var apiEndpoint = GetApiEndpoint(node);

                IsValidating = true;

                var result = await _greytestService.ValidateTokenAsync(Token, apiEndpoint);

                if (!result.IsValid)
                {
                    _dialogService.ShowError(result.ErrorMessage ?? "Token验证失败");
                    return;
                }

                // 启用灰度测试模式
                var tokenInfo = await _greytestService.GetTokenInfoAsync(Token, apiEndpoint);
                if (tokenInfo != null)
                {
                    _greytestService.Enable(Token, tokenInfo.DownloadUrl);
                    IsEnabled = true;

                    var message = $"目前Token有效。\n-------------\nToken信息：\n秘钥：{Token}\n备注：{result.Note}\n-------------\n灰度测试模式已开启。\n请在自动安装安装此秘钥对应版本汉化。\n秘钥信息请勿外传。";
                    _dialogService.ShowMessage(message, "提示");
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"验证Token失败：{ex.Message}");
            }
            finally
            {
                IsValidating = false;
            }
        }

        private bool CanExecuteValidateToken()
        {
            return !IsValidating && !string.IsNullOrWhiteSpace(Token) && Token != "请输入秘钥";
        }

        private void ExecuteShowTokenInfo()
        {
            try
            {
                if (!_greytestService.IsEnabled)
                {
                    _dialogService.ShowMessage("灰度测试模式未开启。", "提示");
                    return;
                }

                var token = _greytestService.CurrentToken ?? "未知";
                var message = $"灰度测试模式已开启。\n当前Token：{token}\n请在自动安装安装此秘钥对应版本汉化。";
                _dialogService.ShowMessage(message, "提示");
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"显示Token信息失败：{ex.Message}");
            }
        }

        #endregion

        #region 私有方法

        private string GetApiEndpoint(string nodeName)
        {
            var nodeListPath = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Config",
                "NodeList.json");

            if (!System.IO.File.Exists(nodeListPath))
            {
                return "https://api.zeroasso.top/{0}";
            }

            try
            {
                var json = System.IO.File.ReadAllText(nodeListPath);
                var root = Newtonsoft.Json.JsonConvert.DeserializeObject<RootModel>(json);

                if (root?.ApiNode != null)
                {
                    var node = root.ApiNode.FirstOrDefault(n => n.Name == nodeName && n.IsDefault);
                    if (node != null)
                    {
                        return node.Endpoint;
                    }
                }

                return root?.ApiNode.FirstOrDefault()?.Endpoint ?? "https://api.zeroasso.top/{0}";
            }
            catch
            {
                return "https://api.zeroasso.top/{0}";
            }
        }

        #endregion
    }
}
