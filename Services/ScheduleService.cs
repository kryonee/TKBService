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

            var teachers = Enumerable.Range(1, 20).Select(i => new Teacher(
                $"GV{i:00}",
                allSubjects.OrderBy(_ => rand.Next()).Take(15).ToList(),
                (from d in Enumerable.Range(1, 6)
                 from p in Enumerable.Range(1, 5)
                 where rand.NextDouble() < 0.95
                 select ((DayOfWeek)d, p)).ToList()
            )).ToList();

            var rooms = scheduleList
                .Where(s => s.RoomName != null)
                .Select(s => new Room(s.RoomName!, s.RoomName!, s.FacultyName ?? ""))
                .DistinctBy(r => r.Id)
                .ToList();

            var usedTeachers = new Dictionary<string, HashSet<Slot>>();
            var usedRooms = new Dictionary<string, HashSet<Slot>>();
            var usedSubjects = new Dictionary<string, HashSet<Slot>>();
            var scheduled = new List<ScheduleAssignment>();

            bool IsSlotNear(HashSet<Slot> assignedSlots, Slot newSlot) => false;

            var days = Enum.GetValues<DayOfWeek>().Where(d => d >= DayOfWeek.Monday && d <= DayOfWeek.Saturday);
            var periods = Enumerable.Range(1, 5);

            foreach (var subject in scheduleList)
            {
                if (string.IsNullOrEmpty(subject.SubjectName) || string.IsNullOrEmpty(subject.SubjectTeachingName)) continue;

                Console.WriteLine($"[DEBUG] Đang xét môn: {subject.SubjectName} ({subject.SubjectTeachingName})");

                var teacherOptions = teachers.Where(t => t.CanTeachSubjects.Contains(subject.SubjectName)).OrderBy(_ => rand.Next()).ToList();
                var roomOptions = rooms.Where(r => r.RoomType == subject.FacultyName).ToList();

                if (roomOptions.Count == 0)
                {
                    Console.WriteLine($"[DEBUG] Không tìm thấy phòng phù hợp với khoa '{subject.FacultyName}' — fallback sang tất cả phòng");
                    roomOptions = rooms.ToList();
                }

                roomOptions = roomOptions
                    .OrderBy(r => usedRooms.TryGetValue(r.Id, out var used) ? used.Count : 0)
                    .ToList();

                var slotOptions = (from d in days from p in periods select new Slot(d, p))
                    .OrderBy(slot =>
                        (usedRooms.Values.Count(ur => ur.Contains(slot)) +
                         usedTeachers.Values.Count(ut => ut.Contains(slot)) +
                         usedSubjects.Values.Count(us => us.Contains(slot))))
                    .ToList();

                Console.WriteLine($"[DEBUG] Số giáo viên phù hợp: {teacherOptions.Count}, số phòng phù hợp: {roomOptions.Count}, số slot: {slotOptions.Count}");

                bool assigned = false;
                foreach (var teacher in teacherOptions)
                {
                    Console.WriteLine($"[DEBUG] Giáo viên {teacher.Id} có {teacher.AvailableSlots.Count} slot rảnh.");
                    usedTeachers.TryAdd(teacher.Id, new());

                    foreach (var room in roomOptions)
                    {
                        usedRooms.TryAdd(room.Id, new());
                        usedSubjects.TryAdd(subject.SubjectName, new());

                        foreach (var slot in slotOptions)
                        {
                            bool available = true;

                            if (!teacher.AvailableSlots.Contains((slot.Day, slot.Period)))
                            {
                                available = false;
                            }
                            else if (usedTeachers[teacher.Id].Contains(slot)) available = false;
                            else if (usedRooms[room.Id].Contains(slot))
                            {
                                Console.WriteLine($"[DEBUG] Phòng {room.Id} đã bị chiếm slot {slot.Day}, Ca {slot.Period}");
                                available = false;
                            }
                            else if (usedSubjects[subject.SubjectName].Contains(slot)) available = false;

                            if (!available) continue;

                            Console.WriteLine($"[DEBUG] Slot hợp lệ tìm thấy: GV {teacher.Id} - {room.Id} - {slot.Day}, Ca {slot.Period}");

                            usedTeachers[teacher.Id].Add(slot);
                            usedRooms[room.Id].Add(slot);
                            usedSubjects[subject.SubjectName].Add(slot);

                            scheduled.Add(new ScheduleAssignment(
                                subject.SubjectName!, teacher.Id, room.Id, slot.Day, slot.Period, subject.SubjectTeachingName!
                            ));

                            Console.WriteLine($"[DEBUG] Xếp {subject.SubjectName} với GV {teacher.Id} tại {room.Id}, {slot.Day}, Ca {slot.Period}");
                            assigned = true;
                            break;
                        }
                        if (assigned) break;
                    }
                    if (assigned) break;
                }

                if (!assigned)
                {
                    Console.WriteLine($"Không thể xếp môn: {subject.SubjectName} ({subject.SubjectTeachingName})");
                }
            }

            return scheduled.GroupBy(s => s.ClassName)
                .ToDictionary(g => g.Key, g => g.OrderBy(s => s.Day).ThenBy(s => s.Period).ToList());
        }
    }
}