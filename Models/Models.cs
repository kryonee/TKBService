using System.Text.Json.Serialization;

namespace TKBService.Models
{
    public class ScheduleItem
    {
        [JsonPropertyName("subjectName")] public string? SubjectName { get; set; }
        [JsonPropertyName("startDateTime")] public DateTime StartDateTime { get; set; }
        [JsonPropertyName("endDateTime")] public DateTime EndDateTime { get; set; }
        [JsonPropertyName("roomName")] public string? RoomName { get; set; }
        [JsonPropertyName("facultyName")] public string? FacultyName { get; set; }
        [JsonPropertyName("subjectTeachingName")] public string? SubjectTeachingName { get; set; }
    }

    public record Teacher(string Id, List<string> CanTeachSubjects, List<(DayOfWeek, int)> AvailableSlots);
    public record Room(string Id, string Name, string RoomType);
    public record Slot(DayOfWeek Day, int Period);
    public record ScheduleAssignment(string SubjectName, string TeacherId, string RoomId, DayOfWeek Day, int Period, string ClassName);
}
