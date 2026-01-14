using LLC_MOD_Toolbox.Interfaces;
using System.Windows.Threading;

namespace LLC_MOD_Toolbox.Services
{
    /// <summary>
    /// 进度服务实现
    /// 负责管理和更新进度条
    /// </summary>
    public class ProgressService : IProgressService
    {
        private readonly DispatcherTimer _timer;
        private float _currentProgress = 0;
        private bool _isRunning = false;

        /// <summary>
        /// 进度更新事件
        /// </summary>
        public event EventHandler<float>? ProgressChanged;

        /// <summary>
        /// 当前进度值（0-100）
        /// </summary>
        public float CurrentProgress
        {
            get => _currentProgress;
            private set
            {
                if (_currentProgress != value)
                {
                    _currentProgress = value;
                    OnProgressChanged(value);
                }
            }
        }

        /// <summary>
        /// 是否正在计时
        /// </summary>
        public bool IsRunning => _isRunning;

        /// <summary>
        /// 初始化ProgressService
        /// </summary>
        public ProgressService()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(0.05) // 50ms更新一次
            };
            _timer.Tick += Timer_Tick;
        }

        /// <summary>
        /// 开始进度计时
        /// </summary>
        public void Start()
        {
            if (!_isRunning)
            {
                _isRunning = true;
                _currentProgress = 0;
                _timer.Start();
                Log.logger.Info("进度计时已启动");
            }
        }

        /// <summary>
        /// 停止进度计时
        /// </summary>
        public void Stop()
        {
            if (_isRunning)
            {
                _isRunning = false;
                _timer.Stop();
                Log.logger.Info($"进度计时已停止，最终进度：{_currentProgress}%");
            }
        }

        /// <summary>
        /// 设置进度值
        /// </summary>
        public void SetProgress(float value)
        {
            // 限制在0-100范围内
            value = Math.Max(0, Math.Min(100, value));
            CurrentProgress = value;
        }

        /// <summary>
        /// 增加进度值
        /// </summary>
        public void AddProgress(float delta)
        {
            SetProgress(_currentProgress + delta);
        }

        /// <summary>
        /// 重置进度
        /// </summary>
        public void Reset()
        {
            CurrentProgress = 0;
            Log.logger.Info("进度已重置");
        }

        /// <summary>
        /// 定时器Tick事件处理
        /// </summary>
        private void Timer_Tick(object? sender, EventArgs e)
        {
            OnProgressChanged(_currentProgress);
        }

        /// <summary>
        /// 触发进度更新事件
        /// </summary>
        protected virtual void OnProgressChanged(float progress)
        {
            ProgressChanged?.Invoke(this, progress);
        }
    }
}
