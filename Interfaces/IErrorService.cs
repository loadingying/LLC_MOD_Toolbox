using LLC_MOD_Toolbox.Models;

namespace LLC_MOD_Toolbox.Interfaces
{
    /// <summary>
    /// 错误处理服务接口
    /// 提供统一的错误报告、日志记录和异常转换功能
    /// </summary>
    public interface IErrorService
    {
        /// <summary>
        /// 报告通用错误
        /// </summary>
        /// <param name="ex">异常对象</param>
        /// <param name="closeWindow">是否关闭窗口</param>
        /// <param name="advice">额外建议</param>
        void ReportError(System.Exception ex, bool closeWindow, string advice = "");

        /// <summary>
        /// 报告Mirror酱服务错误
        /// </summary>
        /// <param name="ex">Mirror酱异常对象</param>
        /// <param name="closeWindow">是否关闭窗口</param>
        void ReportMirrorChyanError(MirrorChyanException ex, bool closeWindow);

        /// <summary>
        /// 将异常转换为用户友好的错误描述
        /// </summary>
        /// <param name="ex">异常对象</param>
        /// <returns>用户友好的错误描述</returns>
        string GetExceptionMessage(System.Exception ex);
    }
}
