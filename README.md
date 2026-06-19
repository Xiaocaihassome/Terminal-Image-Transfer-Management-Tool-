# 终端图片中转管理工具 (ImageManager)
<img width="151" height="151" alt="生成截图管理器1_1图标 (3)(1)" src="https://github.com/user-attachments/assets/ba13be46-055b-42f4-a09a-078ffd105bdf" />


一款专为 AI 终端设计的轻量级图片中转管理工具，支持拖拽、粘贴、路径复制和一键粘贴到目标窗口。

## 功能特性


- **拖拽添加** — 直接拖入图片到窗口
- **粘贴截图** — Ctrl+V 粘贴剪贴板中的图片
- **复制路径** — 勾选图片后一键复制文件路径
- **粘贴到窗口** — 自动将路径粘贴到前台窗口（2秒倒计时切换目标窗口）
- **毛玻璃效果** — 窗口背景实时模糊桌面，视觉效果优雅
- **深色/浅色主题** — 支持手动切换或跟随系统
- **透明度调节** — 自定义窗口透明度（30% ~ 100%）
- **自定义临时目录** — 可设置图片存储位置
- **单实例运行** — 防止重复打开
- **退出自动清理** — 可选关闭时清空临时文件
<img width="1155" height="762" alt="image" src="https://github.com/user-attachments/assets/9be058cf-2f71-49a1-829e-4cd7a1d59e09" /><<img width="1136" height="750" alt="image" src="https://github.com/user-attachments/assets/d83541ff-efde-40fc-9bac-27e9f89c2296" />


## 环境要求

- Windows 10 (build 1809+) 或 Windows 11
- .NET 8.0 Runtime（或使用独立发布版本）

## 快速开始

### 方式一：下载独立版本

从 [Releases](https://github.com/xiaocaiyou-dianliao/ImageManager/releases) 下载最新版本，解压后直接运行 `ImageManager.exe`。

### 方式二：从源码构建

```bash
git clone https://github.com/xiaocaiyou-dianliao/ImageManager.git
cd ImageManager
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

生成的 EXE 位于 `bin/Release/net8.0-windows/win-x64/publish/ImageManager.exe`。

## 使用方法

1. **添加图片**：拖拽图片到窗口，或按 Ctrl+V 粘贴截图
2. **选择图片**：勾选需要的图片（支持全选）
3. **复制路径**：点击「复制路径」按钮
4. **粘贴到窗口**：点击「粘贴到窗口」，在2秒内切换到目标窗口，自动发送 Ctrl+V

## 项目结构

```
ImageManager/
├── App.xaml / App.xaml.cs          # 应用入口、DI 容器、单实例
├── MainWindow.xaml / .cs           # 主窗口
├── SettingsWindow.xaml / .cs       # 设置窗口
├── ViewModels/
│   ├── MainViewModel.cs            # 主窗口逻辑
│   └── SettingsViewModel.cs        # 设置逻辑
├── Services/
│   ├── ConfigService.cs            # 配置持久化
│   ├── ThemeManager.cs             # 主题管理（单例）
│   ├── PasteService.cs             # 粘贴到窗口（Win32 SendInput）
│   ├── FileService.cs              # 文件操作
│   ├── ThumbnailService.cs         # 缩略图生成
│   ├── ClipboardService.cs         # 剪贴板操作
│   └── ToastService.cs             # 提示消息
├── Models/
│   └── ImageItem.cs                # 图片数据模型
├── Converters/
│   └── Converters.cs               # XAML 转换器
├── Resources/
│   ├── Styles.xaml                 # 控件样式
│   └── avatar.png                  # 作者头像
└── Themes/
    ├── Light.xaml                  # 浅色主题资源
    └── Dark.xaml                   # 深色主题资源
```

## 技术栈

- **框架**: .NET 8 + WPF
- **架构**: MVVM（CommunityToolkit.Mvvm）
- **依赖注入**: Microsoft.Extensions.DependencyInjection
- **毛玻璃效果**: WPF 桌面捕获 + 高斯模糊
- **粘贴模拟**: Win32 SendInput API

## 使用须知与免责声明
- 本仓库及对应工具的开放目的为个人学习、技术交流与非营利测试，作者倡议请勿将其直接用于商业盈利用途。
- 项目源码的正式授权规则以 MIT 许可证为准，商业使用请自行评估合规风险。
- 本工具为纯本地运行的图片中转管理工具，不内置、不托管任何在线图片资源。用户自行导入、粘贴的本地图片、截图及程序生成的临时文件，其版权、合规性与全部使用后果均由使用者自行承担。
- 若因对本项目源码、编译程序进行传播、修改、二次分发，或搭配第三方图床、在线素材资源使用而引发版权纠纷及其他法律责任，全部责任由使用者自行承担，项目开发者不承担任何法律责任。
- 本工具按“现状”提供，不对使用过程中的操作失误、文件丢失、系统兼容问题等潜在风险提供担保，所有使用风险由使用者自行承担。
- 本项目在 AI 的广泛协助下开发，因此仍可能存在细微或不易察觉的程序问题。若给您带来不便，敬请理解。

## 许可证

[MIT License](LICENSE)

## 作者

**小蔡有点料** — 开源工具开发者
