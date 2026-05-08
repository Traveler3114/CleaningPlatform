using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleaningPlatformAPI.Entities
{
    [Table("DateOverride")]
    public class DateOverride
    {
        [Key]
        public int Id { get; set; }

        public DateTime Date { get; set; }
        public int? StartHour { get; set; }
        public int? EndHour { get; set; }
        public int? Capacity { get; set; }
        public bool IsFullyClosed { get; set; }
    }
}
