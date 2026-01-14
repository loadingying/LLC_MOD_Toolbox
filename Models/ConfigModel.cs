namespace LLC_MOD_Toolbox.Models
{
    /// <summary>
    /// 通用配置设置
    /// </summary>
    public class GeneralConfig
    {
        /// <summary>
        /// 是否跳过边狱公司路径检查
        /// </summary>
        public bool skipLCBPathCheck { get; set; } = false;

        /// <summary>
        /// 边狱公司游戏安装路径
        /// </summary>
        public string? LCBPath { get; set; } = null;

        /// <summary>
        /// 是否启用国际模式（海外用户使用CDN节点）
        /// </summary>
        public bool internationalMode { get; set; } = false;
    }

    /// <summary>
    /// 安装相关配置设置
    /// </summary>
    public class InstallConfig
    {
        /// <summary>
        /// 启动时自动安装/更新模组
        /// </summary>
        public bool installWhenLaunch { get; set; } = false;

        /// <summary>
        /// 安装完成后自动关闭工具箱
        /// </summary>
        public bool afterInstallClose { get; set; } = false;
    }

    /// <summary>
    /// 公告系统配置设置
    /// </summary>
    public class AnnouncementConfig
    {
        /// <summary>
        /// 是否获取并显示公告
        /// </summary>
        public bool getAnno { get; set; } = true;

        /// <summary>
        /// 已读公告版本号
        /// </summary>
        public int annoVersion { get; set; } = 0;
    }

    /// <summary>
    /// Mirror酱镜像服务配置设置
    /// </summary>
    public class MirrorChyanConfig
    {
        /// <summary>
        /// 是否启用Mirror酱服务
        /// </summary>
        public bool enable { get; set; } = false;

        /// <summary>
        /// 是否已显示过Mirror酱介绍提示
        /// </summary>
        public bool notice { get; set; } = false;
    }

    /// <summary>
    /// 节点选择配置设置
    /// </summary>
    public class NodeConfig
    {
        /// <summary>
        /// 默认下载节点名称
        /// </summary>
        public string defaultNode { get; set; } = "";

        /// <summary>
        /// 默认API节点名称
        /// </summary>
        public string defaultApiNode { get; set; } = "";
    }

    /// <summary>
    /// 应用程序配置根对象
    /// 包含所有配置项，序列化为config.json
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// 通用设置
        /// </summary>
        public GeneralConfig general { get; set; } = new GeneralConfig();

        /// <summary>
        /// 安装设置
        /// </summary>
        public InstallConfig install { get; set; } = new InstallConfig();

        /// <summary>
        /// 公告设置
        /// </summary>
        public AnnouncementConfig announcement { get; set; } = new AnnouncementConfig();

        /// <summary>
        /// Mirror酱设置
        /// </summary>
        public MirrorChyanConfig mirrorChyan { get; set; } = new MirrorChyanConfig();

        /// <summary>
        /// 节点选择设置
        /// </summary>
        public NodeConfig nodeSelect { get; set; } = new NodeConfig();
    }
}
