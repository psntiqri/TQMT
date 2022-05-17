using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using DataAccessLayer;
using Domain;

namespace TimeManager
{
    public partial class Form1 : Form
    {
        private BackgroundWorker bw;
        private Context db = new Context();
        private List<Employee> empList = new List<Employee>();
        private volatile bool cancel = false;
       
        public Form1()
        {
            InitializeComponent();

            // Create a background thread
            bw = new BackgroundWorker();
            bw.DoWork += new DoWorkEventHandler(SyncData);

            // Kick off the Async thread
            bw.RunWorkerAsync();
        }

        private void SyncData(object sender, DoWorkEventArgs e)
        {
            //Database.SetInitializer(new MigrateDatabaseToLatestVersion<Context, DataAccessLayer.Migrations.Configuration>());
            
            while (!cancel)
            {
                //Database.SetInitializer<Context>(
                //    new DropCreateDatabaseIfModelChanges<Context>());

                try
                {
                    //Main entrance
                    string devicename = "AC100";
                    string ipaddress = "192.168.20.202";
                    int deviceno = 3, portno = 4370, commkey = 3;
                    GetData(devicename, ipaddress, deviceno, portno, commkey, true);
                    axBioBridgeSDK1.Disconnect();

                    //Basement
                    devicename = "AC100";
                    ipaddress = "192.168.20.201";
                    deviceno = 2;
                    portno = 4370;
                    commkey = 2;
                    GetData(devicename, ipaddress, deviceno, portno, commkey, false);
                    axBioBridgeSDK1.Disconnect();

                    //2nd Floor
                    devicename = "AC100";
                    ipaddress = "192.168.20.204";
                    deviceno = 5;
                    portno = 4370;
                    commkey = 5;

                    GetData(devicename, ipaddress, deviceno, portno, commkey, false);
                    axBioBridgeSDK1.Disconnect();

                    devicename = "AC100";
                    ipaddress = "192.168.20.205";
                    deviceno = 6;
                    portno = 4370;
                    commkey = 6;

                    GetData(devicename, ipaddress, deviceno, portno, commkey, false);
                    axBioBridgeSDK1.Disconnect();

                    devicename = "AC100";
                    ipaddress = "192.168.20.206";
                    deviceno = 7;
                    portno = 4370;
                    commkey = 7;

                    GetData(devicename, ipaddress, deviceno, portno, commkey, false);
                    axBioBridgeSDK1.Disconnect();
                    Thread.Sleep(15000);

                }
                catch (Exception ex)
                {
                    axBioBridgeSDK1.Disconnect();
                    continue;
                }
            }
            //bw.CancelAsync();
            bw.Dispose();
        }

