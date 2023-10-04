using Commentus_web.Models;

namespace Commentus_web.Extensions
{
    public static class DictionaryExtensions
    {
        public static DateTime? GetTimeStamp(this Dictionary<string, DateTime?> timeStamps, string? userName) =>
            timeStamps.First(x => x.Key == userName).Value;
    }
}
