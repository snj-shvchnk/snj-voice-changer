namespace SnjVoiceChanger;

public sealed class VstPluginScanner
{
    public IReadOnlyList<VstPluginCandidate> Scan(string folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
        {
            return [];
        }

        var plugins = Directory
            .EnumerateFileSystemEntries(folderPath, "*.vst3", SearchOption.AllDirectories)
            .Where(path =>
                File.Exists(path) ||
                Directory.Exists(path))
            .Select(path => new VstPluginCandidate(path, System.IO.Path.GetFileNameWithoutExtension(path)))
            .OrderBy(plugin => plugin.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return plugins;
    }
}
