using log4net;
using System.Reflection;

namespace LLC_MOD_Toolbox
{
    /// <summary>
    /// 日志工具类
    /// 封装log4net，提供统一的日志记录接口
    /// </summary>
    public static class Log
    {
        /// <summary>
        /// log4net日志实例
        /// 自动获取调用类的类型
        /// </summary>
        internal static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType ?? typeof(MainWindow));
    }
}
