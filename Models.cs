using System;
using System.Collections.Generic;

namespace GymSchedulerFx
{
    public enum ShiftType { Morning, Evening }

    public class ScheduleEntry
    {
        public string CoachName { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public ShiftType Shift { get; set; }

        public string ShiftDisplayName
        {
            get { return Shift == ShiftType.Morning ? "早班 06:00-14:00" : "晚班 14:00-22:00"; }
        }

        public string DayDisplayName
        {
            get
            {
                switch (DayOfWeek)
                {
                    case DayOfWeek.Monday: return "週一";
                    case DayOfWeek.Tuesday: return "週二";
                    case DayOfWeek.Wednesday: return "週三";
                    case DayOfWeek.Thursday: return "週四";
                    case DayOfWeek.Friday: return "週五";
                    case DayOfWeek.Saturday: return "週六";
                    case DayOfWeek.Sunday: return "週日";
                    default: return "";
                }
            }
        }
    }
}
