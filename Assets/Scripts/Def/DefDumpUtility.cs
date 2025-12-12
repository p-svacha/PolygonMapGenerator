// Assets/Scripts/DefDumpUtility.cs
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public static class DefDumpUtility
{
    /// <summary>
    /// Finds every "*Defs.cs" under Assets/, extracts the "Defs => new List<...> { ... }" block,
    /// and writes them all into one timestamped text file.
    /// </summary>
    public static void DumpAllDefs()
    {
        // make sure target folder exists
        var folder = Path.Combine(Application.dataPath, "Documentation");
        Directory.CreateDirectory(folder);

        // timestamp so old dumps aren’t overwritten
        var filename = $"AllDefs.txt";
        var fullPath = Path.Combine(folder, filename);

        using var writer = new StreamWriter(fullPath, false, Encoding.UTF8);
        writer.WriteLine("=== SOURCE DEF LISTS ===\n");

        DumpDefSourceDefinitions(writer);

        Debug.Log($"[DefDumpUtility] Source‐defs dump written to: {fullPath}");
    }

    /// <summary>
    /// Scans all *Defs.cs files and pulls out the static "Defs => new List<...> { … };" initializer block.
    /// </summary>
    private static void DumpDefSourceDefinitions(StreamWriter writer)
    {
        // scan your Assets folder
        var defsFiles = Directory.GetFiles(Application.dataPath, "*Defs.cs", SearchOption.AllDirectories);

        // regex to find the Defs property
        var propPattern = new Regex(@"public\s+static\s+List<\w+>\s+Defs\s*=>", RegexOptions.Compiled);

        foreach (var file in defsFiles.OrderBy(f => f))
        {
            var lines = File.ReadAllLines(file);
            var sb = new StringBuilder();
            bool inBlock = false;
            int braceDepth = 0;

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (!inBlock)
                {
                    if (propPattern.IsMatch(line))
                    {
                        inBlock = true;
                        sb.AppendLine(line);
                        braceDepth += Count(line, '{') - Count(line, '}');
                    }
                }
                else
                {
                    sb.AppendLine(line);
                    braceDepth += Count(line, '{') - Count(line, '}');
                    if (braceDepth <= 0)
                    {
                        // end of initializer
                        break;
                    }
                }
            }

            if (sb.Length > 0)
            {
                writer.WriteLine($"--- {Path.GetFileName(file)} ---");
                writer.Write(sb.ToString());
                writer.WriteLine();
            }
        }
    }

    private static int Count(string s, char c) => s.Count(ch => ch == c);
}
