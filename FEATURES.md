# LLC_MOD_Toolbox 功能说明文档

**项目名称**: LLC_MOD_Toolbox
**版本**: 1.3.0
**架构**: MVVM (Model-View-ViewModel)
**框架**: WPF (.NET 8.0)
**最后更新**: 2026-01-14

---

## 📋 目录

- [项目概述](#项目概述)
- [核心功能](#核心功能)
- [技术架构](#技术架构)
- [服务层](#服务层)
- [使用说明](#使用说明)
- [开发指南](#开发指南)
- [维护记录](#维护记录)

---

## 项目概述

LLC_MOD_Toolbox 是一个为《边狱公司》（Limbus Company）游戏提供的模组管理工具，支持汉化模组的安装、卸载、更新等功能。项目采用现代化的MVVM架构，具有良好的可维护性和扩展性。

### 主要特性

- ✅ 模组自动安装/卸载
- ✅ 版本检查与自动更新
- ✅ 多节点下载支持
- ✅ 灰度测试模式
- ✅ Mirror酱模式
- ✅ 抽卡模拟器
- ✅ 节点管理
- ✅ 字体替换
- ✅ 公告系统
- ✅ 启动器模式

---

## 核心功能

### 1. 模组安装

**功能描述**: 自动下载并安装最新的汉化模组

**实现位置**:
- ViewModel: `MainViewModel.InstallCommand`
- Service: `IModInstallService`

**关键特性**:
- 支持多种下载源（官方节点、Mirror酱、GitHub）
- 自动版本检测
- 进度条显示
- 安装前自动备份

**配置项**:
```json
{
  "install": {
    "autoInstall": true,
    "installWhenLaunch": false,
    "mirrorChyan": {
      "enable": false
    }
  }
}
```

### 2. 模组卸载

**功能描述**: 安全卸载已安装的汉化模组

**实现位置**:
- ViewModel: `MainViewModel.UninstallCommand`
- Service: `IModUninstallService`

**关键特性**:
- 自动备份检测
- 安全卸载机制
- UI实时更新

### 3. 版本管理

**功能描述**: 检查并更新模组版本

**实现位置**:
- ViewModel: `MainViewModel.RefreshVersionCommand`
- Service: `IVersionService`

**关键特性**:
- 自动版本对比
- 显示更新日志
- 一键更新功能

### 4. 节点管理

**功能描述**: 管理和切换下载节点

**实现位置**:
- ViewModel: `MainViewModel.InitializeNodesCommand`
- Service: `INodeManagementService`

**关键特性**:
- 支持多个下载节点
- 自动节点选择
- 节点健康检查
- 自定义节点添加

**配置文件**: `Config/NodeList.json`

```json
{
  "nodes": [
    {
      "name": "官方节点1",
      "url": "https://api.example.com/{0}",
      "isDefault": true
    }
  ]
}
```

### 5. Mirror酱模式

**功能描述**: 支持Mirror酱提供的API服务

**实现位置**:
- Service: `IMirrorChyanService`

**关键特性**:
- Token验证
- DPAPI加密存储
- 资源获取
- CDK验证

**安全特性**:
- Token使用DPAPI加密存储
- 本机绑定的安全存储
- 自动Token验证

### 6. 灰度测试模式

**功能描述**: 参与灰度测试，获取测试版模组

**实现位置**:
- Service: `IGreytestService`

**关键特性**:
- Token验证
- 测试版资源下载
- 特殊标识显示

### 7. 抽卡模拟器

**功能描述**: 内置抽卡模拟器，模拟游戏抽卡

**实现位置**:
- ViewModel: `GachaSimulatorPageViewModel`
- Service: `IGachaSimulatorService`

**关键特性**:
- 模拟游戏抽卡
- 统计功能
- 清空记录
- 数据本地存储

### 8. 字体替换

**功能描述**: 自定义游戏字体

**实现位置**:
- ViewModel: `FontReplacePageViewModel`
- Service: `IFontService`

**关键特性**:
- 支持自定义字体
- 自动备份原字体
- 一键恢复

### 9. 公告系统

**功能描述**: 显示项目公告和更新信息

**实现位置**:
- ViewModel: `AnnouncementPageViewModel`
- Service: `IAnnouncementService`

**关键特性**:
- 自动检查公告
- 重要公告倒计时
- 公告级别分类（普通/重要/特殊）
- 已读状态管理

### 10. 进度管理

**功能描述**: 统一的进度显示和管理

**实现位置**:
- Service: `IProgressService`

**关键特性**:
- 实时进度更新
- DispatcherTimer驱动（50ms）
- 自动边界限制（0-100%）
- 事件驱动更新

---

## 技术架构

### MVVM架构

```
┌─────────────────────────────────────────┐
│              View Layer                  │
│  (MainWindow.xaml, Pages, Controls)      │
└─────────────────────────────────────────┘
                   ↕ Data Binding
┌─────────────────────────────────────────┐
│           ViewModel Layer                │
│  (MainViewModel, PageViewModels)        │
│  - Commands                              │
│  - Properties                            │
│  - INotifyPropertyChanged                │
└─────────────────────────────────────────┘
                   ↕ Service Calls
┌─────────────────────────────────────────┐
│            Service Layer                 │
│  (27 Services)                          │
│  - Business Logic                        │
│  - Data Access                           │
│  - External API Integration             │
└─────────────────────────────────────────┘
                   ↕
┌─────────────────────────────────────────┐
│            Model Layer                   │
│  (Configuration, Nodes, etc.)           │
└─────────────────────────────────────────┘
```

### 依赖注入

项目使用 `ServiceLocator` 实现依赖注入：

```csharp
// 服务注册
services.AddSingleton<IConfigService, ConfigService>();
services.AddSingleton<INavigationService, NavigationService>();
// ... 其他27个服务

// 服务获取
var service = ServiceLocator.GetService<INavigationService>();
```

### Command模式

所有用户操作通过Command实现，避免Code-behind：

```xml
<!-- XAML -->
<Button Command="{Binding InstallCommand}" />
```

```csharp
// ViewModel
public ICommand InstallCommand { get; }

// 构造函数中
InstallCommand = new AsyncRelayCommand(ExecuteInstallAsync);
```

---

## 服务层

### 服务列表（27个）

#### 核心服务

1. **IConfigService** - 配置管理
   - 加载/保存配置
   - 线程安全访问
   - 配置热更新

2. **IPathService** - 路径管理
   - 游戏目录检测
   - 子目录路径生成
   - 路径验证

3. **IDownloadService** - 文件下载
   - HTTP/HTTPS下载
   - 进度回调
   - 断点续传支持

4. **IDialogService** - 对话框服务
   - 消息对话框
   - 确认对话框
   - 输入对话框

5. **IInstallService** - 安装服务
   - 模组安装核心逻辑
   - 版本检查
   - 文件解压

6. **IFontService** - 字体服务
   - 字体替换
   - 字体备份
   - 字体恢复

7. **IGreytestService** - 灰度测试服务
   - Token验证
   - 测试版资源获取
   - 灰度状态管理

8. **ILoadingTextService** - Loading文本服务
   - 随机Loading文本
   - 在线更新
   - 权重随机

9. **IAnnouncementService** - 公告服务
   - 公告检查
   - 公告解析
   - 倒计时管理

10. **IMirrorChyanService** - Mirror酱服务
    - Token管理
    - API调用
    - DPAPI加密

11. **IProgressService** - 进度服务
    - 进度管理
    - 定时器驱动
    - 事件通知

12. **IUIService** - UI服务
    - 全局操作控制
    - 对话框集成
    - URL打开

13. **IResourceService** - 资源服务
    - Hash缓存管理
    - 资源持久化
    - 缓存验证

14. **IErrorService** - 错误服务
    - 错误日志
    - 异常处理
    - 错误报告

15. **ILauncherService** - 启动器服务
    - 启动器模式
    - 游戏启动
    - 参数管理

16. **IVersionService** - 版本服务
    - 版本检查
    - 版本对比
    - 更新提示

17. **IFileUtilityService** - 文件工具服务
    - 文件操作
    - 下载节点选择
    - 文件验证

18. **IModInstallService** - 模组安装服务
    - 字体安装
    - 模组安装
    - Mirror酱集成

19. **IModUninstallService** - 模组卸载服务
    - 安全卸载
    - 备份检测
    - 恢复机制

20. **IGachaSimulatorService** - 抽卡模拟器服务
    - 抽卡逻辑
    - 概率计算
    - 统计管理

21. **INodeManagementService** - 节点管理服务
    - 节点加载
    - 默认节点选择
    - 后备机制

22. **INavigationService** - 导航服务
    - 页面导航
    - 页面状态管理
    - 控件字典管理

23. **ILinkService** - 链接服务
    - 外部链接管理
    - URL跳转
    - 链接字典

24. **IEasterEggService** - 彩蛋服务
    - 彩蛋状态管理
    - 图片加载
    - 可见性控制

#### API客户端

25. **OfficialApiClient** - 官方API客户端
    - 版本检查API
    - Hash获取API
    - 公告API
    - Loading文本API

26. **MirrorChyanApiClient** - Mirror酱API客户端
    - 资源获取API
    - 模组信息API
    - CDK验证API

### 服务使用示例

```csharp
// 获取服务
var configService = ServiceLocator.GetService<IConfigService>();
var pathService = ServiceLocator.GetService<IPathService>();

// 使用服务
var gameDir = pathService.GameDirectory;
var config = configService.AppSettings;
```

---

## 使用说明

### 环境要求

- **操作系统**: Windows 10/11 (x64)
- **.NET版本**: .NET 8.0 Runtime
- **游戏**: 边狱公司 (Limbus Company)

### 安装步骤

1. **下载最新版本**
   - 从 [Releases](../../releases) 下载最新版本
   - 解压到任意目录

2. **配置游戏路径**
   - 工具会自动检测Steam安装路径
   - 如果未检测到，手动选择游戏目录

3. **选择下载节点**
   - 设置 → 节点选择
   - 选择速度最快的节点

4. **安装模组**
   - 点击"开始安装"按钮
   - 等待下载和安装完成

### 配置文件

#### config.json

```json
{
  "general": {
    "internationalMode": false,
    "autoCheckUpdate": true
  },
  "install": {
    "autoInstall": false,
    "installWhenLaunch": false,
    "mirrorChyan": {
      "enable": false
    }
  },
  "announcement": {
    "annoVersion": 0
  }
}
```

#### NodeList.json

```json
{
  "nodes": [
    {
      "name": "官方节点1",
      "url": "https://api.example.com/{0}",
      "isDefault": true
    },
    {
      "name": "备用节点",
      "url": "https://backup.example.com/{0}",
      "isDefault": false
    }
  ]
}
```

### 功能使用说明

#### 自动安装

1. 启动工具
2. 点击"开始安装"按钮
3. 等待下载完成
4. 自动安装模组

#### Mirror酱模式

1. 配置 → Mirror酱设置
2. 输入Token
3. 启用Mirror酱模式
4. 安装时自动使用Mirror酱源

#### 抽卡模拟器

1. 点击"抽卡模拟器"选项
2. 首次使用需初始化
3. 选择抽取次数
4. 点击"抽取"按钮
5. 查看结果和统计

#### 字体替换

1. 点击"字体替换"选项
2. 选择自定义字体文件
3. 点击"应用字体"
4. 重启游戏生效

---

## 开发指南

### 项目结构

```
LLC_MOD_Toolbox/
├── Config/                  # 配置文件目录
│   ├── config.json         # 主配置文件
│   ├── NodeList.json       # 节点列表
│   └── loadingText.json    # Loading文本
├── Docs/                   # 文档目录
│   ├── REFACTOR_STATUS.md  # 重构状态
│   └── DAILY_PROGRESS_*.md # 进度文档
├── Interfaces/             # 接口定义
│   └── I*Service.cs       # 服务接口（27个）
├── Models/                 # 数据模型
│   └── *.cs              # 模型类
├── Services/               # 服务实现
│   └── *Service.cs       # 服务类（27个）
├── ViewModels/            # 视图模型
│   ├── MainViewModel.cs  # 主ViewModel
│   └── *PageViewModel.cs # 页面ViewModel（5个）
├── Views/                 # 自定义视图
│   └── *.xaml            # 自定义控件
├── Tests/                 # 单元测试
│   └── Services/         # 服务测试（16个文件，54个测试）
├── Utils/                 # 工具类
│   └── *.cs             # 辅助工具
├── Resources/             # 资源文件
├── MainWindow.xaml        # 主窗口
├── MainWindowWD.xaml.cs   # 业务逻辑后端
├── App.xaml              # 应用程序入口
└── ServiceLocator.cs      # 服务定位器
```

### 开发环境搭建

1. **克隆项目**
   ```bash
   git clone [repository-url]
   cd LLC_MOD_Toolbox
   ```

2. **还原依赖**
   ```bash
   dotnet restore
   ```

3. **构建项目**
   ```bash
   dotnet build --configuration Release
   ```

4. **运行项目**
   ```bash
   dotnet run --configuration Release
   ```

5. **运行测试**
   ```bash
   dotnet test
   ```

### 添加新功能

#### 1. 添加新服务

```csharp
// 1. 定义接口
public interface INewService
{
    void DoSomething();
}

// 2. 实现服务
public class NewService : INewService
{
    public void DoSomething()
    {
        // 实现逻辑
    }
}

// 3. 注册服务
// ServiceLocator.cs
services.AddSingleton<INewService, NewService>();

// 4. 使用服务
var service = ServiceLocator.GetService<INewService>();
service.DoSomething();
```

#### 2. 添加新ViewModel

```csharp
public class NewPageViewModel : ViewModelBase
{
    private readonly INewService _service;

    public NewPageViewModel(INewService service)
    {
        _service = service;
        // 初始化命令
        DoWorkCommand = new RelayCommand(ExecuteDoWork);
    }

    public ICommand DoWorkCommand { get; }

    private void ExecuteDoWork()
    {
        _service.DoSomething();
    }
}
```

#### 3. 添加单元测试

```csharp
public class NewServiceTests
{
    private readonly INewService _service;

    public NewServiceTests()
    {
        _service = new NewService();
    }

    [Fact]
    public void DoSomething_ShouldWork()
    {
        // Arrange & Act
        _service.DoSomething();

        // Assert
        Assert.True(true);
    }
}
```

### 代码规范

#### 命名规范

- **接口**: `I` + 功能名 + `Service` (例如: `IConfigService`)
- **实现类**: 功能名 + `Service` (例如: `ConfigService`)
- **ViewModel**: 功能名 + `ViewModel` (例如: `MainViewModel`)
- **字段**: `_camelCase` (私有字段)
- **属性**: `PascalCase`
- **方法**: `PascalCase`

#### 注释规范

```csharp
/// <summary>
/// 服务描述
/// </summary>
public class SampleService : ISampleService
{
    /// <summary>
    /// 方法描述
    /// </summary>
    /// <param name="param1">参数1说明</param>
    /// <returns>返回值说明</returns>
    public bool DoSomething(string param1)
    {
        // 实现
        return true;
    }
}
```

### CI/CD

项目使用GitHub Actions进行持续集成和部署：

#### Workflows

1. **build.yml** - 构建流程
   - 触发: push到master/main/develop分支
   - 操作: 还原依赖、构建项目、上传构建产物

2. **test.yml** - 测试流程
   - 触发: push到master/main/develop分支
   - 操作: 运行所有单元测试、上传测试结果

3. **release.yml** - 发布流程
   - 触发: 创建Release时
   - 操作: 构建、发布、打包、上传Release资产

---

## 维护记录

### 版本历史

#### v1.3.1 (2026-01-13/14)
- ✅ 代码清理：删除约350行旧代码
  - 删除字体替换旧Click事件处理（已迁移到FontReplacePageViewModel）
  - 删除灰度测试重复代码（已迁移到GreytestPageViewModel）
  - 删除未使用的辅助方法
  - 删除版本检查fallback代码（统一使用IVersionService）
- ✅ 状态统一管理迁移到Service层
  - 灰度测试状态（greytestStatus/greytestUrl）→ IGreytestService
  - MirrorChyan状态（isMirrorChyanMode/mirrorChyanToken）→ IMirrorChyanService
- ✅ Loading文本逻辑迁移到ILoadingTextService
- ✅ 版本检查逻辑统一使用IVersionService
- ✅ 修复运行时错误：MirrorChyanApiClient 依赖注入配置缺失
- ✅ 可空引用类型警告安全修复（警告从239减至112，降幅53%）
  - 修复JSON嵌套访问空引用、动态类型空引用检查
  - 修复异步方法CS1998警告（NavigationService、InstallService）
  - 涉及文件：MirrorChyanApiClient、NavigationService、InstallService、SettingsPageViewModel等
- ✅ MainWindowWD.xaml.cs 从~1600行减少到~1255行（减少21.5%）
- ✅ 修复抽卡模拟器UI更新
- ✅ 修复灰度测试状态同步
- ✅ 添加抽卡结果趣味消息提示

#### v1.3.0 (2026-01-07)
- ✅ 完成MVVM架构重构
- ✅ 新增27个服务层
- ✅ 实现54个单元测试
- ✅ 架构评分提升到92分（优秀）
- ✅ 新增LinkService和EasterEggService
- ✅ 实现延迟加载优化
- ✅ 添加CI/CD流程

#### v1.2.0 (2025-xx-xx)
- 新增抽卡模拟器功能
- 优化节点管理

#### v1.1.0 (2025-xx-xx)
- 新增Mirror酱模式支持
- 优化下载速度

#### v1.0.0 (2025-xx-xx)
- 初始版本发布
- 基础模组安装功能

### 已知问题

1. **编译警告**: 112个可空引用类型警告（已从239个减少53%，非阻塞）
2. **MainWindowWD.xaml.cs**: 约1255行（已从1600行减少21.5%），持续优化中
3. **测试覆盖**: 约50%，可提升到80%+

### 计划改进

1. **短期计划**
   - ~~清理可空引用类型警告~~ → 已完成53%（112/239）
   - 提升测试覆盖率到60%
   - 继续迁移剩余业务逻辑到ViewModel/Service层

2. **长期计划**
   - 完全MVVM化（消除Code-behind）
   - 测试覆盖率提升到80%
   - 添加性能监控
   - 完善API文档

---

## 技术支持

### 文档

- [重构状态文档](Docs/REFACTOR_STATUS.md)
- [进度文档](Docs/DAILY_PROGRESS_*.md)
- [最终总结](Docs/FINAL_REFACTOR_SUMMARY.md)

### 问题反馈

如遇到问题，请在 [Issues](../../issues) 中提交Bug报告。

### 贡献指南

欢迎提交Pull Request！

1. Fork本项目
2. 创建功能分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 开启Pull Request

### 开发者协议

- 遵循现有代码规范
- 新功能需要添加单元测试
- 更新相关文档
- 提交前确保编译无错误

---

## 许可证

本项目采用 MIT 许可证 - 详见 [LICENSE](LICENSE) 文件

---

## 致谢

- 感谢所有为本项目做出贡献的开发者
- 感谢边狱公司汉化团队的辛勤工作
- 感谢所有用户的反馈和支持

---

**文档维护**: LLC_MOD_Toolbox 开发团队
**最后更新**: 2026-01-07
**文档版本**: 1.3.0
