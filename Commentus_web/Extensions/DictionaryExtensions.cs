namespace Commentus_web.Extensions
{
    public static class DictionaryExtensions
    {
        public static DateTime? GetTimeStamp(this Dictionary<string, DateTime?> timeStamps, string? userName) =>
            !timeStamps.Where(x => x.Key == userName).FirstOrDefault().Value.Equals(default(DateTime)) ?
                                timeStamps.First(x => x.Key == userName).Value :
                                default(DateTime);
    }
}
