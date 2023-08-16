using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPSpeedView
{
    public class ConfigData
    {
        public static string LockGpsFilePath = "DataTxt//LockGpsData.txt";
        public static string MidFastGpsFilePath = "DataTxt//MidFastGpsData.txt";
        public static string AnalyseGpsFilePath = "DataTxt//AnalyseGpsData.txt";
        public static string ConfigDataFilePath = "DataTxt//Config.txt";


        public static TimeSpan GpStartTime = new TimeSpan(9, 30, 0);
        public static TimeSpan GpStopTime = new TimeSpan(15, 0, 0);

        // 标记隐藏股
        public static List<string> FlagGps = new List<string>();
        // 最大股价
        public static double MaxPrice = 35.0;
        // 最小市值
        public static double MinMarkValue = 0;
        // 最小涨速
        public static double MinACCER = 2;

        // 盘后记录股

        // 记录盘尾最小涨幅
        public static double MinMarkUpForLocal = 9.0;
        // 是否根据最小涨幅过滤
        public static bool IsMinMarkUpForLocalEnable = false;
        // 开始记录时刻
        public static TimeSpan GPStartRecordTime = new TimeSpan(10,0,0);
        // 结束记录时刻
        public static TimeSpan GPEndRecordTime = new TimeSpan(14, 40, 0);

        public static bool Show60GP = true;
        public static bool Show00GP = true;
        public static bool Show30GP = false;
        public static bool Show43GP = false;
        public static bool ShowSTGP = false;

        public static bool IsFilterGps = true;

        public static double MinMarkUpForMidFast = 5.0;

        private static List<string> m_LockGPs;
        /// <summary>
        /// 自选股集合
        /// </summary>
        public static List<string> LockGPs
        {
            get 
            {
                if(m_LockGPs == null)
                {
                    m_LockGPs = DataHelper.GetLockGPs();
                }
                return m_LockGPs; 
            }
        }

        private static Dictionary<string, List<string>> m_MidFastUpGPs;
        /// <summary>
        /// 盘中速涨盘尾涨停股
        /// </summary>
        public static Dictionary<string, List<string>> MidFastUpGPs
        {
            get
            {
                if (m_MidFastUpGPs == null)
                {
                    m_MidFastUpGPs = DataHelper.GetMidFastUpGPs();
                }
                return m_MidFastUpGPs;
            }
        }

        private static Dictionary<string, List<string>> m_AnalyseGPs;
        /// <summary>
        /// 分析优选股
        /// </summary>
        public static Dictionary<string, List<string>> AnalyseGPs
        {
            get
            {
                if (m_AnalyseGPs == null)
                {
                    m_AnalyseGPs = DataHelper.GetAnalyseGPs();
                }
                return m_AnalyseGPs;
            }
        }

        #region 数据缓存
        /// <summary>
        /// 自选股缓存数据
        /// </summary>
        public static List<ViewEntity> LockGpDataInfo;
        /// <summary>
        /// 盘中速涨股缓存数据
        /// </summary>
        public static Dictionary<string, List<ViewEntity>> MidFastUpGoDataInfo;

        public static Dictionary<string, List<double>> TotalGpSufferData = new Dictionary<string, List<double>>();
        #endregion
    }

    public class GPEntity
    {
        public string Code { get; set; }

        public DateTime Time { get; set; }
    }
    public enum SortType
    {
        ACCER,
        ACCERInFive
    }

    // test commnit
    // test commnit
}
