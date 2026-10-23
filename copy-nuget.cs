#:property OutputPath=./output

using System.Text;

var argPath = args?.FirstOrDefault();
if (string.IsNullOrEmpty(argPath) || argPath == "-t" || argPath == "--target")
{
    argPath = Environment.CurrentDirectory;
}

var targetPath = "";
var isTargetPath = false;
foreach (var para in args ?? [])
{
    if (isTargetPath)
    {
        targetPath = para;
        break;
    }
    else if (para == "-t" || para == "--target")
    {
        isTargetPath = true;
    }
}

if (string.IsNullOrEmpty(targetPath))
{
    Console.WriteLine("Need point the target directory by \"-t\" or \"--target\"!\r\nExample: \"copy-nuget D:\\Projects\\packages -t D:\\Projects\\nugets\"");
    return;
}

Log($"Woring dir: {argPath}...");
if (!Directory.Exists(argPath))
{
    Log($"Dir \"{argPath}\" not exist!", LogLevel.Error);
    return;
}

var copyToDir = "D:\\Projects\\copied";
if (!Directory.Exists(copyToDir))
{
    Directory.CreateDirectory(copyToDir);
}

int count = 0;
try
{
    var nugetList = GetNugetFile(argPath).ToArray();

    foreach (var file in nugetList)
    {
        var target = Path.Combine(copyToDir, Path.GetFileName(file));
        Log($"Copy \"{file}\" to \"{target}\"...");
        File.Copy(file, target, true);
        count += 1;
    }
}
catch (Exception ex)
{
    Log($"Oops! Looks like occur some error: \r\n{ex}", LogLevel.Error);
    return;
}

Log($"Completed! Copy \"{count}\" files!");

IEnumerable<string> GetNugetFile(string dir)
{
    foreach (var file in Directory.GetFiles(dir))
    {
        if (file.EndsWith(".nupkg", StringComparison.OrdinalIgnoreCase)
            && File.Exists(file))
        {
            yield return file;
        }
    }

    foreach (var subDir in Directory.GetDirectories(dir))
    {
        if (Directory.Exists(subDir))
        {
            foreach (var subFile in GetNugetFile(subDir))
            {
                yield return subFile;
            }
        }
    }
}

void Log(string content, LogLevel level = LogLevel.Info)
{
    StringBuilder str = new StringBuilder();
    str.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff "));
    str.Append(level switch
    {
        LogLevel.Warning => "[WARN] ",
        LogLevel.Error => "[ERR] ",
        _ => "[INFO] "
    });

    str.Append(content);

    Console.WriteLine(str.ToString());
}

enum LogLevel
{
    Info,
    Warning,
    Error
}
