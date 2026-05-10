using System.Text;
using System.Text.Json;

namespace FileAnalyzerSandbox.Sandbox;

public class SandboxAnalyzer
{
    private readonly string _sandboxPath;
    private readonly Dictionary<string, List<string>> _threatSignatures;

    public SandboxAnalyzer()
    {
        _sandboxPath = Path.Combine(Path.GetTempPath(), "FileAnalyzerSandbox", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_sandboxPath);

        // Initialize threat signatures
        _threatSignatures = new Dictionary<string, List<string>>
        {
            ["Malicious Script"] = new() { "eval(", "document.write(", "alert(", "onload=" },
            ["Potential Virus"] = new() { "MZ", "This program cannot be run in DOS mode" },
            ["Suspicious Macro"] = new() { "AutoOpen", "Document_Open", "Shell", "WScript.Shell" },
            ["Ransomware Indicator"] = new() { "encrypt", "decrypt", "ransom", "bitcoin" }
        };
    }

    public async Task SaveFileAsync(string fileName, byte[] content)
    {
        var filePath = Path.Combine(_sandboxPath, fileName);
        await File.WriteAllBytesAsync(filePath, content);
    }

    public async Task<AnalysisResult> AnalyzeFileAsync(string fileName)
    {
        var filePath = Path.Combine(_sandboxPath, fileName);

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File {fileName} not found in sandbox");

        var content = await File.ReadAllBytesAsync(filePath);
        var textContent = Encoding.UTF8.GetString(content);

        var threats = new List<string>();

        foreach (var signature in _threatSignatures)
        {
            foreach (var pattern in signature.Value)
            {
                if (textContent.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                {
                    threats.Add($"{signature.Key}: found pattern '{pattern}'");
                }
            }
        }

        // Additional analysis
        var fileInfo = new FileInfo(filePath);
        var result = new AnalysisResult
        {
            FileName = fileName,
            FileSize = fileInfo.Length,
            IsMalicious = threats.Any(),
            Threats = threats,
            ThreatLevel = threats.Any() ?
                (threats.Count > 3 ? "Critical" : threats.Count > 1 ? "High" : "Medium") :
                "Safe",
            AnalysisDetails = new
            {
                FileExtension = Path.GetExtension(fileName),
                FileHash = ComputeHash(content),
                ScanTimestamp = DateTime.UtcNow,
                ThreatCount = threats.Count
            }
        };

        return result;
    }

    public async Task DeleteFileAsync(string fileName)
    {
        var filePath = Path.Combine(_sandboxPath, fileName);
        if (File.Exists(filePath))
        {
            await Task.Run(() => File.Delete(filePath));
        }
    }

    private string ComputeHash(byte[] content)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hash = sha256.ComputeHash(content);
        return Convert.ToHexString(hash).ToLower();
    }

    public void Cleanup()
    {
        if (Directory.Exists(_sandboxPath))
        {
            Directory.Delete(_sandboxPath, true);
        }
    }
}

public class AnalysisResult
{
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public bool IsMalicious { get; set; }
    public List<string> Threats { get; set; } = new();
    public string ThreatLevel { get; set; } = string.Empty;
    public object AnalysisDetails { get; set; } = new();
}