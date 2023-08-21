using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPSpeedView
{
    public static class DataHelper
    {
        static DataHelper()
        {
            try
            {
                if (!Directory.Exists("DataTxt"))
                {
                    Directory.CreateDirectory("DataTxt");
                }
            }
            catch (Exception)
            {
            }
        }
        public static List<string> GetLockGPs()
        {
            List<string> gps = new List<string>();
            try
            {
                if (File.Exists(ConfigData.LockGpsFilePath))
                {
                    string strs = File.ReadAllText(ConfigData.LockGpsFilePath);
                    var codes = strs.Split(';');
                    foreach (var item in codes)
                    {
                        if(!string.IsNullOrEmpty(item))
                            gps.Add(item);
                    }
                }
            }
            catch (Exception)
            {
            }
            return gps;
        }

        public static Dictionary<string, List<string>> GetMidFastUpGPs(string path = null)
        {
            Dictionary<string, List<string>> gps = new Dictionary<string, List<string>>();

            if (path == null)
                path = ConfigData.MidFastGpsFilePath;
            try
            {
                if (File.Exists(path))
                {
                    FileInfo fileInfo = new FileInfo(path);
                    using (StreamReader streamReader = fileInfo.OpenText())
                    {
                        string line = streamReader.ReadLine();
                        while (line != null)
                        {
                            var strs = line.Split(':');
                            if (strs.Count() == 2)
                            {
                                var times = strs[0].Split('-');
                                if(times.Count()!= 3)
                                    times = strs[0].Split('/');
                                if (times.Count() != 3)
                                    continue;
                                DateTime time = new DateTime(int.Parse(times[0]), int.Parse(times[1]), int.Parse(times[2]));
                                var codes = strs[1].Split(';');
                                List<string> codeList = new List<string>();
                                foreach (var item in codes)
                                {
                                    if(!string.IsNullOrEmpty(item))
                                        codeList.Add(item);
                                }
                                gps.Add(time.Date.ToShortDateString(), codeList);
                            }
                            line = streamReader.ReadLine();
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            return gps;
        }
        public static Dictionary<string, List<string>> GetAnalyseGPs()
        {
            Dictionary<string, List<string>> gps = new Dictionary<string, List<string>>();

            try
            {
                if (File.Exists(ConfigData.AnalyseGpsFilePath))
                {
                    FileInfo fileInfo = new FileInfo(ConfigData.AnalyseGpsFilePath);
                    using (StreamReader streamReader = fileInfo.OpenText())
                    {
                        string line = streamReader.ReadLine();
                        while (line != null)
                        {
                            var strs = line.Split(':');
                            if (strs.Count() == 2)
                            {
                                var times = strs[0].Split('-');
                                if (times.Count() != 3)
                                    times = strs[0].Split('/');
                                if (times.Count() != 3)
                                    continue;
                                DateTime time = new DateTime(int.Parse(times[0]), int.Parse(times[1]), int.Parse(times[2]));
                                var codes = strs[1].Split(';');
                                List<string> codeList = new List<string>();
                                foreach (var item in codes)
                                {
                                    if (!string.IsNullOrEmpty(item))
                                        codeList.Add(item);
                                }
                                gps.Add(time.Date.ToShortDateString(), codeList);
                            }
                            line = streamReader.ReadLine();
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            return gps;
        }

        public static void GetConfigData()
        {
            try
            {
                if (File.Exists(ConfigData.ConfigDataFilePath))
                {
                    string text = File.ReadAllText(ConfigData.ConfigDataFilePath);
                    var strs = text.Split(';');
                    ConfigData.MaxPrice = double.TryParse(strs[0], out double res) ? res : ConfigData.MaxPrice;
                    ConfigData.MinMarkValue = double.TryParse(strs[1], out double res1) ? res1 : ConfigData.MinMarkValue;
                    ConfigData.MinACCER = double.TryParse(strs[2], out double res2) ? res2 : ConfigData.MinACCER;
                    ConfigData.MinMarkUpForLocal = double.TryParse(strs[3], out double res3) ? res3 : ConfigData.MinMarkUpForLocal;
                    ConfigData.GPStartRecordTime = TimeSpan.TryParse(strs[4], out TimeSpan res4) ? res4 : ConfigData.GPStartRecordTime;
                    ConfigData.GPEndRecordTime = TimeSpan.TryParse(strs[5], out TimeSpan res5) ? res5 : ConfigData.GPEndRecordTime;
                    ConfigData.Show60GP = strs[6] == "1";
                    ConfigData.Show00GP = strs[7] == "1";
                    ConfigData.Show30GP = strs[8] == "1";
                    ConfigData.Show43GP = strs[9] == "1";
                    ConfigData.ShowSTGP = strs[10] == "1";
                    ConfigData.IsFilterGps = strs[11] == "1";
                    ConfigData.MinMarkUpForMidFast = double.TryParse(strs[12], out double res6) ? res6 : ConfigData.MinMarkUpForMidFast;
                }
            }
            catch (Exception)
            {
            }
        }

        public static bool WriteLockGps()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                foreach (var item in ConfigData.LockGPs)
                {
                    sb.Append(item + ";");
                }
                File.WriteAllText(ConfigData.LockGpsFilePath, sb.ToString());
            }
            catch (Exception)
            {

            }
            return true;
        }

        public static bool WriteMidFastUpGPs()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                foreach (var item in ConfigData.MidFastUpGPs)
                {
                    if(item.Value.Count > 0)
                    {
                        sb.Append(item.Key + ":");
                        foreach (var code in item.Value)
                        {
                            sb.Append(code + ";");
                        }
                        sb.Append("\r\n");
                    }
                }
                File.WriteAllText(ConfigData.MidFastGpsFilePath,sb.ToString());
            }
            catch (Exception)
            {

            }
            return true;
        }
        public static bool WriteAnalyseGPs()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                foreach (var item in ConfigData.AnalyseGPs)
                {
                    if (item.Value.Count > 0)
                    {
                        sb.Append(item.Key + ":");
                        foreach (var code in item.Value)
                        {
                            sb.Append(code + ";");
                        }
                        sb.Append("\r\n");
                    }
                }
                File.WriteAllText(ConfigData.AnalyseGpsFilePath, sb.ToString());
            }
            catch (Exception)
            {

            }
            return true;
        }

        public static bool WriteConfigData()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(ConfigData.MaxPrice.ToString() + ";");
                sb.Append(ConfigData.MinMarkValue.ToString() + ";");
                sb.Append(ConfigData.MinACCER.ToString() + ";");
                sb.Append(ConfigData.MinMarkUpForLocal.ToString() + ";");
                sb.Append(ConfigData.GPStartRecordTime.ToString() + ";");
                sb.Append(ConfigData.GPEndRecordTime.ToString() + ";");
                sb.Append((ConfigData.Show60GP?"1":"0")+ ";");
                sb.Append((ConfigData.Show00GP ? "1":"0")+ ";");
                sb.Append((ConfigData.Show30GP ? "1":"0")+ ";");
                sb.Append((ConfigData.Show43GP ? "1":"0")+ ";");
                sb.Append((ConfigData.ShowSTGP ? "1":"0")+ ";");
                sb.Append((ConfigData.IsFilterGps ? "1":"0")+ ";");
                sb.Append(ConfigData.MinMarkUpForMidFast.ToString() + ";");

                File.WriteAllText(ConfigData.ConfigDataFilePath, sb.ToString());
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}
