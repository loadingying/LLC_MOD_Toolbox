namespace LLC_MOD_Toolbox.Models
{
    /// <summary>
    /// Hash校验失败异常
    /// 当下载的文件Hash值与预期不符时抛出
    /// </summary>
    public class HashException : Exception
    {
    }

    /// <summary>
    /// Mirror酱服务异常
    /// Mirror酱API返回错误码时抛出此异常
    /// </summary>
    public class MirrorChyanException : Exception
    {
        /// <summary>
        /// Mirror酱错误码
        /// </summary>
        private int errorID;

        /// <summary>
        /// 初始化Mirror酱异常
        /// </summary>
        /// <param name="errorID">Mirror酱API返回的错误码</param>
        public MirrorChyanException(int errorID)
        {
            this.errorID = errorID;
        }

        /// <summary>
        /// 获取错误描述信息
        /// </summary>
        public override string Message
        {
            get {
                return errorID switch
                {
                    0 => "不是哥们，这不会有问题啊，这条提示绝对不可能出现，要是出现了我穿女装",
                    7001 => "您的 Mirror 酱 CDK 已经过期，请前往 Mirror 酱官网购买。",
                    7002 => "您的 Mirror 酱 CDK 无效，请确保您输入了正确的 CDK。",
                    7003 => "您的 Mirror 酱 CDK 已经达到使用上限，请隔天再试。",
                    7004 => "您的 Mirror 酱 CDK 可能为特殊 CDK，无法用于工具箱，请前往 Mirror 酱官网购买。",
                    7005 => "您的 Mirror 酱 CDK 被冻结，请前往 Mirror 酱售后群询问。",
                    _ => "在解析您的 Mirror 酱请求时发生了未知错误。"
                };
            }
        }
    }
}
