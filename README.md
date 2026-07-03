# 终端图片中转管理工具

[English](#english) | [繁體中文](#繁體中文) | [日本語](#日本語) | [한국어](#한국어)

<img width="151" height="151" alt="App Icon" src="https://raw.githubusercontent.com/Xiaocaihassome/Terminal-Image-Transfer-Management-Tool-/main/app-icon.png" />

一款专为 AI 终端设计的轻量级图片中转管理工具，支持拖拽、粘贴、路径复制和一键粘贴到目标窗口。

## 功能特性

- **拖拽添加** — 直接拖入图片到窗口
- **粘贴截图** — Ctrl+V 粘贴剪贴板中的图片
- **复制路径** — 勾选图片后一键复制文件路径
- **粘贴到窗口** — 自动将路径粘贴到前台窗口（2秒倒计时切换目标窗口）
- **背景效果** — 毛玻璃 / Win11 亚克力 / 纯色 三种背景模式（亚克力不支持时自动回退）
- **字体设置** — 自定义字体族、字重，实时预览，快速选择常用字体
- **隐私模式** — 地址遮挡，隐藏敏感文件路径防止意外暴露
- **窗口置顶** — 窗口始终显示在其他窗口上方
- **更新检查** — 自动检测 GitHub Releases 新版本，一键下载安装
- **粘贴后删除** — 粘贴到窗口时自动删除源文件（设置中开启）
- **开机自启** — 系统启动时自动运行（设置中开启）
- **错误日志** — 自动记录异常，支持一键复制 AI 修复提示词
- **多文件粘贴** — 同时粘贴多个文件路径
- **深色/浅色主题** — 支持手动切换或跟随系统
- **透明度调节** — 自定义窗口透明度（30% ~ 100%）
- **自定义临时目录** — 可设置图片存储位置
- **单实例运行** — 防止重复打开
- **退出自动清理** — 可选关闭时清空临时文件
- **多语言支持** — 简体中文、繁體中文、English、日本語、한국어

## 环境要求

- Windows 10 (build 1809+) 或 Windows 11
- .NET 8.0 Runtime（或使用独立发布版本）

## 快速开始

### 方式一：下载独立版本

从 [Releases](https://github.com/Xiaocaihassome/imagemanager/releases) 下载最新版本，解压后直接运行 `ImageManager.exe`。

### 方式二：从源码构建

```bash
git clone https://github.com/Xiaocaihassome/imagemanager.git
cd ImageManager
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

生成的 EXE 位于 `bin/Release/net8.0-windows/win-x64/publish/ImageManager.exe`。

## 使用教程

1. 拖入图片到窗口，或按 Ctrl+V 粘贴截图
2. 勾选需要的图片，点击「复制路径」复制到剪贴板
3. 在 AI 终端或任意窗口中 Ctrl+V 粘贴路径即可
4. 点击「粘贴到窗口」可自动将路径粘贴到前台窗口
5. 设置中可切换背景效果、字体、语言和隐私模式

## 常见问题

**Q: 粘贴到窗口没有反应？**
A: 点击后在 2 秒内切换到目标窗口，工具会自动发送 Ctrl+V。

**Q: 隐私模式有什么用？**
A: 开启后所有文件路径会被遮蔽为 `********`，适用于屏幕共享或演示场景。

**Q: 背景效果该选哪个？**
A: 毛玻璃适合大多数场景；Win11 亚克力使用系统材质更通透；无则为纯色背景。

**Q: 字体设置如何生效？**
A: 在外观设置中点击字体设置，选择字体和字重后点击「应用」即可全局生效。

## 项目结构

```
ImageManager/
├── App.xaml / App.xaml.cs              # 应用入口、DI 容器、单实例
├── MainWindow.xaml / .cs               # 主窗口
├── SettingsWindow.xaml / .cs           # 设置窗口
├── FontSettingsWindow.xaml / .cs       # 字体设置窗口
├── ViewModels/
│   ├── MainViewModel.cs                # 主窗口逻辑
│   └── SettingsViewModel.cs            # 设置逻辑
├── Services/
│   ├── ConfigService.cs                # 配置持久化
│   ├── ThemeManager.cs                 # 主题管理（单例）
│   ├── BackdropService.cs              # 背景效果（毛玻璃/亚克力/纯色）
│   ├── PasteService.cs                 # 粘贴到窗口（Win32 SendInput）
│   ├── UpdateService.cs                # 更新检查（GitHub Releases）
│   ├── FileService.cs                  # 文件操作
│   ├── ThumbnailService.cs             # 缩略图生成
│   ├── ClipboardService.cs             # 剪贴板操作
│   └── ToastService.cs                 # 提示消息
├── Models/
│   └── ImageItem.cs                    # 图片数据模型
├── Converters/
│   └── Converters.cs                   # XAML 转换器
├── Resources/
│   ├── Icons/                          # 主题图标（PNG）
│   ├── Lang/                           # 多语言资源（5种语言）
│   ├── Styles.xaml                     # 控件样式
│   └── app.ico / app.png              # 应用图标
└── Themes/
    ├── Light.xaml                      # 浅色主题资源
    └── Dark.xaml                       # 深色主题资源
```

## 技术栈

- **框架**: .NET 8 + WPF
- **架构**: MVVM（CommunityToolkit.Mvvm）
- **依赖注入**: Microsoft.Extensions.DependencyInjection
- **背景效果**: DWM Acrylic API（含视觉验证）/ 桌面捕获 + 高斯模糊
- **粘贴模拟**: Win32 SendInput API
- **更新检测**: GitHub Releases API

## 使用须知与免责声明

- 本仓库及对应工具的开放目的为个人学习、技术交流与非营利测试，作者倡议请勿将其直接用于商业盈利用途。
- 项目源码的正式授权规则以 MIT 许可证为准，商业使用请自行评估合规风险。
- 本工具为纯本地运行的图片中转管理工具，不内置、不托管任何在线图片资源。用户自行导入、粘贴的本地图片、截图及程序生成的临时文件，其版权、合规性与全部使用后果均由使用者自行承担。
- 若因对本项目源码、编译程序进行传播、修改、二次分发，或搭配第三方图床、在线素材资源使用而引发版权纠纷及其他法律责任，全部责任由使用者自行承担，项目开发者不承担任何法律责任。
- 本工具按"现状"提供，不对使用过程中的操作失误、文件丢失、系统兼容问题等潜在风险提供担保，所有使用风险由使用者自行承担。
- 本项目在 AI 的广泛协助下开发，因此仍可能存在细微或不易察觉的程序问题。若给您带来不便，敬请理解。

## 许可证

[MIT License](LICENSE)

## 作者

**小蔡有点料** — 开源工具开发者

## 致谢

特别感谢以下项目和资源：

- [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/WindowsCommunityToolkit) — MVVM 框架
- [Microsoft.Extensions.DependencyInjection](https://github.com/dotnet/runtime) — 依赖注入
- [Game Icon Pack by Nieobie](https://github.com/Nieobie/Game-Icon-Pack) — 应用内 UI 图标
- [Inno Setup](https://jrsoftware.org/isinfo.php) — 安装程序制作工具

## 官网

https://imagemanager-6gs.pages.dev/

---

# English

<a id="english"></a>

# Terminal Image Transfer Management Tool

A lightweight image transfer management tool designed for AI terminals. Supports drag & drop, clipboard paste, path copying, and one-click paste to target windows.

## Features

- **Drag & Drop** — Drop images directly into the window
- **Paste Screenshots** — Ctrl+V to paste images from clipboard
- **Copy Paths** — Select images and copy file paths with one click
- **Paste to Window** — Automatically paste the path to the foreground window (2-second countdown to switch target)
- **Background Effects** — Frosted Glass / Win11 Acrylic / Solid color modes (auto-fallback when unsupported)
- **Font Settings** — Customize font family & weight with live preview and quick selection
- **Privacy Mode** — Address masking, hide sensitive file paths to prevent accidental exposure
- **Always on Top** — Window stays on top of other windows
- **Update Check** — Auto-detect new releases on GitHub, one-click download & install
- **Delete After Paste** — Auto-delete source file when pasting (enable in settings)
- **Auto Start on Boot** — Run automatically on system startup (enable in settings)
- **Error Logging** — Auto-record exceptions, one-click copy AI repair prompt
- **Multi-file Paste** — Paste multiple file paths at once
- **Light / Dark Theme** — Manual switching or follow system preference
- **Transparency Control** — Customizable window opacity (30% ~ 100%)
- **Custom Temp Directory** — Configure where images are stored
- **Single Instance** — Prevents duplicate instances
- **Auto Cleanup on Exit** — Optionally clear temp files when closing
- **Multi-language** — 简体中文, 繁體中文, English, 日本語, 한국어

## Requirements

- Windows 10 (build 1809+) or Windows 11
- .NET 8.0 Runtime (or use the self-contained build)

## Quick Start

### Option 1: Download the Installer

Download the latest version from [Releases](https://github.com/Xiaocaihassome/imagemanager/releases), extract, and run `ImageManager.exe`.

### Option 2: Build from Source

```bash
git clone https://github.com/Xiaocaihassome/imagemanager.git
cd ImageManager
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

The output EXE is at `bin/Release/net8.0-windows/win-x64/publish/ImageManager.exe`.

## Tutorial

1. Drag images into the window, or press Ctrl+V to paste a screenshot
2. Check the images you need, click "Copy Path" to copy to clipboard
3. In AI terminal or any window, press Ctrl+V to paste the path
4. Click "Paste to Window" to automatically paste the path to the foreground window
5. In Settings, you can switch background effects, font, language, and privacy mode

## FAQ

**Q: Paste to Window doesn't respond?**
A: After clicking, switch to the target window within 2 seconds, and the tool will automatically send Ctrl+V.

**Q: What does Privacy Mode do?**
A: When enabled, all file paths are masked as `********`, suitable for screen sharing or presentation scenarios.

**Q: Which background effect should I choose?**
A: Frosted Glass suits most scenarios; Win11 Acrylic uses system material for a more transparent look; None gives a solid background.

**Q: How to apply font settings?**
A: In Appearance settings, click Font Settings, select font and weight, then click "Apply" to take effect globally.

## Tech Stack

- **Framework**: .NET 8 + WPF
- **Architecture**: MVVM (CommunityToolkit.Mvvm)
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **Backdrop Effects**: DWM Acrylic API / Desktop capture + Gaussian blur
- **Paste Simulation**: Win32 SendInput API
- **Update Detection**: GitHub Releases API

## Usage Notice & Disclaimer

- This repository and its associated tools are intended solely for personal learning, technical exchange, and non-profit testing. The author requests that you do not use it directly for commercial gain.
- The formal licensing terms for the project source code are governed by the MIT License. Please assess compliance risks on your own for any commercial use.
- This tool runs entirely locally as an image transfer manager. It does not include or host any online image resources. Users are solely responsible for the copyright, compliance, and all consequences of any local images, screenshots, or temporary files they import, paste, or generate.
- If disputes or legal liabilities arise from distributing, modifying, or re-distributing this project's source code or compiled program, or from using it in conjunction with third-party image hosting services or online resources, all responsibility lies with the user. The project developer assumes no legal liability.
- This tool is provided "as is" without warranty of any kind. The developer makes no guarantees regarding operational errors, file loss, system compatibility, or other potential risks. All usage risks are borne by the user.
- This project was developed with extensive AI assistance and may therefore contain subtle or hard-to-detect issues. We appreciate your understanding if any inconvenience arises.

## License

[MIT License](LICENSE)

## Author

**Xiaocaihassome** — Open-source tool developer

## Acknowledgments

Special thanks to the following projects and resources:

- [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/WindowsCommunityToolkit) — MVVM framework
- [Microsoft.Extensions.DependencyInjection](https://github.com/dotnet/runtime) — Dependency injection
- [Game Icon Pack by Nieobie](https://github.com/Nieobie/Game-Icon-Pack) — UI icons
- [Inno Setup](https://jrsoftware.org/isinfo.php) — Installer maker

## Website

https://imagemanager-6gs.pages.dev/

---

# 繁體中文

<a id="繁體中文"></a>

# 終端圖片中轉管理工具

一款專為 AI 終端設計的輕量級圖片中轉管理工具，支援拖曳、貼上、路徑複製和一鍵貼上到目標視窗。

## 功能特性

- **拖曳新增** — 直接拖入圖片到視窗
- **貼上截圖** — Ctrl+V 貼上剪貼簿中的圖片
- **複製路徑** — 勾選圖片後一鍵複製檔案路徑
- **貼上到視窗** — 自動將路徑貼上到前景視窗（2秒倒數計時切換目標視窗）
- **背景效果** — 毛玻璃 / Win11 亞克力 / 純色 三種背景模式（亞克力不支援時自動回退）
- **字型設定** — 自訂字型族、字重，即時預覽，快速選擇常用字型
- **隱私模式** — 地址遮擋，隱藏敏感檔案路徑防止意外暴露
- **視窗置頂** — 視窗始終顯示在其他視窗上方
- **更新檢查** — 自動偵測 GitHub Releases 新版本，一鍵下載安裝
- **貼上後刪除** — 貼上到視窗時自動刪除來源檔案（設定中開啟）
- **開機自啟** — 系統啟動時自動執行（設定中開啟）
- **錯誤日誌** — 自動記錄異常，支援一鍵複製 AI 修復提示詞
- **多檔案貼上** — 同時貼上多個檔案路徑
- **深色/淺色主題** — 支援手動切換或跟隨系統
- **透明度調節** — 自訂視窗透明度（30% ~ 100%）
- **自訂暫存目錄** — 可設定圖片儲存位置
- **單一執行個體** — 防止重複開啟
- **離開自動清理** — 選擇關閉時清空暫存檔案
- **多語言支援** — 簡體中文、繁體中文、English、日本語、한국어

## 環境需求

- Windows 10 (build 1809+) 或 Windows 11
- .NET 8.0 Runtime（或使用獨立發佈版本）

## 快速開始

### 方式一：下載獨立版本

從 [Releases](https://github.com/Xiaocaihassome/imagemanager/releases) 下載最新版本，解壓縮後直接執行 `ImageManager.exe`。

### 方式二：從原始碼建置

```bash
git clone https://github.com/Xiaocaihassome/imagemanager.git
cd ImageManager
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

產生的 EXE 位於 `bin/Release/net8.0-windows/win-x64/publish/ImageManager.exe`。

## 使用教程

1. 拖入圖片到視窗，或按 Ctrl+V 貼上截圖
2. 勾選需要的圖片，點擊「複製路徑」複製到剪貼簿
3. 在 AI 終端或任意視窗中 Ctrl+V 貼上路徑即可
4. 點擊「貼上到視窗」可自動將路徑貼上到前景視窗
5. 設定中可切換背景效果、字型、語言和隱私模式

## 常見問題

**Q: 貼上到視窗沒有反應？**
A: 點擊後在 2 秒內切換到目標視窗，工具會自動發送 Ctrl+V。

**Q: 隱私模式有什麼用？**
A: 開啟後所有檔案路徑會被遮蔽為 `********`，適用於螢幕共享或簡報場景。

**Q: 背景效果該選哪個？**
A: 毛玻璃適合大多數場景；Win11 亞克力使用系統材質更通透；無則為純色背景。

**Q: 字型設定如何生效？**
A: 在外觀設定中點擊字型設定，選擇字型和字重後點擊「套用」即可全域生效。

## 技術架構

- **框架**: .NET 8 + WPF
- **架構**: MVVM（CommunityToolkit.Mvvm）
- **依賴注入**: Microsoft.Extensions.DependencyInjection
- **背景效果**: DWM Acrylic API（含視覺驗證）/ 桌面擷取 + 高斯模糊
- **貼上模擬**: Win32 SendInput API
- **更新偵測**: GitHub Releases API

## 使用須知與免責聲明

- 本倉庫及對應工具的開放目的為個人學習、技術交流與非營利測試，作者倡議請勿將其直接用於商業盈利用途。
- 專案原始碼的正式授權規則以 MIT 許可證為準，商業使用請自行評估合規風險。
- 本工具為純本機執行的圖片中轉管理工具，不內建、不託管任何線上圖片資源。使用者自行匯入、貼上的本機圖片、截圖及程式產生的暫存檔案，其版權、合規性與全部使用後果均由使用者自行承擔。
- 若因對本專案原始碼、編譯程式進行傳播、修改、二次分發，或搭配第三方圖床、線上素材資源使用而引發版權糾紛及其他法律責任，全部責任由使用者自行承擔，專案開發者不承擔任何法律責任。
- 本工具按「現狀」提供，不對使用過程中的操作失誤、檔案遺失、系統相容問題等潛在風險提供擔保，所有使用風險由使用者自行承擔。
- 本專案在 AI 的廣泛協助下開發，因此仍可能存在細微或不易察覺的程式問題。若給您帶來不便，敬請理解。

## 許可證

[MIT License](LICENSE)

## 作者

**小蔡有點料** — 開源工具開發者

## 致謝

特別感謝以下專案和資源：

- [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/WindowsCommunityToolkit) — MVVM 框架
- [Microsoft.Extensions.DependencyInjection](https://github.com/dotnet/runtime) — 依賴注入
- [Game Icon Pack by Nieobie](https://github.com/Nieobie/Game-Icon-Pack) — 應用內 UI 圖示
- [Inno Setup](https://jrsoftware.org/isinfo.php) — 安裝程式製作工具

## 官網

https://imagemanager-6gs.pages.dev/

---

# 日本語

<a id="日本語"></a>

# ターミナル画像転送管理ツール

AIターミナル向けの軽量画像転送管理ツール。ドラッグ＆ドロップ、クリップボード貼り付け、パスコピー、ターゲットウィンドウへのワンクリック貼り付けに対応しています。

## 機能

- **ドラッグ＆ドロップ** — ウィンドウに画像を直接ドロップ
- **スクリーンショット貼り付け** — Ctrl+Vでクリップボードから画像を貼り付け
- **パスコピー** — 画像を選択してワンクリックでファイルパスをコピー
- **ウィンドウに貼り付け** — パスをフロントウィンドウに自動貼り付け（2秒カウントダウンでターゲット切替）
- **背景エフェクト** — フロストグラス / Win11 アクリリック / ソリッドカラー 3種類（サポート外時は自動フォールバック）
- **フォント設定** — フォントファミリー・ウェイトのカスタマイズ、ライブプレビュー、クイック選択
- **プライバシーモード** — アドレスマスキング、機密ファイルパスの誤公開を防止
- **ウインドウを最前面に固定** — ウインドウを常に他のウインドウの上に表示
- **更新チェック** — GitHub Releasesの新バージョンを自動検出、ワンクリックダウンロード＆インストール
- **貼り付け後に削除** — 貼り付け時にソースファイルを自動削除（設定で有効化）
- **電源オン時に自動起動** — システム起動時に自動実行（設定で有効化）
- **エラーログ** — 異常を自動記録、AI修復プロンプトをワンクリックコピー
- **複数ファイル貼り付け** — 複数のファイルパスを一度に貼り付け
- **ライト/ダークテーマ** — 手動切替またはシステム設定に追従
- **透過度調整** — ウィンドウの透過度をカスタマイズ（30% ~ 100%）
- **カスタム一時ディレクトリ** — 画像の保存先を設定可能
- **シングルインスタンス** — 重複起動を防止
- **終了時自動クリーンアップ** — 閉じる際に一時ファイルを削除可能
- **多言語対応** — 簡体中文、繁體中文、English、日本語、한국어

## システム要件

- Windows 10 (build 1809+) または Windows 11
- .NET 8.0 Runtime（または自己完結版を使用）

## クイックスタート

### 方法1：インストーラーをダウンロード

[Releases](https://github.com/Xiaocaihassome/imagemanager/releases)から最新バージョンをダウンロードし、解凍して`ImageManager.exe`を実行してください。

### 方法2：ソースからビルド

```bash
git clone https://github.com/Xiaocaihassome/imagemanager.git
cd ImageManager
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

出力されたEXEは `bin/Release/net8.0-windows/win-x64/publish/ImageManager.exe` にあります。

## チュートリアル

1. 画像をウィンドウにドラッグ、またはCtrl+Vでスクリーンショットを貼り付け
2. 必要な画像にチェックし、「パスをコピー」をクリックしてクリップボードにコピー
3. AIターミナルまたは任意のウィンドウでCtrl+Vを押してパスを貼り付け
4. 「ウィンドウに貼り付け」をクリックすると、パスがフロントウィンドウに自動貼り付け
5. 設定で背景エフェクト、フォント、言語、プライバシーモードを切り替え可能

## よくある質問

**Q: ウィンドウに貼り付けが反応しない？**
A: クリック後2秒以内にターゲットウィンドウに切り替えると、ツールが自動でCtrl+Vを送信します。

**Q: プライバシーモードとは？**
A: 有効にすると、すべてのファイルパスが `********` にマスキングされます。画面共有やプレゼンテーションに最適です。

**Q: 背景エフェクトはどれを選べばいい？**
A: フロストグラスはほとんどのシナリオに適しています。Win11アクリリックはシステム素材を使用し、より透明感があります。なしはソリッドカラー背景です。

**Q: フォント設定はどうすれば有効？**
A: 外観設定でフォント設定をクリックし、フォントとウェイトを選択後「適用」をクリックすると、グローバルに有効になります。

## 技術スタック

- **フレームワーク**: .NET 8 + WPF
- **アーキテクチャ**: MVVM (CommunityToolkit.Mvvm)
- **依存性注入**: Microsoft.Extensions.DependencyInjection
- **背景エフェクト**: DWM Acrylic API（視覚検証付き）/ デスクトップキャプチャ + ガウスぼかし
- **貼り付けシミュレーション**: Win32 SendInput API
- **更新検出**: GitHub Releases API

## 利用規約・免責事項

- 本リポジトリおよび関連ツールは、個人学習、技術交流、非営利テストを目的として公開されています。商業利用にはご使用くださいわないよう作者は要請しています。
- プロジェクトソースコードの正式なライセンス規定はMITライセンスに準拠します。商用利用の場合はコンプライアンスリスクを自己評価してください。
- 本ツールは画像転送管理ツールとしてローカル完結で動作し、オンライン画像リソースを内蔵・ホストしません。ユーザーがインポート、貼り付けたローカル画像、スクリーンショット、プログラム生成の一時ファイルの著作権、コンプライアンス、およびすべての使用結果はユーザー自身の責任となります。
- 本プロジェクトのソースコードやコンパイル済みプログラムの配布、改変、再配布、またはサードパーティの画像ホスティングサービスやオンラインリソースとの併用により、著作権紛争やその他の法的責任が発生した場合、すべての責任はユーザーにあり、プロジェクト開発者は法的責任を負いません。
- 本ツールは「現状」で提供され、操作ミス、ファイル消失、システム互換性の問題などの潜在的リスクについて保証しません。すべての使用リスクはユーザーが負担します。
- 本プロジェクトはAIの広範な支援を受けて開発されたため、微細で気づきにくいプログラム上の問題が残っている可能性があります。ご不便をおかけした場合はご理解をお願いいたします。

## ライセンス

[MIT License](LICENSE)

## 作者

**小蔡有點料 (Xiaocaihassome)** — オープンソースツール開発者

## 謝辞

以下のプロジェクトとリソースに特別感謝します：

- [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/WindowsCommunityToolkit) — MVVM フレームワーク
- [Microsoft.Extensions.DependencyInjection](https://github.com/dotnet/runtime) — 依存性注入
- [Game Icon Pack by Nieobie](https://github.com/Nieobie/Game-Icon-Pack) — アプリ内 UI アイコン
- [Inno Setup](https://jrsoftware.org/isinfo.php) — インストーラー作成ツール

## 公式サイト

https://imagemanager-6gs.pages.dev/

---

# 한국어

<a id="한국어"></a>

# 터미널 이미지 전송 관리 도구

AI 터미널용 경량 이미지 전송 관리 도구입니다. 드래그 앤 드롭, 클립보드 붙여넣기, 경로 복사 및 대상 윈도우에 원클릭 붙여넣기를 지원합니다.

## 기능

- **드래그 앤 드롭** — 윈도우에 이미지를 직접 드롭
- **스크린샷 붙여넣기** — Ctrl+V로 클립보드에서 이미지를 붙여넣기
- **경로 복사** — 이미지를 선택하고 원클릭으로 파일 경로 복사
- **윈도우에 붙여넣기** — 경로를 전면 윈도우에 자동 붙여넣기 (2초 카운트다운으로 대상 전환)
- **배경 효과** — 프로스트 글래스 / Win11 아크릴리ック / 솔리드 컬러 3가지 모드 (미지원 시 자동 전환)
- **폰트 설정** — 폰트 패밀리 및 굵기 커스터마이즈, 라이브 미리보기, 빠른 선택
- **프라이버시 모드** — 주소 마스킹, 민감한 파일 경로 숨겨 노출 방지
- **윈도우 항상 위에** — 윈도우를 항상 다른 윈도우 위에 표시
- **업데이트 확인** — GitHub Releases 새 버전 자동 감지, 원클릭 다운로드 및 설치
- **붙여넣기 후 삭제** — 붙여넣기 시 소스 파일 자동 삭제 (설정에서 활성화)
- **부팅 시 자동 시작** — 시스템 시작 시 자동 실행 (설정에서 활성화)
- **오류 로그** — 이상 자동 기록, AI 수정 프롬프트 원클릭 복사
- **다중 파일 붙여넣기** — 여러 파일 경로를 한 번에 붙여넣기
- **라이트/다크 테마** — 수동 전환 또는 시스템 설정 따르기
- **투명도 조절** — 윈도우 투명도 커스터마이즈 (30% ~ 100%)
- **커스텀 임시 디렉토리** — 이미지 저장 위치 설정 가능
- **단일 인스턴스** — 중복 실행 방지
- **종료 시 자동 정리** — 종료 시 임시 파일 삭제 선택 가능
- **다국어 지원** — 簡体中文、繁體中文、English、日本語、한국어

## 시스템 요구사항

- Windows 10 (build 1809+) 또는 Windows 11
- .NET 8.0 Runtime (또는 self-contained 빌드 사용)

## 빠른 시작

### 방법 1: 설치 파일 다운로드

[Releases](https://github.com/Xiaocaihassome/imagemanager/releases)에서 최신 버전을 다운로드하고, 압축을 풀어 `ImageManager.exe`를 실행하세요.

### 방법 2: 소스에서 빌드

```bash
git clone https://github.com/Xiaocaihassome/imagemanager.git
cd ImageManager
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

출력된 EXE는 `bin/Release/net8.0-windows/win-x64/publish/ImageManager.exe`에 있습니다.

## 튜토리얼

1. 이미지를 윈도우에 드래그하거나 Ctrl+V로 스크린샷을 붙여넣기
2. 필요한 이미지에 체크하고 "경로 복사"를 클릭하여 클립보드에 복사
3. AI 터미널 또는任意の 윈도우에서 Ctrl+V를 눌러 경로를 붙여넣기
4. "윈도우에 붙여넣기"를 클릭하면 경로가 전면 윈도우에 자동 붙여넣기
5. 설정에서 배경 효과, 폰트, 언어, 프라이버시 모드 전환 가능

## 자주 묻는 질문

**Q: 윈도우에 붙여넣기가 반응하지 않아요?**
A: 클릭 후 2초 이내에 대상 윈도우로 전환하면 도구가 자동으로 Ctrl+V를 전송합니다.

**Q: 프라이버시 모드는 무엇인가요?**
A: 활성화하면 모든 파일 경로가 `********`로 마스킹되어 화면 공유나 프레젠테이션에 적합합니다.

**Q: 배경 효과는 어떤 것을 선택해야 하나요?**
A: 프로스트 글래스는 대부분의 시나리오에 적합합니다. Win11 아크릴리ック은 시스템 소재를 사용하여 더 투명합니다. 없음은 솔리드 컬러 배경입니다.

**Q: 폰트 설정은 어떻게 적용되나요?**
A: 외관 설정에서 폰트 설정을 클릭하고 폰트와 굵기를 선택한 후 "적용"을 클릭하면 전역적으로 적용됩니다.

## 기술 스택

- **프레임워크**: .NET 8 + WPF
- **아키텍처**: MVVM (CommunityToolkit.Mvvm)
- **의존성 주입**: Microsoft.Extensions.DependencyInjection
- **배경 효과**: DWM Acrylic API (시각적 검증 포함) / 데스크톱 캡처 + 가우시안 블러
- **붙여넣기 시뮬레이션**: Win32 SendInput API
- **업데이트 감지**: GitHub Releases API

## 이용약관 및 면책조항

- 본 리포지토리 및 관련 도구는 개인 학습, 기술 교류, 비영리 테스트를 목적으로 공개되었습니다. 저자는 상업적 이익을 위해 직접 사용하지 않기를 요청하고 있습니다.
- 프로젝트 소스 코드의 공식 라이선스 규정은 MIT 라이선스를 따릅니다. 상업적 사용의 경우 컴플라이언스 리스크를 자체 평가해 주세요.
- 본 도구는 이미지 전송 관리 도구로서 로컬에서 완전히 작동하며, 온라인 이미지 리소스를 포함하거나 호스팅하지 않습니다. 사용자가 가져오거나 붙여넣은 로컬 이미지, 스크린샷 및 프로그램이 생성한 임시 파일의 저작권, 컴플라이언스 및 모든 사용 결과는 사용자가 전적으로 책임을 집니다.
- 본 프로젝트의 소스 코드나 컴파일된 프로그램의 배포, 수정, 재배포, 또는 서드파티 이미지 호스팅 서비스나 온라인 리소스와의 병용으로 인해 저작권 분쟁이나 기타 법적 책임이 발생한 경우, 모든 책임은 사용자에게 있으며 프로젝트 개발자는 법적 책임을 지지 않습니다.
- 본 도구는 "있는 그대로" 제공되며, 작업 실수, 파일 손실, 시스템 호환성 문제 등 잠재적 위험에 대해 보증하지 않습니다. 모든 사용 위험은 사용자가 부담합니다.
- 본 프로젝트는 AI의 광범위한 지원을 받아 개발되었으므로, 미세하고 눈에 띄기 어려운 프로그램 문제가 남아 있을 수 있습니다. 불편을 끼쳐 드린 점 양해 부탁드립니다.

## 라이선스

[MIT License](LICENSE)

## 저자

**小蔡有點料 (Xiaocaihassome)** — 오픈소스 도구 개발자

## 감사의 글

다음 프로젝트와 리소스에 특별히 감사드립니다:

- [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/WindowsCommunityToolkit) — MVVM 프레임워크
- [Microsoft.Extensions.DependencyInjection](https://github.com/dotnet/runtime) — 의존성 주입
- [Game Icon Pack by Nieobie](https://github.com/Nieobie/Game-Icon-Pack) — 앱 내 UI 아이콘
- [Inno Setup](https://jrsoftware.org/isinfo.php) — 설치 프로그램 제작 도구

## 공식 사이트

https://imagemanager-6gs.pages.dev/
