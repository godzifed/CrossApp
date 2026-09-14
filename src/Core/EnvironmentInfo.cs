using System.Runtime.InteropServices;

namespace Core;

// record відповідає за зберігання даних. Це тип посилання з рівністю за значенням.
public sealed record EnvironmentReport(
    string OsDescription,
    string FrameworkDescription,
    string ProcessArchitecture,
    string DetectedRid,
    string ReportedRid,
    string BaseDirectory,
    string BuildNote); // Додано для додаткового завдання

// static class відповідає за поведінку та алгоритми збору даних.
public static class EnvironmentInfo
{
    public static EnvironmentReport Collect() => new(
        RuntimeInformation.OSDescription,
        RuntimeInformation.FrameworkDescription,
        RuntimeInformation.ProcessArchitecture.ToString(),
        DetectRid(),
        RuntimeInformation.RuntimeIdentifier,
        AppContext.BaseDirectory,
        GetBuildNote());

    // Тернарний оператор (умова ? true : false) та switch вираз для визначення ОС і архітектури.
    private static string DetectRid()
    {
        string os =
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "win" :
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "linux" :
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "osx" : "unknown";

        string arch = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.X86 => "x86",
            Architecture.Arm64 => "arm64",
            Architecture.Arm => "arm",
            _ => "unknown" // Гілка за замовчуванням (discard)
        };

        return $"{os}-{arch}";
    }

    // Умовна компіляція для перевірки Multi-targeting (Додаткове завдання)
    private static string GetBuildNote()
    {
#if NET10_0_OR_GREATER
        return "збірка під net10.0";
#else
        return "збірка під net8.0";
#endif
    }
}