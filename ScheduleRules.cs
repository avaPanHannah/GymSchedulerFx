using System;
using System.Collections.Generic;
using System.Linq;

namespace GymSchedulerFx
{
    public enum RuleCheckResult { OK, DuplicateShift, InsufficientRest, MaxDaysReached, MaxHoursReached }

    public class RuleCheckReport
    {
        public RuleCheckResult Result { get; set; }
        public string Message { get; set; }
        public bool IsSuccess { get { return Result == RuleCheckResult.OK; } }
    }

    public class MinStaffReport
    {
        public List<string> Violations { get; set; }
        public bool IsAllGood { get { return Violations.Count == 0; } }
        public MinStaffReport() { Violations = new List<string>(); }
    }

    public static class ScheduleRules
    {
        public const int MaxDaysPerWeek = 5;
        public const int MaxHoursPerWeek = 40;
        public const int HoursPerShift = 8;

        public static readonly DayOfWeek[] WeekOrder = new[]
        {
            DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
            DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday
        };

        public static string GetDayName(DayOfWeek d)
        {
            switch (d)
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

        public static RuleCheckReport ValidateAddEntry(List<ScheduleEntry> existing, string coach, DayOfWeek day, ShiftType shift)
        {
            // 規則一：同日重複
            bool dup = existing.Exists(e => e.CoachName == coach && e.DayOfWeek == day);
            if (dup)
                return new RuleCheckReport { Result = RuleCheckResult.DuplicateShift, Message = "❌ 規則一：" + coach + " 在 " + GetDayName(day) + " 已有班別，一天只能排一個班！" };

            int todayIdx = Array.IndexOf(WeekOrder, day);

            // 規則二：前天晚班→今天不可早班
            if (shift == ShiftType.Morning && todayIdx > 0)
            {
                DayOfWeek prev = WeekOrder[todayIdx - 1];
                bool prevEvening = existing.Exists(e => e.CoachName == coach && e.DayOfWeek == prev && e.Shift == ShiftType.Evening);
                if (prevEvening)
                    return new RuleCheckReport { Result = RuleCheckResult.InsufficientRest, Message = "❌ 規則二：" + coach + " " + GetDayName(prev) + " 排晚班（22:00下班），" + GetDayName(day) + " 早班（06:00上班）休息不足8小時！" };
            }

            // 規則二：今天晚班→明天不可有早班
            if (shift == ShiftType.Evening && todayIdx < WeekOrder.Length - 1)
            {
                DayOfWeek next = WeekOrder[todayIdx + 1];
                bool nextMorning = existing.Exists(e => e.CoachName == coach && e.DayOfWeek == next && e.Shift == ShiftType.Morning);
                if (nextMorning)
                    return new RuleCheckReport { Result = RuleCheckResult.InsufficientRest, Message = "⚠️ 規則二：" + coach + " " + GetDayName(next) + " 已排早班，今天排晚班會導致休息不足，請先移除 " + GetDayName(next) + " 早班！" };
            }

            // 規則三：最多5天
            int days = existing.FindAll(e => e.CoachName == coach).Count;
            if (days >= MaxDaysPerWeek)
                return new RuleCheckReport { Result = RuleCheckResult.MaxDaysReached, Message = "❌ 規則三：" + coach + " 本週已排滿 " + MaxDaysPerWeek + " 天（一例一休），不可再加排！" };

            // 規則三：最多40小時
            if ((days + 1) * HoursPerShift > MaxHoursPerWeek)
                return new RuleCheckReport { Result = RuleCheckResult.MaxHoursReached, Message = "❌ 規則三：" + coach + " 再加排將超過每週 " + MaxHoursPerWeek + " 小時工時上限！" };

            return new RuleCheckReport { Result = RuleCheckResult.OK, Message = "✅ 排班合規，已成功新增！" };
        }

        public static MinStaffReport CheckMinStaff(List<ScheduleEntry> entries)
        {
            var report = new MinStaffReport();
            foreach (var day in WeekOrder)
            {
                foreach (ShiftType shift in Enum.GetValues(typeof(ShiftType)))
                {
                    string sn = shift == ShiftType.Morning ? "早班" : "晚班";
                    int count = entries.FindAll(e => e.DayOfWeek == day && e.Shift == shift).Count;
                    if (count < 1)
                        report.Violations.Add("⚠️ " + GetDayName(day) + " " + sn + "：無人值班（需至少1人）");
                }
            }
            return report;
        }

        public static int GetDays(List<ScheduleEntry> entries, string coach)
        {
            return entries.FindAll(e => e.CoachName == coach).Count;
        }

        public static int GetHours(List<ScheduleEntry> entries, string coach)
        {
            return GetDays(entries, coach) * HoursPerShift;
        }
    }
}
