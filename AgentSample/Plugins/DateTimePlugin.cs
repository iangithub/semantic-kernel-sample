using System.ComponentModel;
using Microsoft.SemanticKernel;

[Description("DateTimeFunctions")]
public class DateTimePlugin
{
    [KernelFunction("GetCurrentTime"), Description("Retrieves real world the current date time")]
    public string GetCurrentDateTime()
    {
        return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}