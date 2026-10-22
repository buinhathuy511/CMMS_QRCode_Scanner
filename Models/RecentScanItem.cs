using System;

namespace CMMS_QRCode_Scanner.Models
{
    public class RecentScanItem
    {
        public string BarcodeId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime ScannedAt { get; set; }
        
        public string TimeAgo
        {
            get
            {
                var span = DateTime.Now - ScannedAt;
                if (span.TotalMinutes < 1) return "Just now";
                if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} mins ago";
                if (span.TotalHours < 24) return $"{(int)span.TotalHours} hours ago";
                return $"{(int)span.TotalDays} days ago";
            }
        }
    }
}
