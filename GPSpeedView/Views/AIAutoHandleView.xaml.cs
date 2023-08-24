using GPSpeedView.Models;
using Newtonsoft.Json;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GPSpeedView.Views
{
    /// <summary>
    /// AIAutoHandleView.xaml 的交互逻辑
    /// </summary>
    public partial class AIAutoHandleView : Window
    {
        private AiAutoHandleVModel m_VModel;
        public AIAutoHandleView()
        {
            InitializeComponent();
            m_VModel = new AiAutoHandleVModel(this);
            this.DataContext = m_VModel;
        }
    }

    public class AiAutoHandleVModel : BindableBase
    {
        private AIAutoHandleView m_View;

        private Dictionary<DateTime, List<GpDayEntity>> dicGpInfo = new Dictionary<DateTime, List<GpDayEntity>>();
        private Dictionary<string, List<GpDayEntity>> dicAllGpInfo = new Dictionary<string, List<GpDayEntity>>();

        public AccountEntity m_Account;
        public AiAutoHandleVModel(AIAutoHandleView view)
        {
            m_View = view;

            m_Account = new AccountEntity {Cash=200000,ValidBuyNum=2 };

            Thread thread = new Thread(Run);
            thread.Start();
        }

        private void Run()
        {
            DateTime startTime = new DateTime(2023, 1, 1);

            while (startTime != DateTime.Today)
            {
                dicGpInfo.Add(startTime, new List<GpDayEntity>());
                startTime.AddDays(1);
            }
            // 加载所有历史股票数据
            InitGpData();

            List<string> IntentionList = new List<string>();
            // 开始循环操作
            while (startTime != DateTime.Today)
            {
                DateTime tempTime = startTime;
                startTime = startTime.AddDays(1);

                if (dicGpInfo[startTime].Count == 0) continue;

                SimpleAutoExecute(startTime,IntentionList);
            }
        }
        private void InitGpData()
        {
            var files = Directory.GetFiles("HistoryInfo");
            foreach (var item in files)
            {
                string json = File.ReadAllText(item);
                List<GpDayEntity> list = JsonConvert.DeserializeObject<List<GpDayEntity>>(json);
                if (list.Count == 0)
                {
                    continue;
                }
                dicAllGpInfo.Add(list[0].Code, list);
                foreach (var keyValue in dicGpInfo)
                {
                    var temp = list.Find(x => x.Time == keyValue.Key);
                    if (temp != null)
                        keyValue.Value.Add(temp);
                }
            }
        }

        private void SimpleAutoExecute(DateTime time, List<string> IntentionList)
        {
            DateTime yestoday = time.AddDays(-1);
            // 已有持仓，判断是否处理
            for (int i = 0; i < m_Account.CurBuyInGps.Count; i++)
            {
                var temp = m_Account.CurBuyInGps[i];
                var nowEnt = dicAllGpInfo[temp.GpCode].Find(x => x.Time == time);
                var yesEnt = dicAllGpInfo[temp.GpCode].Find(x => x.Time == yestoday);
                // 低开，按开盘价卖出
                if (yesEnt.ClsPrice > nowEnt.PrePrice)
                {
                    PayOutGp(temp, nowEnt.PrePrice, time);
                    i--;
                    continue;
                }
                // 高开，如涨停，不处理，否则按最低价卖出
                if (nowEnt.MarkUp > 9.5)
                    continue;
                PayOutGp(temp, nowEnt.LowPrice, time);
                i--;
            }

            // 本日满仓，不买入，清除昨日意向
            if (m_Account.CurBuyInGps.Count == m_Account.ValidBuyNum)
            {
                IntentionList.Clear();
            }

            // 无意向，本日添加意向，无操作
            if (IntentionList.Count == 0)
            {
                foreach (var item in dicGpInfo[time])
                {
                    if (item.MarkUp > 9.5)
                    {
                        IntentionList.Add(item.Code);
                    }
                }
                return;
            }


        }

        private void BuyGp(string code,double price,int count,DateTime time)
        {
            BuyInGpEntity ent = new BuyInGpEntity();
            ent.BuyInPrice = price;
            ent.BuyInTime = time;
            ent.GpCount = count;
            ent.GpCode = code;
            m_Account.CurBuyInGps.Add(ent);
            m_Account.Cash -= price * count;
        }
        private void PayOutGp(BuyInGpEntity ent,double price,DateTime time)
        {
            ent.PayOutPrice = price;
            ent.PayOutTime = time;
            ent.IsPayOut = true;

            ent.Earnings = (ent.PayOutPrice - ent.BuyInPrice) * ent.GpCount;
            m_Account.HisBuyInGps.Add(ent);
            m_Account.CurBuyInGps.Remove(ent);
            m_Account.Cash += price * ent.GpCount;
        }
    }
}
