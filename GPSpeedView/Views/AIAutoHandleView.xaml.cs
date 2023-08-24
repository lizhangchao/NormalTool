using GPSpeedView.Models;
using Newtonsoft.Json;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Linq;

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

        private ObservableCollection<string> m_MsgList = new ObservableCollection<string>();

        public ObservableCollection<string> MsgList
        {
            get { return m_MsgList; }
            set 
            { 
                m_MsgList = value;
                RaisePropertyChanged();
            }
        }

        public AiAutoHandleVModel(AIAutoHandleView view)
        {
            m_View = view;

            m_Account = new AccountEntity {Cash=200000,ValidBuyNum=2 };

            Thread thread = new Thread(Run);
            thread.Start();
        }

        private void Run()
        {
            try
            {
                DateTime startTime = new DateTime(2023, 1, 1);

                while (startTime != DateTime.Today)
                {
                    dicGpInfo.Add(startTime, new List<GpDayEntity>());
                    startTime = startTime.AddDays(1);
                }
                // 加载所有历史股票数据
                InitGpData();

                List<string> IntentionList = new List<string>();
                startTime = new DateTime(2023, 1, 1);
                // 开始循环操作
                while (startTime != DateTime.Today)
                {
                    DateTime tempTime = startTime;
                    startTime = startTime.AddDays(1);

                    if (dicGpInfo[startTime].Count == 0) continue;

                    SimpleAutoExecute(startTime, IntentionList);

                    m_View.listBox.Dispatcher.Invoke(new Action(() =>
                    {
                        MsgList.Add(string.Format("日期【{0}】 今日资产【{1}】", tempTime, m_Account.MarkValue));

                    }));
                }
            }
            catch (Exception ex)
            {
                throw;
                //MessageBox.Show(ex.Message);
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
            // 已有持仓，判断是否处理
            for (int i = 0; i < m_Account.CurBuyInGps.Count; i++)
            {
                var temp = m_Account.CurBuyInGps[i];
                var nowEnt = dicAllGpInfo[temp.GpCode].Find(x => x.Time == time);
                int nowIndex = dicAllGpInfo[temp.GpCode].IndexOf(nowEnt);
                var yesEnt = dicAllGpInfo[temp.GpCode][nowIndex - 1];
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
            List<string> highGp = new List<string>();
            List<string> midGp = new List<string>();
            List<string> lowGp = new List<string>();
            foreach (var item in IntentionList)
            {
                var nowGp =  dicAllGpInfo[item].Find(x => x.Time == time);
                int indexNow = dicAllGpInfo[item].IndexOf(nowGp);
                var yesGp = dicAllGpInfo[item][indexNow - 1];
                if (nowGp.PrePrice < yesGp.ClsPrice)
                    continue;
                var preMarkUp = (nowGp.PrePrice - yesGp.ClsPrice) * 100.0/ yesGp.ClsPrice;
                if (preMarkUp >= 2 && preMarkUp < 5)
                {
                    highGp.Add(item);
                }
                else if(preMarkUp >= 5 && preMarkUp < 8)
                {
                    midGp.Add(item);
                }
                else if(preMarkUp >= 0 && preMarkUp < 2)
                {
                    lowGp.Add(item);
                }
            }
            IntentionList.Clear();
            foreach (var item in highGp)
            {
                if (m_Account.CurBuyInGps.Count == m_Account.ValidBuyNum)
                    return;
                var tempGp = dicGpInfo[time].Find(x => x.Code == item);
                double curCash = 0;
                if (m_Account.CurBuyInGps.Count == 0)
                {
                    curCash = m_Account.Cash % 2 == 1 ? m_Account.Cash / 2 + 1 : m_Account.Cash / 2;
                }
                else
                {
                    curCash = m_Account.Cash;
                }
                int count = (int)Math.Round(curCash / (tempGp.PrePrice * 100), 0, MidpointRounding.AwayFromZero) * 100;
                BuyGp(item, tempGp.Name, tempGp.PrePrice, count, time);
            }
            foreach (var item in midGp)
            {
                if (m_Account.CurBuyInGps.Count == m_Account.ValidBuyNum)
                    return;
                var tempGp = dicGpInfo[time].Find(x => x.Code == item);
                double curCash = 0;
                if (m_Account.CurBuyInGps.Count == 0)
                {
                    curCash = m_Account.Cash % 2 == 1 ? m_Account.Cash / 2 + 1 : m_Account.Cash / 2;
                }
                else
                {
                    curCash = m_Account.Cash;
                }
                int count = (int)Math.Round(curCash / (tempGp.PrePrice * 100), 0, MidpointRounding.AwayFromZero) * 100;
                BuyGp(item, tempGp.Name, tempGp.PrePrice, count, time);
            }
            foreach (var item in lowGp)
            {
                if (m_Account.CurBuyInGps.Count == m_Account.ValidBuyNum)
                    return;
                var tempGp = dicGpInfo[time].Find(x => x.Code == item);
                double curCash = 0;
                if (m_Account.CurBuyInGps.Count == 0)
                {
                    curCash = m_Account.Cash % 2 == 1 ? m_Account.Cash / 2 + 1 : m_Account.Cash / 2;
                }
                else
                {
                    curCash = m_Account.Cash;
                }
                int count = (int)Math.Round(curCash / (tempGp.PrePrice * 100), 0, MidpointRounding.AwayFromZero) * 100;
                BuyGp(item, tempGp.Name, tempGp.PrePrice, count, time);
            }
        }

        private void BuyGp(string code,string name,double price,int count,DateTime time)
        {
            BuyInGpEntity ent = new BuyInGpEntity();
            ent.BuyInPrice = price;
            ent.BuyInTime = time;
            ent.GpCount = count;
            ent.GpCode = code;
            ent.Name = name;
            m_Account.CurBuyInGps.Add(ent);
            m_Account.Cash -= price * count;

            m_View.listBox.Dispatcher.Invoke(new Action(() =>
            {
                MsgList.Add(string.Format("买入【{0}】（{1}）,买入价格【{2}】，数量【{3}】，总价【{4}】", name, code, price, count, price * count));

            }));
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
            m_View.listBox.Dispatcher.Invoke(new Action(() =>
            {
                MsgList.Add(string.Format("卖出【{0}】（{1}）,卖出价格【{2}】，数量【{3}】，总价【{4}】，盈亏【{5}】",
    ent.Name, ent.GpCode, price, ent.GpCount, price * ent.GpCount, ent.Earnings));
            }));

        }
    }
}
