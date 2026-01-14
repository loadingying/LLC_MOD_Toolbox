using LLC_MOD_Toolbox.Interfaces;
using System.Net.Http;
using System.Threading.Tasks;

namespace LLC_MOD_Toolbox.Services
{
    /// <summary>
    /// 彩蛋服务实现
    /// 管理应用彩蛋功能
    /// </summary>
    public class EasterEggService : IEasterEggService
    {
        private bool _isEnabled = false;

        /// <summary>
        /// 是否启用彩蛋功能
        /// </summary>
        public bool IsEnabled => _isEnabled;

        /// <summary>
        /// 彩蛋可见性状态变化事件
        /// </summary>
        public event System.EventHandler<bool>? VisibilityChanged;

        /// <summary>
        /// 启用彩蛋功能
        /// </summary>
        public void Enable()
        {
            _isEnabled = true;
            Log.logger.Info("彩蛋功能已启用");
        }

        /// <summary>
        /// 禁用彩蛋功能
        /// </summary>
        public void Disable()
        {
            _isEnabled = false;
            Log.logger.Info("彩蛋功能已禁用");
        }

        /// <summary>
        /// 设置彩蛋选项可见性
        /// </summary>
        public async Task SetOptionVisibilityAsync(bool visible)
        {
            VisibilityChanged?.Invoke(this, visible);
            Log.logger.Debug($"彩蛋选项可见性设置为：{visible}");
            await Task.CompletedTask;
        }

        /// <summary>
        /// 加载彩蛋图片
        /// </summary>
        public async Task<byte[]?> LoadEasterEggImageAsync(bool useInternationalMode)
        {
            try
            {
                string url = useInternationalMode
                    ? "https://cdn-api.zeroasso.top/v2/eepic/get_image"
                    : "https://api.zeroasso.top/v2/eepic/get_image";

                Log.logger.Info($"正在加载彩蛋图片：{url}");

                using (var client = new HttpClient())
                {
                    client.Timeout = System.TimeSpan.FromSeconds(10);
                    var bytes = await client.GetByteArrayAsync(url);
                    Log.logger.Info($"彩蛋图片加载成功，大小：{bytes.Length}字节");
                    return bytes;
                }
            }
            catch (System.Exception ex)
            {
                Log.logger.Error("加载彩蛋图片失败", ex);
                return null;
            }
        }
    }
}
