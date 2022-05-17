using System.Windows.Forms;

namespace Exilesoft.TimeManager
{
    public class AxBioBridgeTimeRecordsReader : ITimeRecordsReader
    {
        private AxBioBridgeSDKLib.AxBioBridgeSDK _axBioBridgeSdk;

        public short Connect_TCPIP(string deviceModel, int deviceNo, string ipAddress, int portNo, int commKey)
        {
            return _axBioBridgeSdk.Connect_TCPIP(deviceModel, deviceNo,
                ipAddress, portNo, commKey);
        }

        public short ReadGeneralLog(ref int logSize)
        {
            return _axBioBridgeSdk.ReadGeneralLog(ref logSize);
        }

        public short GetGeneralLog(ref int enrollNo, ref int year, ref int month, ref int day, ref int hour, ref int minute, ref int second, ref int verifyMode, ref int inOutMode, ref int workCode)
        {
            return _axBioBridgeSdk.GetGeneralLog(ref enrollNo, ref year, ref month, ref day, ref hour, ref minute,
                ref second, ref verifyMode, ref inOutMode, ref workCode);
        }

        public short DeleteGeneralLog()
        {
            return _axBioBridgeSdk.DeleteGeneralLog();
        }

        public void Initialize(Control axSdk)
        {
            _axBioBridgeSdk = (AxBioBridgeSDKLib.AxBioBridgeSDK)axSdk;
        }
    }
}
