using System.Threading.Tasks;

namespace LLC_MOD_Toolbox.Interfaces
{
    /// <summary>
    /// 彩蛋服务接口
    /// 管理应用彩蛋功能（如隐藏页面、特殊图片等）
    /// </summary>
    public interface IEasterEggService
    {
        /// <summary>
        /// 是否启用彩蛋功能
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// 彩蛋可见性状态变化事件
        /// </summary>
        event System.EventHandler<bool>? VisibilityChanged;

        /// <summary>
        /// 启用彩蛋功能
        /// </summary>
        void Enable();

        /// <summary>
        /// 禁用彩蛋功能
        /// </summary>
        void Disable();

        /// <summary>
        /// 设置彩蛋选项可见性
        /// </summary>
        /// <param name="visible">是否可见</param>
        Task SetOptionVisibilityAsync(bool visible);

        /// <summary>
        /// 加载彩蛋图片
        /// </summary>
        /// <param name="useInternationalMode">是否使用国际模式</param>
        /// <returns>图片字节数组，失败返回null</returns>
        Task<byte[]?> LoadEasterEggImageAsync(bool useInternationalMode);
    }
}
