using System.Runtime.InteropServices;
using System.Text.Json;

var appInfo = new
{
    Student = "Федун Тарас, група ФЕІ-35",
    OSDescription = RuntimeInformation.OSDescription,
    OSEnvironment = Environment.OSVersion.ToString(),
    Architecture = RuntimeInformation.ProcessArchitecture.ToString(),
    DotNetVersion = Environment.Version.ToString(),
    Runtime = RuntimeInformation.FrameworkDescription,
    AppDirectory = AppContext.BaseDirectory,
    CurrentDirectory = Environment.CurrentDirectory,
    Domain = "Склад (товари, партії, залишки, переміщення)"
};

if (args.Contains("--json"))
{
    Console.WriteLine(JsonSerializer.Serialize(appInfo));
}
else
{
    Console.WriteLine("CrossApp — практикум з крос-платформного програмування");
    Console.WriteLine($"Студент: {appInfo.Student}");
    Console.WriteLine(new string('-', 52));
    Console.WriteLine($"ОС (OSDescription): {appInfo.OSDescription}");
    Console.WriteLine($"ОС (Environment)  : {appInfo.OSEnvironment}");
    Console.WriteLine($"Архітектура процесу: {appInfo.Architecture}");
    Console.WriteLine($"Версія .NET (CLR): {appInfo.DotNetVersion}");
    Console.WriteLine($"Runtime           : {appInfo.Runtime}");
    Console.WriteLine($"Каталог застосунку: {appInfo.AppDirectory}");
    Console.WriteLine($"Поточний каталог  : {appInfo.CurrentDirectory}");
    Console.WriteLine(new string('-', 52));
    Console.WriteLine($"Предметна область: {appInfo.Domain}");
}