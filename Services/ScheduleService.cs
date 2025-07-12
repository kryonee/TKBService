using TKBService.Models;

namespace TKBService.Services
{
    public class ScheduleService
    {
        public Dictionary<string, List<ScheduleAssignment>> GenerateSchedule(List<ScheduleItem> scheduleList)
        {
            var rand = new Random();
            var allSubjects = scheduleList
                .Where(s => !string.IsNullOrWhiteSpace(s.SubjectName))
                .Select(s => s.SubjectName!.Trim())
                .Distinct()
                .ToList();

            var teachers = Enumerable.Range(1, 100).Select(i => new Teacher(
                $"GV{i:00}",
                allSubjects.OrderBy(_ => rand.Next()).Take(30).ToList(),
                (from d in Enumerable.Range(1, 6)
                 from p in Enumerable.Range(1, 6)
                 where rand.NextDouble() < 0.98
                 select ((DayOfWeek)d, p)).ToList()
            )).ToList();

            var rooms = scheduleList
                .Where(s => s.RoomName != null)
                .Select(s => new Room(s.RoomName!, s.RoomName!, s.FacultyName ?? ""))
                .DistinctBy(r => r.Id)
                .ToList();

            var usedTeachers = new Dictionary<string, HashSet<Slot>>();
            var usedRooms = new Dictionary<string, HashSet<Slot>>();
            var scheduled = new List<ScheduleAssignment>();
            var failedSubjects = new List<string>();
            var reasons = new Dictionary<string, List<string>>();

            var days = Enum.GetValues<DayOfWeek>().Where(d => d >= DayOfWeek.Monday && d <= DayOfWeek.Saturday);
            var periods = Enumerable.Range(1, 6);

            // Greedy strategy: group by subject+class to reduce conflict
            var grouped = scheduleList.GroupBy(x => $"{x.SubjectName}||{x.SubjectTeachingName}");

            foreach (var group in grouped)
            {
                var subjectParts = group.Key.Split("||");
                var subjectName = subjectParts[0];
                var className = subjectParts[1];
                var subjectKey = $"{subjectName} ({className})";

                reasons[subjectKey] = new();

                var teacherOptions = teachers.Where(t => t.CanTeachSubjects.Contains(subjectName)).OrderBy(_ => rand.Next()).ToList();
                if (!teacherOptions.Any())
                {
                    reasons[subjectKey].Add("Không có giáo viên nào phù hợp");
                    failedSubjects.Add($"⚠️ Không thể xếp môn: {subjectKey}");
                    continue;
                }

                var roomOptions = rooms.Where(r => r.RoomType == group.First().FacultyName).ToList();
                if (roomOptions.Count == 0)
                {
                    reasons[subjectKey].Add("Không tìm thấy phòng đúng khoa — fallback dùng tất cả phòng");
                    roomOptions = rooms.ToList();
                }

                roomOptions = roomOptions.OrderBy(r => usedRooms.TryGetValue(r.Id, out var used) ? used.Count : 0).ToList();

                var slotOptions = (from d in days from p in periods select new Slot(d, p))
                    .OrderBy(slot => usedRooms.Values.Count(ur => ur.Contains(slot)) + usedTeachers.Values.Count(ut => ut.Contains(slot)))
                    .ToList();

                bool assigned = false;
                foreach (var slot in slotOptions)
                {
                    foreach (var teacher in teacherOptions)
                    {
                        if (!teacher.AvailableSlots.Contains((slot.Day, slot.Period))) continue;
                        usedTeachers.TryAdd(teacher.Id, new());
                        if (usedTeachers[teacher.Id].Contains(slot)) continue;

                        foreach (var room in roomOptions)
                        {
                            usedRooms.TryAdd(room.Id, new());
                            if (usedRooms[room.Id].Contains(slot)) continue;

                            foreach (var item in group)
                            {
                                scheduled.Add(new ScheduleAssignment(
                                    item.SubjectName!, teacher.Id, room.Id, slot.Day, slot.Period, item.SubjectTeachingName!
                                ));
                            }

                            usedTeachers[teacher.Id].Add(slot);
                            usedRooms[room.Id].Add(slot);
                            reasons.Remove(subjectKey);
                            assigned = true;
                            break;
                        }
                        if (assigned) break;
                    }
                    if (assigned) break;
                }

                if (!assigned)
                {
                    failedSubjects.Add($"⚠️ Không thể xếp môn: {subjectKey}");
                }
            }

            if (failedSubjects.Any())
            {
                File.WriteAllLines("unscheduled.log", failedSubjects);
                Console.WriteLine($"(Tổng: {failedSubjects.Count}) môn không thể xếp slot");

                using var sw = new StreamWriter("unscheduled_detailed.log");
                foreach (var kvp in reasons)
                {
                    sw.WriteLine($"⛔ {kvp.Key}");
                    foreach (var reason in kvp.Value.Distinct())
                        sw.WriteLine($"  - {reason}");
                    sw.WriteLine();
                }
            }

            return scheduled.GroupBy(s => s.ClassName)
                .ToDictionary(g => g.Key, g => g.OrderBy(s => s.Day).ThenBy(s => s.Period).ToList());
        }
    }
}
