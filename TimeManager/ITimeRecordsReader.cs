using System.Windows.Forms;

namespace Exilesoft.TimeManager
{
    public interface ITimeRecordsReader
    {
        void Initialize(Control axSdk);
        short Connect_TCPIP(string deviceModel, int deviceNo, string ipAddress, int portNo, int commKey);
        short ReadGeneralLog(ref int logSize);
        short GetGeneralLog(ref int enrollNo, ref int year, ref int month, ref int day, ref int hour, ref int minute, ref int second, ref int verifyMode, ref int inOutMode, ref int workCode);
        short DeleteGeneralLog();
    }
}
