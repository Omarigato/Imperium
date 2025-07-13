using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imperium.Service.DTOs.File
{
    /// <summary>
    /// DTO статистики файлов
    /// </summary>
    public class FileStatisticsDto
    {
        public int TotalFiles { get; set; }
        public int AttachedFiles { get; set; }
        public int OrphanFiles { get; set; }
        public long TotalSizeBytes { get; set; }
        public double AverageFileSizeMB => TotalFiles > 0 ? TotalSizeBytes / (1024.0 * 1024.0) / TotalFiles : 0;
        public string TotalSizeFormatted => FormatFileSize(TotalSizeBytes);
        public DateTime? OldestFileDate { get; set; }
        public DateTime? NewestFileDate { get; set; }
        public Dictionary<string, int> FileTypeDistribution { get; set; } = new();

        private static string FormatFileSize(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
            if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024.0 * 1024.0):F1} MB";
            return $"{bytes / (1024.0 * 1024.0 * 1024.0):F1} GB";
        }
    }
}
