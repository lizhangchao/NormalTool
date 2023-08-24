using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPSpeedView.Models
{
    public class AccountEntity
    {
        /// <summary>
        /// 资产
        /// </summary>
        public double MarkValue
        {
            get 
            {
                double tempCash = m_Cash;
                foreach (var item in m_CurBuyInGps)
                {
                    tempCash += item.BuyInPrice * item.GpCount;
                }
                return tempCash; 
            }
        }

        private double m_Cash;
        /// <summary>
        /// 现金
        /// </summary>
        public double Cash
        {
            get { return m_Cash; }
            set { m_Cash = value; }
        }

        private int m_ValidBuyNum = 1;
        /// <summary>
        /// 可持仓数量
        /// </summary>
        public int ValidBuyNum
        {
            get { return m_ValidBuyNum; }
            set { m_ValidBuyNum = value; }
        }

        private List<BuyInGpEntity> m_CurBuyInGps = new List<BuyInGpEntity>();
        /// <summary>
        /// 当前持仓
        /// </summary>
        public List<BuyInGpEntity> CurBuyInGps
        {
            get { return m_CurBuyInGps; }
            set { m_CurBuyInGps = value; }
        }

        private List<BuyInGpEntity> m_HisBuyInGps = new List<BuyInGpEntity>();
        /// <summary>
        /// 历史持仓
        /// </summary>
        public List<BuyInGpEntity> HisBuyInGps
        {
            get { return m_HisBuyInGps; }
            set { m_HisBuyInGps = value; }
        }
    }

    public class BuyInGpEntity
    {
        public string GpCode { get; set; }
        /// <summary>
        /// 买入价格
        /// </summary>
        public double BuyInPrice { get; set; }
        /// <summary>
        /// 卖出价格
        /// </summary>
        public double PayOutPrice { get; set; }
        /// <summary>
        /// 股数
        /// </summary>
        public int GpCount { get; set; }
        /// <summary>
        /// 买入时间
        /// </summary>
        public DateTime BuyInTime { get; set; }
        /// <summary>
        /// 卖出时间
        /// </summary>
        public DateTime PayOutTime { get; set; }
        /// <summary>
        /// 盈亏
        /// </summary>
        public double Earnings { get; set; }
        /// <summary>
        /// 是否已卖出
        /// </summary>
        public bool IsPayOut { get; set; } = false;
    }

}
