# AcVelocity Workspace Rule: Argon Analyzer + Gemini 3.8 Flash Worker

In this repository (`AcVelocity` — .NET 8 WPF Assetto Corsa Launcher), always operate using the **Argon Analyzer + 3.8 Flash Worker** architecture:

1. **Argon (Main Agent — Lead Analyzer & Architect)**:
   - Inspect architecture across `Models/`, `Services/`, `ViewModels/MainViewModel.cs`, and `MainWindow.xaml` / `MainWindow.xaml.cs`.
   - Design exact class/method contracts and partition work by file so workers never collide.
   - Run the final build & publish command (`& "C:\Users\net_z\.dotnet\dotnet.exe" publish -c Release -o ./publish`) after workers finish.

2. **Gemini 3.8 Flash (`Model: "flash"` — Execution Worker)**:
   - For writing/editing C# services, models, viewmodels, or XAML, invoke `TypeName: "self"` with `Model: "flash"` so the 3.8 Flash worker inherits `write_to_file` and `replace_file_content`.
   - For read-only codebase/file inspection, invoke `TypeName: "research"` with `Model: "flash"`.