        private void GetData(string devicename, string ipaddress, int deviceno, int portno, int commkey, bool getemployee)
        {
            if (axBioBridgeSDK1.Connect_TCPIP(devicename, deviceno, ipaddress, portno, commkey) == 0)
            {
                int enrollNo = new int();
                int enrollNo1 = new int();
                int userSize = new int();
                string name = "";
                string pwd = "";
                int priv = new int();
                bool enable = new bool();

                var existingEmployeeList = db.Employees;
                var externalEmployeeList = db.EmployeesExternal;
                var att = db.Attendances;

                //if (getemployee)
                //{
                //    if (axBioBridgeSDK1.ReadAllUserInfo(ref userSize) == 0)
                //    {
                //        while (axBioBridgeSDK1.GetAllUserInfo(ref enrollNo1, ref name, ref pwd, ref priv, ref enable) ==
                //               0)
                //        {

                //            bool foundEmployee = false;
                //            foreach (var existingEmployee in existingEmployeeList)
                //            {
                //                if (existingEmployee.EnrollNo == enrollNo1)
                //                {
                //                    foundEmployee = true;
                //                    break;
                //                }
                //            }

                //            bool foundExternalEmployee = false;
                //            if (!foundEmployee)
                //            {

                //                foreach (var externalEmployee in externalEmployeeList)
                //                {
                //                    if (externalEmployee.EnrollNo == enrollNo1)
                //                    {
                //                        foundExternalEmployee = true;
                //                        break;
                //                    }
                //                }
                //            }

                //            if (!foundEmployee)
                //            {
                //                if (!foundExternalEmployee)
                //                {

                //                    Employee employee = new Employee();
                //                    employee.EnrollNo = enrollNo1;

                //                    //Get full employee name from EmployeeDetail
                //                    int no1 = enrollNo1;
                //                    var employeeObject = db.EmployeesDetail.Where(a => a.EnrollNo == no1);
                //                    var firstOrDefault = employeeObject.FirstOrDefault();
                //                    if (firstOrDefault != null)
                //                        employee.Name = firstOrDefault.FullName;
                //                    else
                //                        employee.Name = name;

                //                    employee.Password = pwd;
                //                    employee.Priv = priv;
                //                    employee.Enable = enable;
                //                    existingEmployeeList.Add(employee);
                //                }
                //            }
                //        }

                //        db.SaveChanges();
                //    }
                //}

                int logSize = 0;
                int year = 0;
                int month = 0;
                int day = 0;
                int hour = 0;
                int minute = 0;
                int sec = 0;
                int verifyMode = 0;
                int inOutMode = 0;
                int workCode = 0;

                if (axBioBridgeSDK1.ReadGeneralLog(ref logSize) == 0)
                {
                    while (
                        axBioBridgeSDK1.GetGeneralLog(ref enrollNo, ref year, ref month, ref day, ref hour, ref minute,
                                                      ref sec, ref verifyMode, ref inOutMode, ref workCode) ==
                        0)
                    {
                        var existingAttendances = db.Attendances;
                        string mode = "";

                        if (inOutMode == 0)
                            mode = "in";
                        else
                            mode = "out";

                        bool foundAttendance = false;
                        foreach (var existingAttendance in existingAttendances)
                        {
                            if (existingAttendance.EnrollNo == enrollNo && existingAttendance.Year == year &&
                                existingAttendance.Month == month && existingAttendance.Day == day &&
                                existingAttendance.Hour == hour &&
                                existingAttendance.Minute == minute && existingAttendance.Second == sec &&
                                existingAttendance.VerifyMode == verifyMode && existingAttendance.InOutMode == mode &&
                                existingAttendance.WorkCode == workCode)
                            {
                                foundAttendance = true;
                                break;
                            }
                        }

                        if (!foundAttendance)
                        {
                            var attendance = new Attendance();
                            var flag = false;
                            foreach (var employee in existingEmployeeList)
                            {
                                if (employee.EnrollNo == enrollNo)
                                {
                                    employee.Name = employee.Name.Trim();
                                    attendance.Employee = employee;
                                    flag = true;
                                    break;
                                }
                            }

                            if (flag)
                            {
                                attendance.EnrollNo = enrollNo;
                                attendance.Year = year;
                                attendance.Month = month;
                                attendance.Day = day;
                                attendance.Hour = hour;
                                attendance.Minute = minute;
                                attendance.Second = sec;
                                //Add location info
                                attendance.Location = db.Locations.Where(a => a.DeviceNo == deviceno).SingleOrDefault();

                                attendance.VerifyMode = verifyMode;
                                if (inOutMode == 0)
                                    attendance.InOutMode = "in";
                                else
                                    attendance.InOutMode = "out";
                                attendance.WorkCode = workCode;

                                db.Attendances.Add(attendance);
                                db.SaveChanges();
                            }
                        }

                    }
                }

                axBioBridgeSDK1.Disconnect();
            }
        }


        //private void SyncData(object sender, DoWorkEventArgs e)
        //{
        //    while (!cancel)
        //    {
        //        //progressBar1.Visible = true;
        //        string devicename = "AC100";

        //        string ipaddress = "192.168.20.202";
        //        int deviceno = 3, portno = 4370, commkey = 3;

        //        if (axBioBridgeSDK1.Connect_TCPIP(devicename, deviceno, ipaddress, portno, commkey) == 0)
        //        {
        //            //label1.Text = "Connected";
        //            //lblConnected.BackColor = Color.Yellow;
        //            //lblConnected.Text = ".....Connected to Main Entrance.....";
        //        }
        //        else
        //        {
        //            //label1.Text = "Not Connected";
        //            //lblConnected.BackColor = Color.Red;
        //            //lblConnected.Text = "Failed to Connect to main entrance";
        //        }

