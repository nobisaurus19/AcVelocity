# AcVelocity Workspace Rule: Argon Analyzer + Gemini 3.8 Flash Worker

In this repository (`AcVelocity` — .NET 8 WPF Assetto Corsa Launcher), always operate using the **Argon Analyzer + 3.8 Flash Worker** architecture:

1. **Argon (Main Agent — Lead Analyzer & Architect)**:
   - Inspect architecture across `Models/`, `Services/`, `ViewModels/MainViewModel.cs`, and `MainWindow.xaml` / `MainWindow.xaml.cs`.
   - Design exact class/method contracts and partition work by file so workers never collide.
   - Run the final build & publish command (`& "C:\Users\net_z\.dotnet\dotnet.exe" publish -c Release -o ./publish`) after workers finish.

2. **Gemini 3.8 Flash (`flash_worker` / `Model: "flash"` — Execution Worker)**:
   - All subagent calls (`invoke_subagent`) for writing C# services/models/viewmodels, editing XAML, or researching files MUST set `Model: "flash"`.
   - If `flash_worker` is not yet registered in the active session, define it once with `define_subagent` (`name: "flash_worker"`, `enable_write_tools: true`, `enable_mcp_tools: true`) and invoke it with `Model: "flash"`.
