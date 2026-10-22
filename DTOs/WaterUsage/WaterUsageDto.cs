using System;

namespace NWSDB.API.DTOs.WaterUsage
{
    public class WaterUsageDto
    {
        public string ReadingMonth { get; set; } = string.Empty;
        public int PreviousReading { get; set; }
        public int CurrentReading { get; set; }
        public int UnitsConsumed { get; set; }
        public DateTime RecordedDate { get; set; }
    }
}