        //        Database.SetInitializer<Context>(
        //            new DropCreateDatabaseIfModelChanges<Context>());


        //        int enrollNo = new int();
        //        int enrollNo1 = new int();
        //        int userSize = new int();
        //        string name = "";
        //        string pwd = "";
        //        int priv = new int();
        //        bool enable = new bool();

        //        var existingEmployeeList = db.Employees;
        //        var externalEmployeeList = db.EmployeesExternal;
        //        var att = db.Attendances;

        //        if (axBioBridgeSDK1.ReadAllUserInfo(ref userSize) == 0)
        //        {
        //            while (axBioBridgeSDK1.GetAllUserInfo(ref enrollNo1, ref name, ref pwd, ref priv, ref enable) ==
        //                   0)
        //            {

        //                bool foundEmployee = false;
        //                foreach (var existingEmployee in existingEmployeeList)
        //                {
        //                    if (existingEmployee.EnrollNo == enrollNo1)
        //                    {
        //                        foundEmployee = true;
        //                        break;
        //                    }
        //                }

        //                bool foundExternalEmployee = false;
        //                if (!foundEmployee)
        //                {

        //                    foreach (var externalEmployee in externalEmployeeList)
        //                    {
        //                        if (externalEmployee.EnrollNo == enrollNo1)
        //                        {
        //                            foundExternalEmployee = true;
        //                            break;
        //                        }
        //                    }
        //                }

        //                if (!foundEmployee)
        //                {
        //                    if (!foundExternalEmployee)
        //                    {

        //                        Employee employee = new Employee();
        //                        employee.EnrollNo = enrollNo1;

        //                        //Get full employee name from EmployeeDetail
        //                        int no1 = enrollNo1;
        //                        var employeeObject = db.EmployeesDetail.Where(a => a.EnrollNo == no1);
        //                        var firstOrDefault = employeeObject.FirstOrDefault();
        //                        if (firstOrDefault != null)
        //                            employee.Name = firstOrDefault.FullName;
        //                        else
        //                            employee.Name = name;

        //                        employee.Password = pwd;
        //                        employee.Priv = priv;
        //                        employee.Enable = enable;
        //                        existingEmployeeList.Add(employee);
        //                    }
        //                }
        //            }

        //            db.SaveChanges();
        //        }

        //        int logSize = 0;
        //        int year = 0;
        //        int month = 0;
        //        int day = 0;
        //        int hour = 0;
        //        int minute = 0;
        //        int sec = 0;
        //        int verifyMode = 0;
        //        int inOutMode = 0;
        //        int workCode = 0;

        //        if (axBioBridgeSDK1.ReadGeneralLog(ref logSize) == 0)
        //        {
        //            while (
        //                axBioBridgeSDK1.GetGeneralLog(ref enrollNo, ref year, ref month, ref day, ref hour, ref minute,
        //                                              ref sec, ref verifyMode, ref inOutMode, ref workCode) ==
        //                0)
        //            {
        //                var existingAttendances = db.Attendances;
        //                string mode = "";

        //                if (inOutMode == 0)
        //                    mode = "in";
        //                else
        //                    mode = "out";

        //                bool foundAttendance = false;
        //                foreach (var existingAttendance in existingAttendances)
        //                {
        //                    if (existingAttendance.EnrollNo == enrollNo && existingAttendance.Year == year &&
        //                        existingAttendance.Month == month && existingAttendance.Day == day &&
        //                        existingAttendance.Hour == hour &&
        //                        existingAttendance.Minute == minute && existingAttendance.Second == sec &&
        //                        existingAttendance.VerifyMode == verifyMode && existingAttendance.InOutMode == mode &&
        //                        existingAttendance.WorkCode == workCode)
        //                    {
        //                        foundAttendance = true;
        //                        break;
        //                    }
        //                }

        //                if (!foundAttendance)
        //                {
        //                    var attendance = new Attendance();
        //                    var flag = false;
        //                    foreach (var employee in existingEmployeeList)
        //                    {
        //                        if (employee.EnrollNo == enrollNo)
        //                        {
        //                            employee.Name = employee.Name.Trim();
        //                            attendance.Employee = employee;
        //                            flag = true;
        //                            break;
        //                        }
        //                    }

