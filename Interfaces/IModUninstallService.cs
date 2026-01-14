namespace LLC_MOD_Toolbox.Interfaces
{
    /// <summary>
    /// 模组卸载服务接口
    /// 处理模组及其相关文件的卸载逻辑
    /// </summary>
    public interface IModUninstallService
    {
        /// <summary>
        /// 卸载所有模组组件
        /// </summary>
        /// <param name="limbusCompanyDir">Limbus Company游戏目录</param>
        /// <returns>卸载任务</returns>
        System.Threading.Tasks.Task UninstallAllAsync(string limbusCompanyDir);

        /// <summary>
        /// 删除语言包
        /// </summary>
        /// <param name="limbusCompanyDir">Limbus Company游戏目录</param>
        void DeleteLanguagePack(string limbusCompanyDir);

        /// <summary>
        /// 删除BepInEx模组
        /// </summary>
        /// <param name="limbusCompanyDir">Limbus Company游戏目录</param>
        void DeleteBepInEx(string limbusCompanyDir);

        /// <summary>
        /// 删除MelonLoader模组
        /// </summary>
        /// <param name="limbusCompanyDir">Limbus Company游戏目录</param>
        void DeleteMelonLoader(string limbusCompanyDir);
    }
}
