Using WinRT APIs from a C# Desktop App
======================================

1. Right click in project file and click **Unload Project**.
2. Right click in project file and click **Edit ConsoleApp.csproj**
3. Add `<TargetPlatformVersion>8.0</TargetPlatformVersion>` within `<PropertyGroup>`.
4. Right click in project file and click **Reload Project**.
5. Select **Add Reference...**.
6. Add the **Windows** library located in **Windows > Core**.
7. To handle WinRT events and async methods, add **System.Runtime.WindowsRuntime.dll** library located at **C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETCore\v4.5** as a reference.

Source: [Using Windows 8 WinRT APIs in .NET Desktop Applications](http://blogs.msdn.com/b/cdndevs/archive/2013/10/02/using-windows-8-winrt-apis-in-net-desktop-applications.aspx)
