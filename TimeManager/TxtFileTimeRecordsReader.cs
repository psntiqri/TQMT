using System;
using System.Configuration;
using System.IO;
using System.Windows.Forms;

namespace Exilesoft.TimeManager
{
    public class TxtFileTimeRecordsReader : ITimeRecordsReader
    {
        private string[] _records;
        private int _startIndex;
        private string _txtFilePath;
        public short Connect_TCPIP(string deviceModel, int deviceNo, string ipAddress, int portNo, int commKey)
        {
            // Connecting and veryfing the connection to the device
            // Connect_TCPIP Returns 0 if the connection is successful
            _txtFilePath = ConfigurationManager.AppSettings["TxtFilePath"];
            _txtFilePath = _txtFilePath + "sample_DeviceNo_" + deviceNo + ".txt";
            if (!File.Exists(_txtFilePath))
            {
                //MessageBox.Show("Can not find time records file");
                return -1;
            }
            return 0;
        }

        public short ReadGeneralLog(ref int logSize)
        {
            // Gets all transaction logs from device memory
            // Returns 0 for success or -1 for fail.
            try
            {
                _records = File.ReadAllLines(_txtFilePath);
                _startIndex = 0;
            }
            catch (Exception)
            {
                MessageBox.Show("Error reading records");
                return -1;
            }
            
            return 0;
        }

        public short GetGeneralLog(ref int enrollNo, ref int year, ref int month, ref int day, ref int hour, ref int minute, ref int second, ref int verifyMode, ref int inOutMode, ref int workCode)
        {
            // Gets a single transaction log from device memory
            // Returns 0 for success or -1 for fail.
            try
            {
                if (_records != null && _records.Length > _startIndex)
                {
                    string record = _records[_startIndex];
                    string[] data = record.Split(',');
                    if (data.Length >= 10)
                    {
                        enrollNo = int.Parse(data[0]);
                        year = int.Parse(data[1]);
                        month = int.Parse(data[2]);
                        day = int.Parse(data[3]);
                        hour = int.Parse(data[4]);
                        minute = int.Parse(data[5]);
                        second = int.Parse(data[6]);
                        verifyMode = int.Parse(data[7]);
                        inOutMode = int.Parse(data[8]);
                        workCode = int.Parse(data[9]);

                        _startIndex++;
                        return 0;
                    }
                    //else
                    //{
                    //    MessageBox.Show("Invalid data : " + record);
                    //    return -1;
                    //}
                    
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Invalid data");
            }
            return -1;
        }


        public short DeleteGeneralLog()
        {
            // Returns 0 for success or -1 for fail.
            return 0;
        }

        public void Initialize(Control axSdk)
        {
            
        }
    }
}
