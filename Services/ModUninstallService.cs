using LLC_MOD_Toolbox;
using LLC_MOD_Toolbox.Interfaces;

namespace LLC_MOD_Toolbox.Services
{
    /// <summary>
    /// 模组卸载服务实现
    /// 处理模组及其相关文件的卸载逻辑
    /// </summary>
    public class ModUninstallService : IModUninstallService
    {
        /// <summary>
        /// 卸载所有模组组件
        /// </summary>
        public async System.Threading.Tasks.Task UninstallAllAsync(string limbusCompanyDir)
        {
            try
            {
                Log.logger.Info("开始卸载所有模组组件");

                DeleteLanguagePack(limbusCompanyDir);
                DeleteBepInEx(limbusCompanyDir);
                DeleteMelonLoader(limbusCompanyDir);

                Log.logger.Info("卸载完成");
            }
            catch (Exception ex)
            {
                Log.logger.Error("卸载过程中出现错误", ex);
                throw;
            }
        }

        /// <summary>
        /// 删除语言包
        /// </summary>
        public void DeleteLanguagePack(string limbusCompanyDir)
        {
            Log.logger.Info("删除语言包");

            string langDir = System.IO.Path.Combine(limbusCompanyDir, "LimbusCompany_Data", "Lang", "LLC_zh-CN");
            DeleteDirectory(langDir);

            // 注意：ChangeLCBLangConfig需要保留在MainWindow中，因为它涉及UI状态
        }

        /// <summary>
        /// 删除BepInEx版本汉化补丁
        /// </summary>
        public void DeleteBepInEx(string limbusCompanyDir)
        {
            Log.logger.Info("删除BepInEx模组");

            DeleteDirectory(System.IO.Path.Combine(limbusCompanyDir, "BepInEx"));
            DeleteDirectory(System.IO.Path.Combine(limbusCompanyDir, "dotnet"));
            DeleteFile(System.IO.Path.Combine(limbusCompanyDir, "doorstop_config.ini"));
            DeleteFile(System.IO.Path.Combine(limbusCompanyDir, "Latest(框架日志).log"));
            DeleteFile(System.IO.Path.Combine(limbusCompanyDir, "Player(游戏日志).log"));
            DeleteFile(System.IO.Path.Combine(limbusCompanyDir, "winhttp.dll"));
            DeleteFile(System.IO.Path.Combine(limbusCompanyDir, "winhttp.dll.disabled"));
            DeleteFile(System.IO.Path.Combine(limbusCompanyDir, "changelog.txt"));
            DeleteFile(System.IO.Path.Combine(limbusCompanyDir, "BepInEx-IL2CPP-x64.7z"));
            DeleteFile(System.IO.Path.Combine(limbusCompanyDir, "LimbusLocalize_BIE.7z"));
            DeleteFile(System.IO.Path.Combine(limbusCompanyDir, "tmpchinese_BIE.7z"));
        }

        /// <summary>
        /// 删除MelonLoader模组
        /// </summary>
        public void DeleteMelonLoader(string limbusCompanyDir)
        {
            Log.logger.Info("删除MelonLoader模组");

            DeleteDirectory(System.IO.Path.Combine(limbusCompanyDir, "MelonLoader"));
            DeleteDirectory(System.IO.Path.Combine(limbusCompanyDir, "Mods"));
            DeleteDirectory(System.IO.Path.Combine(limbusCompanyDir, "Plugins"));
            DeleteDirectory(System.IO.Path.Combine(limbusCompanyDir, "UserData"));
            DeleteDirectory(System.IO.Path.Combine(limbusCompanyDir, "UserLibs"));
            DeleteFile(System.IO.Path.Combine(limbusCompanyDir, "dobby.dll"));
            DeleteFile(System.IO.Path.Combine(limbusCompanyDir, "version.dll"));
        }

        /// <summary>
        /// 删除目录
        /// </summary>
        private void DeleteDirectory(string path)
        {
            if (System.IO.Directory.Exists(path))
            {
                Log.logger.Info($"删除目录：{path}");
                System.IO.Directory.Delete(path, true);
            }
            else
            {
                Log.logger.Info($"目录不存在：{path}");
            }
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        private void DeleteFile(string path)
        {
            if (System.IO.File.Exists(path))
            {
                Log.logger.Info($"删除文件：{path}");
                System.IO.File.Delete(path);
            }
            else
            {
                Log.logger.Info($"文件不存在：{path}");
            }
        }
    }
}