        //                    if (flag)
        //                    {
        //                        attendance.EnrollNo = enrollNo;
        //                        attendance.Year = year;
        //                        attendance.Month = month;
        //                        attendance.Day = day;
        //                        attendance.Hour = hour;
        //                        attendance.Minute = minute;
        //                        attendance.Second = sec;
        //                        //Add location info
        //                        attendance.Location = db.Locations.Where(a => a.DeviceNo == 3).SingleOrDefault();

        //                        attendance.VerifyMode = verifyMode;
        //                        if (inOutMode == 0)
        //                            attendance.InOutMode = "in";
        //                        else
        //                            attendance.InOutMode = "out";
        //                        attendance.WorkCode = workCode;

        //                        db.Attendances.Add(attendance);
        //                        db.SaveChanges();
        //                    }
        //                }

        //            }
        //        }

        //        axBioBridgeSDK1.Disconnect();

        //        string devicename1 = "AC100";
        //        string ipaddress1 = "192.168.20.201";
        //        int deviceno1 = 2, portno1 = 4370, commkey1 = 2;

        //        if (axBioBridgeSDK1.Connect_TCPIP(devicename1, deviceno1, ipaddress1, portno1, commkey1) == 0)
        //        {
        //            //label1.Text = "Connected";
        //        }
        //        else
        //        {
        //            //label1.Text = "Not Connected";
        //        }

        //        if (axBioBridgeSDK1.ReadGeneralLog(ref logSize) == 0)
        //        {
        //            while (
        //                axBioBridgeSDK1.GetGeneralLog(ref enrollNo, ref year, ref month, ref day, ref hour, ref minute,
        //                                              ref sec, ref verifyMode, ref inOutMode, ref workCode) ==
        //                0)
        //            {
        //                var existingAttances = db.Attendances;
        //                string mode = "";

        //                if (inOutMode == 0)
        //                    mode = "in";
        //                else
        //                    mode = "out";

        //                bool foundAttendance = false;
        //                foreach (var existingAttance in existingAttances)
        //                {
        //                    if (existingAttance.EnrollNo == enrollNo && existingAttance.Year == year &&
        //                        existingAttance.Month == month && existingAttance.Day == day &&
        //                        existingAttance.Hour == hour &&
        //                        existingAttance.Minute == minute && existingAttance.Second == sec &&
        //                        existingAttance.VerifyMode == verifyMode && existingAttance.InOutMode == mode &&
        //                        existingAttance.WorkCode == workCode)
        //                    {
        //                        foundAttendance = true;
        //                    }
        //                }

        //                if (!foundAttendance)
        //                {
        //                    var attendance = new Attendance();
        //                    bool flag1 = false;
        //                    foreach (var employee in existingEmployeeList)
        //                    {
        //                        if (employee.EnrollNo == enrollNo)
        //                        {
        //                            employee.Name = employee.Name.Trim();
        //                            attendance.Employee = employee;
        //                            flag1 = true;
        //                            break;
        //                        }
        //                    }
        //                    if (flag1)
        //                    {
        //                        attendance.EnrollNo = enrollNo;
        //                        attendance.Year = year;
        //                        attendance.Month = month;
        //                        attendance.Day = day;
        //                        attendance.Hour = hour;
        //                        attendance.Minute = minute;
        //                        attendance.Second = sec;
        //                        //Add location info
        //                        attendance.Location = db.Locations.Where(a => a.DeviceNo == 2).SingleOrDefault();

        //                        attendance.VerifyMode = verifyMode;
        //                        if (inOutMode == 0)
        //                            attendance.InOutMode = "in";
        //                        else
        //                            attendance.InOutMode = "out";
        //                        attendance.WorkCode = workCode;

        //                        db.Attendances.Add(attendance);
        //                        db.SaveChanges();
        //                    }
        //                }
        //            }
        //        }

        //        axBioBridgeSDK1.Disconnect();
        //        Thread.Sleep(15000);
        //    }
        //    //bw.CancelAsync();
        //    bw.Dispose();
        //}
        
        private void button2_Click(object sender, EventArgs e)
        {
            cancel = true;
            Dispose();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Dispose();
        }
    }
}
