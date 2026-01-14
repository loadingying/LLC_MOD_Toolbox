using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LLC_MOD_Toolbox
{
    /// <summary>
    /// 安全字符串存储工具类
    /// 使用Windows DPAPI（Data Protection API）加密敏感数据
    /// 参考MAA（MuiAide）的实现方式
    /// </summary>
    /// <remarks>
    /// DPAPI特性：
    /// - 只有当前Windows用户可以解密
    /// - 数据与计算机绑定，无法在其他机器上解密
    /// - 适合存储CDK、Token等敏感信息
    /// </remarks>
    public static class SecureStringStorage
    {
        /// <summary>
        /// 存储文件路径
        /// 位置：%LocalAppData%\LLC_MOD_Toolbox\MirrorChyan.dat
        /// </summary>
        private static readonly string StoragePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "LLC_MOD_Toolbox", "MirrorChyan.dat");

        /// <summary>
        /// 保存加密字符串到本地
        /// </summary>
        /// <param name="data">要加密并保存的字符串</param>
        /// <exception cref="Exception">保存失败时抛出</exception>
        public static void SaveToken(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                DeleteSecretFile();
                return;
            }

            try
            {
                var bytes = Encoding.UTF8.GetBytes(data);

                // 使用DPAPI加密，只有当前用户可以解密
                var encryptedBytes = ProtectedData.Protect(bytes,
                    null, DataProtectionScope.CurrentUser);

                Directory.CreateDirectory(Path.GetDirectoryName(StoragePath)!);
                File.WriteAllBytes(StoragePath, encryptedBytes);
            }
            catch (Exception ex)
            {
                throw new Exception("保存数据失败", ex);
            }
        }

        /// <summary>
        /// 读取并解密字符串
        /// </summary>
        /// <returns>解密后的字符串，失败时返回空字符串</returns>
        public static string LoadToken()
        {
            try
            {
                if (!File.Exists(StoragePath))
                    return "";

                var encryptedBytes = File.ReadAllBytes(StoragePath);
                var decryptedBytes = ProtectedData.Unprotect(encryptedBytes,
                    null, DataProtectionScope.CurrentUser);

                return Encoding.UTF8.GetString(decryptedBytes);
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// 删除保存的数据文件
        /// </summary>
        public static void DeleteSecretFile()
        {
            try
            {
                if (File.Exists(StoragePath))
                {
                    File.Delete(StoragePath);
                }
            }
            catch
            {
                // 忽略删除失败
            }
        }

        /// <summary>
        /// 检查是否存在保存的数据
        /// </summary>
        /// <returns>如果数据文件存在返回true，否则返回false</returns>
        public static bool HasSavedData()
        {
            return File.Exists(StoragePath);
        }
    }
}
