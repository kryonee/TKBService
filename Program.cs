using System;
using System.Threading.Tasks;
using TKBService.Services;
using TKBService.Models;
using TKBService.Controllers;
class Program
{
    static async Task Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("Hệ thống sắp xếp thời khoá biểu...");

        var controller = new ScheduleController();
        await controller.RunAsync(useApi: true);
        Console.WriteLine("Hoàn tất.");
    }
}