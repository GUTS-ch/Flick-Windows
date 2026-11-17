# Flick for Windows

Flick 是一个桌面 AI 助手。选中文字后按 **Ctrl+Shift+E**，选择解释、总结、翻译、润色或续写。

## 功能
- Windows 全局快捷键：Ctrl+Shift+E
- 读取当前选中的文本
- 内置五种常用提示词
- 兼容 OpenAI Chat Completions API，可配置 API 地址、密钥与模型
- 设置保存在 `%LocalAppData%\\Flick\\settings.json`

## 使用方法
1. 下载 `sha256:5d719c816639f674c5be0d1a1f37e19079bfe48c01fb8a86f6b092bc67788980` 并运行。
2. 点击“设置”，填写 API Base URL、API Key 和模型名称。
3. 在任意应用选中文字，按 Ctrl+Shift+E，然后选择操作。

## 构建
需要 .NET 8 SDK 和 Windows 桌面开发支持：

```powershell
dotnet publish FlickWindows.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

## 项目来源
本项目 Windows 版本参考 [think2do/Flick](https://github.com/think2do/Flick) 的介绍与功能设计。原项目为 macOS 应用。本仓库为独立的 Windows 移植版本。
