using System.Configuration;

namespace Exilesoft.TimeManager
{
    public class TimeRecordsReaderFactory
    {
        public ITimeRecordsReader GetTimeRecordsReader()
        {
            int timeRecordsReader = int.Parse(ConfigurationManager.AppSettings["TimeRecordsReader"]);
            if (timeRecordsReader != 0)
                return new TxtFileTimeRecordsReader();
            return new AxBioBridgeTimeRecordsReader();
        }
    }
}
