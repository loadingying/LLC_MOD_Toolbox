using LLC_MOD_Toolbox.Interfaces;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace LLC_MOD_Toolbox.Services
{
    /// <summary>
    /// 字体服务实现
    /// 提供字体验证、替换、还原和预览功能
    /// </summary>
    public class FontService : IFontService
    {
        private const string CHINESE_FONT_TTF = "ChineseFont.ttf";
        private const string CHINESE_FONT_OTF = "ChineseFont.otf";
        private const string BACKUP_FONT = "ChineseFont.ttf.bak";

        /// <summary>
        /// 验证字体文件是否有效
        /// </summary>
        public bool IsValidFontFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || filePath == "输入字体路径")
            {
                return false;
            }

            // 1. 检查文件扩展名
            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            if (extension != ".ttf" && extension != ".otf" && extension != ".ttc")
            {
                return false;
            }

            // 2. 检查文件是否存在并且可以访问
            if (!File.Exists(filePath))
            {
                return false;
            }

            // 3. 尝试加载字体来验证它是否真的是字体文件
            try
            {
                // 尝试加载字体，如果不是有效的字体文件，这将引发异常
                using (PrivateFontCollection fontCollection = new PrivateFontCollection())
                {
                    fontCollection.AddFontFile(filePath);
                    return fontCollection.Families.Length > 0;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取字体家族名称
        /// </summary>
        public string GetFontFamilyName(string filePath)
        {
            try
            {
                using (PrivateFontCollection fontCollection = new PrivateFontCollection())
                {
                    fontCollection.AddFontFile(filePath);
                    if (fontCollection.Families.Length > 0)
                    {
                        return fontCollection.Families[0].Name;
                    }
                }
            }
            catch
            {
                // 忽略异常，返回默认值
            }

            // 如果无法获取字体名称，返回文件名
            return Path.GetFileNameWithoutExtension(filePath);
        }

        /// <summary>
        /// 替换游戏字体
        /// </summary>
        public System.Threading.Tasks.Task<FontReplaceResult> ReplaceFontAsync(
            string fontFilePath,
            string gameDirectory,
            System.Threading.CancellationToken cancellationToken = default)
        {
            return System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    var result = new FontReplaceResult();

                    // 验证字体文件
                    if (!IsValidFontFile(fontFilePath))
                    {
                        result.Success = false;
                        result.ErrorMessage = "字体文件无效";
                        return result;
                    }

                    // 构建路径
                    var fontDir = Path.Combine(gameDirectory, "LimbusCompany_Data", "Lang", "LLC_zh-CN", "Font", "Context");
                    var oldFontTTFPath = Path.Combine(fontDir, CHINESE_FONT_TTF);
                    var oldFontOTFPath = Path.Combine(fontDir, CHINESE_FONT_OTF);
                    var backupDir = Path.Combine(gameDirectory, "LimbusCompany_Data", "Lang", "LLC_zh-CN", "BackupFont");
                    var backupFontPath = Path.Combine(backupDir, BACKUP_FONT);

                    // 检查是否已安装汉化
                    if (!File.Exists(oldFontTTFPath) && !File.Exists(backupFontPath))
                    {
                        result.Success = false;
                        result.ErrorMessage = "请先安装汉化，然后再进行字体替换";
                        return result;
                    }

                    // 备份原字体
                    if (File.Exists(oldFontTTFPath) && !File.Exists(backupFontPath))
                    {
                        Directory.CreateDirectory(backupDir);
                        File.Move(oldFontTTFPath, backupFontPath);
                        result.HasBackup = true;
                    }

                    // 删除旧字体
                    if (File.Exists(oldFontTTFPath))
                    {
                        File.Delete(oldFontTTFPath);
                    }

                    if (File.Exists(oldFontOTFPath))
                    {
                        File.Delete(oldFontOTFPath);
                    }

                    // 复制新字体
                    string extension = new FileInfo(fontFilePath).Extension;
                    string newFontPath = Path.Combine(fontDir, $"ChineseFont{extension}");
                    File.Copy(fontFilePath, newFontPath, true);

                    result.Success = true;
                    result.ReplacedFontPath = newFontPath;
                    return result;
                }
                catch (Exception ex)
                {
                    return new FontReplaceResult
                    {
                        Success = false,
                        ErrorMessage = $"字体替换失败：{ex.Message}"
                    };
                }
            }, cancellationToken);
        }

        /// <summary>
        /// 还原原始字体
        /// </summary>
        public System.Threading.Tasks.Task<FontRestoreResult> RestoreFontAsync(
            string gameDirectory,
            System.Threading.CancellationToken cancellationToken = default)
        {
            return System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    var backupDir = Path.Combine(gameDirectory, "LimbusCompany_Data", "Lang", "LLC_zh-CN", "BackupFont");
                    var backupFontPath = Path.Combine(backupDir, BACKUP_FONT);
                    var fontDir = Path.Combine(gameDirectory, "LimbusCompany_Data", "Lang", "LLC_zh-CN", "Font", "Context");
                    var oldFontTTFPath = Path.Combine(fontDir, CHINESE_FONT_TTF);
                    var oldFontOTFPath = Path.Combine(fontDir, CHINESE_FONT_OTF);

                    // 检查备份是否存在
                    if (!File.Exists(backupFontPath))
                    {
                        return new FontRestoreResult
                        {
                            Success = false,
                            ErrorMessage = "没有找到备份字体文件"
                        };
                    }

                    // 删除现有字体
                    if (File.Exists(oldFontTTFPath))
                    {
                        File.Delete(oldFontTTFPath);
                    }

                    if (File.Exists(oldFontOTFPath))
                    {
                        File.Delete(oldFontOTFPath);
                    }

                    // 还原备份
                    File.Move(backupFontPath, oldFontTTFPath);

                    return new FontRestoreResult
                    {
                        Success = true,
                        RestoredFontPath = oldFontTTFPath
                    };
                }
                catch (Exception ex)
                {
                    return new FontRestoreResult
                    {
                        Success = false,
                        ErrorMessage = $"字体还原失败：{ex.Message}"
                    };
                }
            }, cancellationToken);
        }

        /// <summary>
        /// 检查是否有字体备份
        /// </summary>
        public bool HasBackupFont(string gameDirectory)
        {
            if (string.IsNullOrWhiteSpace(gameDirectory))
                return false;

            var backupFontPath = Path.Combine(
                gameDirectory,
                "LimbusCompany_Data",
                "Lang",
                "LLC_zh-CN",
                "BackupFont",
                BACKUP_FONT);

            return File.Exists(backupFontPath);
        }

        /// <summary>
        /// 检查游戏是否已安装汉化
        /// </summary>
        public bool HasChineseFontInstalled(string gameDirectory)
        {
            if (string.IsNullOrWhiteSpace(gameDirectory))
                return false;

            var fontDir = Path.Combine(
                gameDirectory,
                "LimbusCompany_Data",
                "Lang",
                "LLC_zh-CN",
                "Font",
                "Context");

            var ttfFont = Path.Combine(fontDir, CHINESE_FONT_TTF);
            var backupFont = Path.Combine(
                gameDirectory,
                "LimbusCompany_Data",
                "Lang",
                "LLC_zh-CN",
                "BackupFont",
                BACKUP_FONT);

            return File.Exists(ttfFont) || File.Exists(backupFont);
        }
    }
}
