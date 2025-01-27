﻿using Dapper.Contrib.Extensions;
using System;

namespace MyGeotabAPIAdapter.Database.Models
{
    [Table("ExceptionEvents")]
    public class DbExceptionEvent
    {
        [Key]
        public long id { get; set; }
        public string GeotabId { get; set; }
        public DateTime? ActiveFrom { get; set; }
        public DateTime? ActiveTo { get; set; }
        public string DeviceId { get; set; }
        public float? Distance { get; set; }
        public string DriverId { get; set; }
        [Write(false)]
        public TimeSpan? Duration 
        {
            get { return TimeSpan.FromTicks(DurationTicks.GetValueOrDefault()); }
            set
            {
                if (value.HasValue)
                {
                    DurationTicks = value.Value.Ticks;
                }
            }
        }
        public long? DurationTicks { get; set; }
        public string RuleId { get; set; }
        public long? Version { get; set; }
        [ChangeTracker]
        public DateTime RecordCreationTimeUtc { get; set; }
    }
}
