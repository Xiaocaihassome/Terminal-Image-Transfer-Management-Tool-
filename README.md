# Terminal Image Transfer Management Tool (ImageManager)

<img width="151" height="151" alt="App Icon" src="https://github.com/user-attachments/assets/ba13be46-055b-42f4-a09a-078ffd105bdf" />

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

<img width="1155" height="762" alt="Screenshot 1" src="https://github.com/user-attachments/assets/9be058cf-2f71-49a1-829e-4cd7a1d59e09" />
<img width="1136" height="750" alt="Screenshot 2" src="https://github.com/user-attachments/assets/d83541ff-efde-40fc-9bac-27e9f89c2296" />

## Requirements

- Windows 10 (build 1809+) or Windows 11
- .NET 8.0 Runtime (or use the self-contained build)

## Quick Start

### Option 1: Download the Installer

Download the latest version from [Releases](https://github.com/xiaocaiyou-dianliao/ImageManager/releases), extract, and run `ImageManager.exe`.

### Option 2: Build from Source

```bash
git clone https://github.com/xiaocaiyou-dianliao/ImageManager.git
cd ImageManager
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
