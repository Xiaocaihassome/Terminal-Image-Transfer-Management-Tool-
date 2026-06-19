# 终端图片中转管理工具

[English](#english) | [繁體中文](#繁體中文) | [日本語](#日本語) | [한국어](#한국어)

<img width="151" height="151" alt="App Icon" src="https://github.com/user-attachments/assets/ba13be46-055b-42f4-a09a-078ffd105bdf" />

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

<img width="1155" height="762" alt="Screenshot 1" src="https://github.com/user-attachments/assets/9be058cf-2f71-49a1-829e-4cd7a1d59e09" />
<img width="1136" height="750" alt="Screenshot 2" src="https://github.com/user-attachments/assets/d83541ff-efde-40fc-9bac-27e9f89c2296" />

## 环境要求

- Windows 10 (build 1809+) 或 Windows 11
- .NET 8.0 Runtime（或使用独立发布版本）

## 快速开始

### 方式一：下载独立版本

从 [Releases](https://github.com/Xiaocaihassome/Terminal-Image-Transfer-Management-Tool-/releases) 下载最新版本，解压后直接运行 `ImageManager.exe`。

### 方式二：从源码构建

```bash
git clone https://github.com/Xiaocaihassome/Terminal-Image-Transfer-Management-Tool-.git
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
- 本工具按"现状"提供，不对使用过程中的操作失误、文件丢失、系统兼容问题等潜在风险提供担保，所有使用风险由使用者自行承担。
- 本项目在 AI 的广泛协助下开发，因此仍可能存在细微或不易察觉的程序问题。若给您带来不便，敬请理解。

## 许可证

[MIT License](LICENSE)

## 作者

**小蔡有点料** — 开源工具开发者

---

# English

<a id="english"></a>

A lightweight image transfer management tool designed for AI terminals. Supports drag & drop, clipboard paste, path copying, and one-click paste to target windows.

## Features

- **Drag & Drop** — Drop images directly into the window
- **Paste Screenshots** — Ctrl+V to paste images from clipboard
- **Copy Paths** — Select images and copy file paths with one click
- **Paste to Window** — Automatically paste the path to the foreground window (2-second countdown to switch target)
- **Frosted Glass Effect** — Real-time desktop blur background for an elegant visual experience
- **Light / Dark Theme** — Manual switching or follow system preference
- **Transparency Control** — Customizable window opacity (30% ~ 100%)
- **Custom Temp Directory** — Configure where images are stored
- **Single Instance** — Prevents duplicate instances
- **Auto Cleanup on Exit** — Optionally clear temp files when closing

## Requirements

- Windows 10 (build 1809+) or Windows 11
- .NET 8.0 Runtime (or use the self-contained build)

## Quick Start

### Option 1: Download the Installer

Download the latest version from [Releases](https://github.com/Xiaocaihassome/Terminal-Image-Transfer-Management-Tool-/releases), extract, and run `ImageManager.exe`.

### Option 2: Build from Source

```bash
git clone https://github.com/Xiaocaihassome/Terminal-Image-Transfer-Management-Tool-.git
cd ImageManager
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

The output EXE is at `bin/Release/net8.0-windows/win-x64/publish/ImageManager.exe`.

## Usage

1. **Add Images**: Drag images into the window, or press Ctrl+V to paste a screenshot
2. **Select Images**: Check the images you need (Select All supported)
3. **Copy Path**: Click the "Copy Path" button
4. **Paste to Window**: Click "Paste to Window", switch to the target window within 2 seconds, and Ctrl+V is sent automatically

## Tech Stack

- **Framework**: .NET 8 + WPF
- **Architecture**: MVVM (CommunityToolkit.Mvvm)
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **Frosted Glass**: Desktop capture + Gaussian blur
- **Paste Simulation**: Win32 SendInput API

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

---

# 繁體中文

<a id="繁體中文"></a>

一款專為 AI 終端設計的輕量級圖片中轉管理工具，支援拖曳、貼上、路徑複製和一鍵貼上到目標視窗。

## 功能特性

- **拖曳新增** — 直接拖入圖片到視窗
- **貼上截圖** — Ctrl+V 貼上剪貼簿中的圖片
- **複製路徑** — 勾選圖片後一鍵複製檔案路徑
- **貼上到視窗** — 自動將路徑貼上到前景視窗（2秒倒數計時切換目標視窗）
- **毛玻璃效果** — 視窗背景即時模糊桌面，視覺效果優雅
- **深色/淺色主題** — 支援手動切換或跟隨系統
- **透明度調節** — 自訂視窗透明度（30% ~ 100%）
- **自訂暫存目錄** — 可設定圖片儲存位置
- **單一執行個體** — 防止重複開啟
- **離開自動清理** — 選擇關閉時清空暫存檔案

## 環境需求

- Windows 10 (build 1809+) 或 Windows 11
- .NET 8.0 Runtime（或使用獨立發佈版本）

## 快速開始

### 方式一：下載獨立版本

從 [Releases](https://github.com/Xiaocaihassome/Terminal-Image-Transfer-Management-Tool-/releases) 下載最新版本，解壓縮後直接執行 `ImageManager.exe`。

### 方式二：從原始碼建置

```bash
git clone https://github.com/Xiaocaihassome/Terminal-Image-Transfer-Management-Tool-.git
cd ImageManager
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

產生的 EXE 位於 `bin/Release/net8.0-windows/win-x64/publish/ImageManager.exe`。

## 使用方法

1. **新增圖片**：拖曳圖片到視窗，或按 Ctrl+V 貼上截圖
2. **選取圖片**：勾選需要的圖片（支援全選）
3. **複製路徑**：點擊「複製路徑」按鈕
4. **貼上到視窗**：點擊「貼上到視窗」，在2秒內切換到目標視窗，自動發送 Ctrl+V

## 技術架構

- **框架**: .NET 8 + WPF
- **架構**: MVVM（CommunityToolkit.Mvvm）
- **依賴注入**: Microsoft.Extensions.DependencyInjection
- **毛玻璃效果**: WPF 桌面擷取 + 高斯模糊
- **貼上模擬**: Win32 SendInput API

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

---

# 日本語

<a id="日本語"></a>

AIターミナル向けの軽量画像転送管理ツール。ドラッグ＆ドロップ、クリップボード貼り付け、パスコピー、ターゲットウィンドウへのワンクリック貼り付けに対応しています。

## 機能

- **ドラッグ＆ドロップ** — ウィンドウに画像を直接ドロップ
- **スクリーンショット貼り付け** — Ctrl+Vでクリップボードから画像を貼り付け
- **パスコピー** — 画像を選択してワンクリックでファイルパスをコピー
- **ウィンドウに貼り付け** — パスをフロントウィンドウに自動貼り付け（2秒カウントダウンでターゲット切替）
- **フロストグラス効果** — ウィンドウ背景でリアルタイムにデスクトップをぼかし、エレガントなビジュアル体験
- **ライト/ダークテーマ** — 手動切替またはシステム設定に追従
- **透過度調整** — ウィンドウの透過度をカスタマイズ（30% ~ 100%）
- **カスタム一時ディレクトリ** — 画像の保存先を設定可能
- **シングルインスタンス** — 重複起動を防止
- **終了時自動クリーンアップ** — 閉じる際に一時ファイルを削除可能

## システム要件

- Windows 10 (build 1809+) または Windows 11
- .NET 8.0 Runtime（または自己完結版を使用）

## クイックスタート

### 方法1：インストーラーをダウンロード

[Releases](https://github.com/Xiaocaihassome/Terminal-Image-Transfer-Management-Tool-/releases)から最新バージョンをダウンロードし、解凍して`ImageManager.exe`を実行してください。

### 方法2：ソースからビルド

```bash
git clone https://github.com/Xiaocaihassome/Terminal-Image-Transfer-Management-Tool-.git
cd ImageManager
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

出力されたEXEは `bin/Release/net8.0-windows/win-x64/publish/ImageManager.exe` にあります。

## 使い方

1. **画像を追加**: 画像をウィンドウにドラッグ、またはCtrl+Vでスクリーンショットを貼り付け
2. **画像を選択**: 必要な画像にチェック（全選択対応）
3. **パスをコピー**: 「パスをコピー」ボタンをクリック
4. **ウィンドウに貼り付け**: 「ウィンドウに貼り付け」をクリックし、2秒以内にターゲットウィンドウに切り替えるとCtrl+Vが自動送信される

## 技術スタック

- **フレームワーク**: .NET 8 + WPF
- **アーキテクチャ**: MVVM (CommunityToolkit.Mvvm)
- **依存性注入**: Microsoft.Extensions.DependencyInjection
- **フロストグラス**: デスクトップキャプチャ + ガウスぼかし
- **貼り付けシミュレーション**: Win32 SendInput API

## 利用規約・免責事項

- 本リポジトリおよび関連ツールは、個人学習、技術交流、非営利テストを目的として公開されています。商業利用にはご使用くださいわないよう作者は要請しています。
- プロジェクトソースコードの正式なライセンス規定はMITライセンスに準拠します。商用利用の場合はコンプライアンスリスクを自己評価してください。
- 本ツールは画像転送管理ツールとしてローカル完結で動作し、オンライン画像リソースを内蔵・ホストしません。ユーザーがインポート、貼り付けたローカル画像、スクリーンショット、プログラム生成の一時ファイルの著作権、コンプライアンス、およびすべての使用結果はユーザー自身の責任となります。
- 本プロジェクトのソースコードやコンパイル済みプログラムの配布、改変、再配布、またはサードパーティの画像ホスティングサービスやオンラインリソースとの併用により、著作権紛争やその他の法的責任が発生した場合、すべての責任はユーザーにあり、プロジェクト開発者は法的責任を負いません。
- 本ツールは「現状」で提供され、操作ミス、ファイル消失、システム互換性の問題などの潜在的リスクについて保証しません。すべての使用リスクはユーザーが負担します。
- 本プロジェクトはAIの広範な支援を受けて開発されたため、微細で気づきにくいプログラム上の問題が残っている可能性があります。ご不便をおかけした場合はご理解をお願いいたします。

##许可证

[MIT许可证](许可证)

## 作者

**小蔡有點料 (Xiaocaihassome)** — オープンソースツール開発者

---

# 한국어

<a id="한국어"></a>

AI 터미널용 경량 이미지 전송 관리 도구입니다. 드래그 앤 드롭, 클립보드 붙여넣기, 경로 복사 및 대상 윈도우에 원클릭 붙여넣기를 지원합니다.

## 기능

- **拖放**— 直接将图像拖放到窗口中
- **스크린샷 붙여넣기** — Ctrl+V로 클립보드에서 이미지를 붙여넣기
- **경로 복사** — 이미지를 선택하고 원클릭으로 파일 경로 복사
- **粘贴到窗口**— 将路径自动粘贴到当前窗口（通过2秒倒计时切换目标）
- **玻璃效果**— 通过实时桌面模糊效果打造优雅的窗口背景视觉体验
- **라이트/다크 테마** — 수동 전환 또는 시스템 설정 따르기
- **투명도 조절** — 윈도우 투명도 커스터마이즈 (30% ~ 100%)
- **커스텀 임시 디렉토리** — 이미지 저장 위치 설정 가능
- **单实例**— 防止重复运行
- **종료 시 자동 정리** — 종료 시 임시 파일 삭제 선택 가능

##系统要求

- Windows 10 (build 1809+) 또는 Windows 11
-.NET 8.0 运行时（或使用自包含构建）

##快速入门

###方法1：下载安装文件

[版本](https://github.com/Xiaocaihassome/Terminal-Image-Transfer-Management-Tool-/releases)从下载最新版本，并解压`ImageManager.exe`后运行

###方法2：从源码构建

```bash
git clone https://github.com/Xiaocaihassome/Terminal-Image-Transfer-Management-Tool-.git
cd ImageManager
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

生成的EXE文件位于`bin/Release/net8.0-windows/win-x64/publish/ImageManager.exe``

##使用方法

1. **添加图片**: 将图片拖拽至窗口，或使用Ctrl+V粘贴截图
2. **选择图片**: 勾选所需图片（支持全选）
**: 点击“复制路径”按钮
4. **粘贴到窗口**: 点击“粘贴到窗口”后，在2秒内切换到目标窗口，即可自动发送Ctrl+V

##技术栈

- **框架**: .NET 8 + WPF
- **架构**: MVVM（CommunityToolkit.Mvvm）
- **依赖注入**: Microsoft.Extensions.DependencyInjection
- **玻璃效果**: 桌面捕获 + 高斯模糊
**: Win32 SendInput API

##使用条款及免责声明

-本仓库及相关工具仅用于个人学习、技术交流及非商业性测试目的。作者恳请勿将其用于任何商业用途。
-项目源代码的官方许可协议遵循MIT许可证。如用于商业用途，请自行评估合规风险。
-本工具作为图像传输管理工具，完全在本地运行，不包含或托管任何在线图像资源。用户导入或粘贴的本地图像、截图以及程序生成的临时文件的版权、合规性及一切使用后果，均由用户自行承担全部责任。
-因本项目的源代码或编译后程序的分发、修改、再分发，或与第三方镜像托管服务及在线资源的结合使用而引发的著作权纠纷或其他法律责任，均由用户自行承担，项目开发者不承担任何法律责任。
-本工具按“原样”提供，不就操作失误、文件丢失、系统兼容性问题等潜在风险作出任何保证。所有使用风险均由用户自行承担。
-本项目在人工智能的广泛支持下开发，因此可能仍存在一些细微且不易察觉的程序问题。由此给您带来的不便，敬请谅解。

##许可证

[MIT许可证](许可证)

##作者

**小蔡有点料 (Xiaocaihassome)**— 开源工具开发者
