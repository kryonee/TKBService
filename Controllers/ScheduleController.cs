using System.Text.Encodings.Web;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TKBService.Models;
using TKBService.Services;

namespace TKBService.Controllers
{
    public class ScheduleController
    {
        public async Task RunAsync(bool useApi = true)
        {
            Console.OutputEncoding = Encoding.UTF8;

            List<ScheduleItem> scheduleItems;

            if (useApi)
            {
                var apiUrl = "https://taydo.taydocantho.com/Api/Public/GetPaging";
                var apiKey = "TfOH1SdlNLW9z5kzqYk6r6G6H7zGO8kN3gAydyZ1mWc=";

                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("api-key", apiKey);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var postData = new { page = 1, pageSize = 100 };
                var content = new StringContent(JsonSerializer.Serialize(postData), Encoding.UTF8, "application/json");

                Console.WriteLine("\ud83d\udce1 \u0110ang g\u1ecdi API...");
                var response = await httpClient.PostAsync(apiUrl, content);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"\u274c API l\u1ed7i: {response.StatusCode}");
                    return;
                }

                var responseBody = await response.Content.ReadAsStringAsync();
                var jsonDoc = JsonDocument.Parse(responseBody);
                var itemsJson = jsonDoc.RootElement.GetProperty("result").GetProperty("items");

                scheduleItems = itemsJson.EnumerateArray().Select(item => new ScheduleItem
                {
                    SubjectName = item.GetProperty("subjectName").GetString(),
                    StartDateTime = item.GetProperty("startDateTime").GetDateTime(),
                    EndDateTime = item.GetProperty("endDateTime").GetDateTime(),
                    RoomName = item.GetProperty("roomName").GetString(),
                    FacultyName = item.GetProperty("facultyName").GetString(),
                    SubjectTeachingName = item.GetProperty("subjectTeachingName").GetString()
                }).ToList();

                await File.WriteAllTextAsync("output.json", JsonSerializer.Serialize(scheduleItems,
                    new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));

                Console.WriteLine("\ud83d\udcc2 \u0110\u00e3 l\u01b0u output.json");
            }
            else
            {
                Console.WriteLine("\ud83d\udcc2 \u0110ang t\u1ea3i d\u1eef li\u1ec7u t\u1eeb response.json...");
                var raw = await File.ReadAllTextAsync("response.json");
                var jsonDoc = JsonDocument.Parse(raw);
                var itemsJson = jsonDoc.RootElement.GetProperty("result").GetProperty("items");

                scheduleItems = itemsJson.EnumerateArray().Select(item => new ScheduleItem
                {
                    SubjectName = item.GetProperty("subjectName").GetString(),
                    StartDateTime = item.GetProperty("startDateTime").GetDateTime(),
                    EndDateTime = item.GetProperty("endDateTime").GetDateTime(),
                    RoomName = item.GetProperty("roomName").GetString(),
                    FacultyName = item.GetProperty("facultyName").GetString(),
                    SubjectTeachingName = item.GetProperty("subjectTeachingName").GetString()
                }).ToList();

                Console.WriteLine("\u2705 \u0110\u00e3 \u0111\u1ecdc d\u1eef li\u1ec7u t\u1eeb file JSON.");
            }

            // G\u1ecdi service
            var service = new ScheduleService();
            var result = service.GenerateSchedule(scheduleItems);

            await File.WriteAllTextAsync("scheduled_by_class.json",
                JsonSerializer.Serialize(result, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                }));

            Console.WriteLine("\ud83d\udcc2 \u0110\u00e3 l\u01b0u l\u1ecbch nh\u00f3m theo l\u1edbp v\u00e0o scheduled_by_class.json");
        }
    }
}
