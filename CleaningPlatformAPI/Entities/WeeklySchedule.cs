using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleaningPlatformAPI.Entites
{
    [Table("WeeklySchedule")]
    public class WeeklySchedule
    {
        [Key]
        public int Id { get; set; }

        public int DayOfWeek { get; set; }   // 0=Sunday...
        public int StartHour { get; set; }
        public int EndHour { get; set; }
        public int Capacity { get; set; }
    }
}