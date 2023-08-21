using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPSpeedView.Models
{
    public class GpDayEntity
    {
        public string Code { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// 日期
        /// </summary>
        public DateTime Time { get; set; }
        /// <summary>
        /// 开盘价
        /// </summary>
        public double PrePrice { get; set; }
        /// <summary>
        /// 收盘价
        /// </summary>
        public double ClsPrice { get; set; }
        /// <summary>
        /// 最高价
        /// </summary>
        public double HighPrice { get; set; }
        /// <summary>
        /// 最低价
        /// </summary>
        public double LowPrice { get; set; }
        /// <summary>
        /// 涨跌幅
        /// </summary>
        public double MarkUp { get; set; }
        /// <summary>
        /// 涨跌额
        /// </summary>
        public double MarkUpPrice { get; set; }
        /// <summary>
        /// 成交量
        /// </summary>
        public double Volume { get; set; }
        /// <summary>
        /// 成交额
        /// </summary>
        public double TurnVolume { get; set; }
        /// <summary>
        /// 振幅
        /// </summary>
        public double Amplitude { get; set; }
        /// <summary>
        /// 换手
        /// </summary>
        public double ChangeHands { get; set; }
    }
}
