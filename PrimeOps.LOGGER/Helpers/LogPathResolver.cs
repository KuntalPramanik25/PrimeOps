using PrimeOps.LOGGER.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PrimeOps.LOGGER.Helpers
{
    public static class LogPathResolver
    {
        /// <summary>
        /// Resolves: {basePath}/Logs/{yyyy}/{MM}/{dd}/{level}.txt
        /// Supports file segmentation when size exceeds limit: info_001.txt, info_002.txt ...
        /// </summary>
        public static string Resolve(string basePath, LogLevel level, DateTime date, int maxFileSizeMb)
        {
            var folder = Path.Combine(basePath, "Logs", date.Year.ToString(), date.Month.ToString("D2"),date.Day.ToString("D2"));

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var levelName = level.ToString().ToLower();
            var filePath = Path.Combine(folder, $"{levelName}.txt");

            // File segmentation
            if (File.Exists(filePath))
            {
                var info = new FileInfo(filePath);
                if (info.Length >= maxFileSizeMb * 1024L * 1024L)
                {
                    int segment = 2;

                    while (File.Exists(Path.Combine(folder, $"{levelName}_{segment:D3}.txt")))
                        segment++;

                    filePath = Path.Combine(folder, $"{levelName}_{segment:D3}.txt");
                }
            }

            return filePath;
        }

        /// <summary>Returns all date folders older than retentionMonths.</summary>
        public static IEnumerable<string> GetExpiredDayFolders(string basePath, int retentionMonths)
        {
            var logsRoot = Path.Combine(basePath, "Logs");

            if (!Directory.Exists(logsRoot)) 
                yield break;

            var cutoff = DateTime.Now.AddMonths(-retentionMonths);

            // Walk Logs/{yyyy}/{MM}/{dd}
            foreach (var yearDir in Directory.GetDirectories(logsRoot))
            {
                if (!int.TryParse(Path.GetFileName(yearDir), out int year)) 
                    continue;

                foreach (var monthDir in Directory.GetDirectories(yearDir))
                {
                    if (!int.TryParse(Path.GetFileName(monthDir), out int month)) 
                        continue;

                    foreach (var dayDir in Directory.GetDirectories(monthDir))
                    {
                        if (!int.TryParse(Path.GetFileName(dayDir), out int day)) 
                            continue;
                        var folderDate = new DateTime(year, month, day);

                        if (folderDate < cutoff)
                            yield return dayDir;
                    }
                }
            }
        }
    }
}
