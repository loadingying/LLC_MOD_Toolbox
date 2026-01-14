using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Win32;

/// <summary>
/// Steam游戏定位器
/// 自动查找Steam游戏的安装路径
/// </summary>
public static class SteamLocator
{
    /// <summary>
    /// 查找Limbus Company的安装路径
    /// </summary>
    /// <param name="appId">Steam应用ID（默认为1973530）</param>
    /// <param name="executableName">可执行文件名称（默认为LimbusCompany.exe）</param>
    /// <returns>游戏安装目录的完整路径</returns>
    /// <exception cref="InvalidOperationException">未在注册表找到Steam安装路径</exception>
    /// <exception cref="DirectoryNotFoundException">未在任何库中找到游戏</exception>
    public static string FindLimbusCompanyPath(string? appId, string? executableName)
    {
        var steamRoot = GetSteamRootFromRegistry();
        if (string.IsNullOrWhiteSpace(steamRoot))
            throw new InvalidOperationException("未在注册表找到 Steam 安装路径");

        steamRoot = NormalizePath(steamRoot);

        var libs = new List<string> { steamRoot };
        var vdf = Path.Combine(steamRoot, "steamapps", "libraryfolders.vdf");
        libs.AddRange(ParseLibraryFolders(vdf));

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var raw in libs)
        {
            var lib = NormalizePath(raw);
            if (!seen.Add(lib)) continue;

            var steamapps = Path.Combine(lib, "steamapps");

            if (!string.IsNullOrWhiteSpace(appId))
            {
                var acf = Path.Combine(steamapps, $"appmanifest_{appId}.acf");
                var dir = ParseInstallDir(acf);
                if (!string.IsNullOrWhiteSpace(dir))
                {
                    var full = Path.Combine(steamapps, "common", dir);
                    if (IsValidGameDir(full, executableName))
                        return full;
                }
            }

            var fallback = Path.Combine(steamapps, "common", "Limbus Company");
            if (IsValidGameDir(fallback, executableName))
                return fallback;
        }

        throw new DirectoryNotFoundException("未在任何库中找到 Limbus Company");
    }

    /// <summary>
    /// 从注册表获取Steam安装根目录
    /// </summary>
    private static string GetSteamRootFromRegistry()
    {
        if (TryRegGetString(Registry.CurrentUser, @"Software\Valve\Steam", "SteamPath", out var root) &&
            !string.IsNullOrWhiteSpace(root))
            return root!;

        if (TryRegGetString(Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Valve\Steam", "InstallPath", out root) &&
            !string.IsNullOrWhiteSpace(root))
            return root!;

        return string.Empty;
    }

    /// <summary>
    /// 尝试从注册表读取字符串值
    /// </summary>
    private static bool TryRegGetString(RegistryKey hive, string path, string name, out string? value)
    {
        value = null;
        try
        {
            using var key = hive.OpenSubKey(path, false);
            if (key == null) return false;
            value = key.GetValue(name) as string;
            return value != null;
        }
        catch { return false; }
    }

    /// <summary>
    /// 标准化路径格式
    /// </summary>
    private static string NormalizePath(string p)
    {
        if (string.IsNullOrWhiteSpace(p)) return p;
        p = p.Replace('/', '\\').Replace(@"\\", @"\");
        try { return Path.GetFullPath(p); }
        catch { return p; }
    }

    /// <summary>
    /// 解析Steam库文件夹配置文件（libraryfolders.vdf）
    /// </summary>
    private static IEnumerable<string> ParseLibraryFolders(string file)
    {
        var results = new List<string>();
        try
        {
            if (!File.Exists(file)) return results;
            var text = File.ReadAllText(file);
            var re = new Regex("(?i)\"path\"\\s*\"([^\"]+)\"", RegexOptions.Compiled);
            foreach (Match m in re.Matches(text))
            {
                if (m.Groups.Count >= 2)
                    results.Add(NormalizePath(m.Groups[1].Value));
            }
        }
        catch { }
        return results;
    }

    /// <summary>
    /// 解析Steam应用清单文件（appmanifest_*.acf）
    /// </summary>
    private static string ParseInstallDir(string acf)
    {
        try
        {
            if (!File.Exists(acf)) return string.Empty;
            var b = File.ReadAllText(acf);
            var re = new Regex("(?i)\"installdir\"\\s*\"([^\"]+)\"", RegexOptions.Compiled);
            var m = re.Match(b);
            if (m.Success && m.Groups.Count >= 2)
                return m.Groups[1].Value.Replace(@"\\", @"\");
        }
        catch { }
        return string.Empty;
    }

    /// <summary>
    /// 验证游戏目录是否有效
    /// </summary>
    private static bool IsValidGameDir(string dir, string? executableName)
    {
        if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir)) return false;
        if (string.IsNullOrWhiteSpace(executableName)) return true;
        return File.Exists(Path.Combine(dir, executableName));
    }
}
