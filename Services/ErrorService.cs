using LLC_MOD_Toolbox;
using LLC_MOD_Toolbox.Interfaces;
using LLC_MOD_Toolbox.Models;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows;
using SevenZip;

namespace LLC_MOD_Toolbox.Services
{
    /// <summary>
    /// 错误处理服务
    /// 统一处理错误报告、日志记录和异常转换
    /// </summary>
    public class ErrorService : IErrorService
    {
        private readonly IDialogService _dialogService;

        /// <summary>
        /// 初始化ErrorService
        /// </summary>
        /// <param name="dialogService">对话框服务</param>
        public ErrorService(IDialogService dialogService)
        {
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        }

        /// <summary>
        /// 报告通用错误
        /// </summary>
        /// <param name="ex">异常对象</param>
        /// <param name="closeWindow">是否关闭窗口</param>
        /// <param name="advice">额外建议</param>
        public void ReportError(Exception ex, bool closeWindow, string advice = "")
        {
            Log.logger.Error("出现了问题：\n", ex);
            string errorMessage = GetExceptionMessage(ex);

            string message = closeWindow
                ? $"运行中出现了问题，且在这个错误发生后，工具箱将关闭。\n{advice}若要反馈，请带上链接或日志。\n反馈请勿！请勿截图此页面！\n错误分析原因：\n{errorMessage}"
                : $"运行中出现了问题。但你仍然能够使用工具箱（大概）。\n{advice}若要反馈，请带上链接或日志。\n反馈请勿！请勿截图此页面！\n——————————\n错误分析原因：\n{errorMessage}";

            _dialogService.ShowMessage(message, "错误");

            if (closeWindow)
            {
                Application.Current.Shutdown();
            }
        }

        /// <summary>
        /// 报告Mirror酱服务错误
        /// </summary>
        /// <param name="ex">Mirror酱异常对象</param>
        /// <param name="closeWindow">是否关闭窗口</param>
        public void ReportMirrorChyanError(MirrorChyanException ex, bool closeWindow)
        {
            Log.logger.Error("访问 Mirror 酱服务中出现了错误\n", ex);

            string message = closeWindow
                ? $"访问 Mirror 酱服务出现了问题，且在这个错误发生后，工具箱将关闭。\n出现该问题原因：{ex.Message}\n若要反馈，请带上链接或日志。\n反馈请勿！请勿截图此页面！"
                : $"访问 Mirror 酱服务出现了问题。但你仍然能够使用工具箱（大概）。\n出现该问题原因：{ex.Message}\n若要反馈，请带上链接或日志。\n反馈请勿！请勿截图此页面！";

            _dialogService.ShowMessage(message, "错误");

            if (closeWindow)
            {
                Application.Current.Shutdown();
            }
        }

        /// <summary>
        /// 将异常转换为用户友好的错误描述
        /// </summary>
        /// <param name="ex">异常对象</param>
        /// <returns>用户友好的错误描述</returns>
        public string GetExceptionMessage(Exception ex)
        {
            return ex switch
            {
                WebException or System.Net.Http.HttpRequestException
                or System.Net.Sockets.SocketException or System.Net.HttpListenerException =>
                    "网络链接错误，请尝试更换节点，关闭加速器或代理后再试。\n您也可以尝试在官网【常见问题】进行排查。",

                SevenZipException =>
                    "解压出现问题，大概率为网络问题。\n请尝试更换节点，关闭加速器或代理后再试。\n您也可以尝试在官网【常见问题】进行排查。",

                FileNotFoundException =>
                    "无法找到文件，可能是网络问题，也可能是边狱公司路径出现错误。\n请尝试更换节点，关闭加速器或代理后再试。\n您也可以尝试在官网【常见问题】进行排查。",

                UnauthorizedAccessException =>
                    "无权限访问文件，请尝试以管理员身份启动，也可能是你打开了边狱公司？",

                IOException =>
                    "文件访问出现问题。\n可能是文件已被边狱公司占用？\n您可以尝试关闭边狱公司。",

                HashException =>
                    "文件损坏。\n大概率为网络问题，请尝试更换节点，关闭加速器或代理后再试。\n您也可以尝试在官网【常见问题】进行排查。",

                _ =>
                    "未知错误原因，错误已记录至日志，请查看官网【常见问题】进行排查。\n如果没有解决，请尝试进行反馈。"
            };
        }
    }
}
