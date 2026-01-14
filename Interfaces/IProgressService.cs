using System.Windows.Threading;

namespace LLC_MOD_Toolbox.Interfaces
{
    /// <summary>
    /// 进度服务接口
    /// 负责管理和更新进度条
    /// </summary>
    public interface IProgressService
    {
        /// <summary>
        /// 当前进度值（0-100）
        /// </summary>
        float CurrentProgress { get; }

        /// <summary>
        /// 是否正在计时
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// 进度更新事件
        /// 参数：当前进度值（0-100）
        /// </summary>
        event EventHandler<float>? ProgressChanged;

        /// <summary>
        /// 开始进度计时
        /// </summary>
        void Start();

        /// <summary>
        /// 停止进度计时
        /// </summary>
        void Stop();

        /// <summary>
        /// 设置进度值
        /// </summary>
        /// <param name="value">进度值（0-100）</param>
        void SetProgress(float value);

        /// <summary>
        /// 增加进度值
        /// </summary>
        /// <param name="delta">增量</param>
        void AddProgress(float delta);

        /// <summary>
        /// 重置进度
        /// </summary>
        void Reset();
    }
}
