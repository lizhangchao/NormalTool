using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Newtonsoft.Json.Linq;
using Prism;
using Prism.Mvvm;

namespace GPSpeedView
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Dictionary<int, MainWindow> DicMainView = new Dictionary<int, MainWindow>();
        public static MainWindow mainView;
        private MainViewModel m_ViewModel;
        public int m_ViewType = 0;
        public string m_Time;

        public static void ShowView(int viewType)
        {
            MainWindow view = null;
            if (DicMainView.ContainsKey(viewType))
            {
                view = DicMainView[viewType];
            }
            else
            {
                view = new MainWindow(viewType);
                if(viewType == 1)
                {
                    view.Left = mainView.Left - view.Width - 20;
                    view.Top = mainView.Top;
                }
                else if (viewType == 2)
                {
                    view.Left = mainView.Left;
                    view.Top = mainView.Top - view.Height - 20;
                }
            }
            view.Activate();
            if(view.WindowState == WindowState.Minimized)
            {
                view.WindowState = WindowState.Normal;
            }
            view.Show();
        }
        public MainWindow()
        {
            InitializeComponent();
            m_ViewModel = new MainViewModel(this);
            this.DataContext = m_ViewModel;
            m_ViewModel.OrderBy = SortType.ACCER;
            mainView = this;
        }

        // viewType  1: 历史选股  2: 自选股
        private MainWindow(int viewType)
        {
            m_ViewType = viewType;
            InitializeComponent();
            m_ViewModel = new MainViewModel(this);
            this.DataContext = m_ViewModel;
            if (viewType == 1)
            {
                this.Title = "盘中涨停票";
                SettingMenuItem.Visibility = Visibility.Collapsed;
                SortSettingMenuItem.Visibility = Visibility.Collapsed;
                RemoveListItem.Visibility = Visibility.Collapsed;
                ShowTodayItem.Visibility = Visibility.Collapsed;
                ColumnACCERInFive.Visibility = Visibility.Collapsed;
                ColumnACCER.Visibility = Visibility.Collapsed;
                SelectTimeItem.Visibility = Visibility.Visible;

                AddTimeItems();

            }
            else if (viewType == 2)
            {
                this.Title = "自选";
                SettingMenuItem.Visibility = Visibility.Collapsed;
                SortSettingMenuItem.Visibility = Visibility.Collapsed;
                RemoveListItem.Visibility = Visibility.Collapsed;
                ShowTodayItem.Visibility = Visibility.Collapsed;
                ColumnACCERInFive.Visibility = Visibility.Collapsed;
                ColumnACCER.Visibility = Visibility.Collapsed;
                LockListItem.Visibility = Visibility.Collapsed;
            }
            else
            {
                m_ViewModel.OrderBy = SortType.ACCER;
            }

        }

        private void AddTimeItems()
        {
            foreach (var item in ConfigData.MidFastUpGPs.Keys)
            {
                MenuItem menu = new MenuItem();
                menu.Header = item;
                menu.Click += SelectTimeItem_Click;
                SelectTimeItem.Items.Add(menu);
            }
        }
        private void SelectTimeItem_Click(object sender, RoutedEventArgs e)
        {
            var menu = sender as MenuItem;
            m_Time = menu.Header.ToString();
            m_ViewModel.InitDataForOldGps(m_Time);
        }
        private void SettingMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ConfigView view = new ConfigView();
            view.Show();
        }

        private void RemoveListItem_Click(object sender, RoutedEventArgs e)
        {
            RemoveListView view = new RemoveListView();
            view.Show();
        }

        private void GpDataGrid_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (GpDataGrid.SelectedItem != null)
            {
                var ent = GpDataGrid.SelectedItem as ViewEntity;
                var m_GPDetailView = new GPDetailView(ent.Code);
                m_GPDetailView.Show();
            }
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            if (GpDataGrid.SelectedItem != null)
            {
                var ent = GpDataGrid.SelectedItem as ViewEntity;
                if(m_ViewType == 1)
                {
                    if (ConfigData.MidFastUpGPs.ContainsKey(m_Time))
                    {
                        ConfigData.MidFastUpGPs[m_Time].RemoveAll(x => x == ent.Code);
                    }
                    m_ViewModel.InitDataForOldGps(m_Time);
                }
                else if (m_ViewType == 2)
                {
                    ConfigData.LockGPs.RemoveAll(x => x == ent.Code);
                    m_ViewModel.InitDataForLockGps();
                }
                else
                {
                    if (!ConfigData.FlagGps.Contains(ent.Name))
                    {
                        ConfigData.FlagGps.Add(ent.Name);
                    }
                }
            }
        }

        private void LockBtn_Click(object sender, RoutedEventArgs e)
        {
            if (GpDataGrid.SelectedItem != null)
            {
                var ent = GpDataGrid.SelectedItem as ViewEntity;

                if (!ConfigData.LockGPs.Contains(ent.Code))
                {
                    ConfigData.LockGPs.Add(ent.Code);
                }
            }
        }

        private void ShowTodayItem_Click(object sender, RoutedEventArgs e)
        {
            ShowView(1);
        }

        private void main_Closed(object sender, EventArgs e)
        {
            DataHelper.WriteMidFastUpGPs();
            DataHelper.WriteAnalyseGPs();
            DataHelper.WriteLockGps();
            DataHelper.WriteConfigData();
            if (DicMainView.ContainsKey(m_ViewType))
            {
                DicMainView.Remove(m_ViewType);
            }
        }

        private void LockListItem_Click(object sender, RoutedEventArgs e)
        {
            ShowView(2);
        }

        private void browenBtn_Click(object sender, RoutedEventArgs e)
        {
            if (GpDataGrid.SelectedItem != null)
            {
                var ent = GpDataGrid.SelectedItem as ViewEntity;
                string typeId = "";
                if (ent.Code.StartsWith("00"))
                {
                    typeId = "sz";
                }
                else if (ent.Code.StartsWith("60"))
                {
                    typeId = "sh";
                }
                else
                {
                    return;
                }
                string url = $"https://quote.eastmoney.com/{typeId}{ent.Code}.html";
                string path = @"C:\Program Files (x86)\Microsoft\Edge Beta\Application\msedge.exe";

                ProcessStartInfo psi = new ProcessStartInfo { FileName = path, Arguments = url };
                Process.Start(psi);
            }
        }

        private void ExportListItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 创建一个 Bitmap 对象，表示画布
                Bitmap bitmap = new Bitmap(1000, 1000);

                // 创建一个 Graphics 对象，用于在 Bitmap 上绘图
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    // 设置画布背景色
                    graphics.Clear(System.Drawing.Color.White);

                    // 设置绘图字体和颜色
                    Font font = new Font("Arial", 12);
                    SolidBrush brush = new SolidBrush(System.Drawing.Color.Black);

                    float y = 10f; // 纵坐标

                    float x = 10f;
                    // 在画布上逐个绘制成语
                    foreach (ViewEntity ent in m_ViewModel.GPData)
                    {
                        graphics.DrawString(ent.Code, font, brush, new PointF(x, y));
                        x += 100;
                        if (x > 1000)
                        {
                            x = 10;
                            y += 20;
                        }
                    }
                }

                // 保存绘制好的图像为文件
                bitmap.Save(@"D:\桌面文件\output.png", ImageFormat.Png);

                // 释放资源
                bitmap.Dispose();
            }
            catch (Exception)
            {
            }
            MessageBox.Show("导出成功");
        }
    }

    public class MainViewModel : BindableBase
    {
        private MainWindow m_View;

        private Timer timer = new Timer();
        private Timer backDataTimer = new Timer();
        //private string url1ForACCER = "http://74.push2.eastmoney.com/api/qt/clist/get?&pn=1&pz=40&po=1&np=1&fltt=2&invt=2&wbp2u=|0|0|0|web&fid=f22&fs=m:0+t:6,m:0+t:80,m:1+t:2,m:1+t:23,m:0+t:81+s:2048&fields=f1,f2,f3,f4,f5,f6,f7,f8,f9,f10,f12,f13,f14,f15,f16,f17,f18,f20,f21,f23,f24,f25,f22,f11,f62,f128,f136,f115,f152";
        //private string url1ForACCERInFive = "http://74.push2.eastmoney.com/api/qt/clist/get?&pn=1&pz=40&po=1&np=1&fltt=2&invt=2&wbp2u=|0|0|0|web&fid=f11&fs=m:0+t:6,m:0+t:80,m:1+t:2,m:1+t:23,m:0+t:81+s:2048&fields=f1,f2,f3,f4,f5,f6,f7,f8,f9,f10,f12,f13,f14,f15,f16,f17,f18,f20,f21,f23,f24,f25,f22,f11,f62,f128,f136,f115,f152";
        private string url1ForACCER = "http://74.push2.eastmoney.com/api/qt/clist/get?&pn=1&pz=40&po=1&np=1&fltt=2&invt=2&wbp2u=|0|0|0|web&fid=f22&fs=m:0+t:6,m:0+t:80,m:1+t:2,m:1+t:23,m:0+t:81+s:2048&fields=f2,f3,f12,f14,f20,f22,f11";
        private string url1ForACCERInFive = "http://74.push2.eastmoney.com/api/qt/clist/get?&pn=1&pz=40&po=1&np=1&fltt=2&invt=2&wbp2u=|0|0|0|web&fid=f11&fs=m:0+t:6,m:0+t:80,m:1+t:2,m:1+t:23,m:0+t:81+s:2048&fields=f2,f3,f12,f14,f20,f22,f11";

        private int total;
        private bool hasWrited = false;
        private ObservableCollection<ViewEntity> m_GPData = new ObservableCollection<ViewEntity>();

        public ObservableCollection<ViewEntity> GPData
        {
            get { return m_GPData; }
            set 
            { 
                m_GPData = value;
                RaisePropertyChanged();
            }
        }

        private SortType m_OrderBy;

        public SortType OrderBy
        {
            get { return m_OrderBy; }
            set 
            { 
                m_OrderBy = value;
                if(m_OrderBy == SortType.ACCER)
                {
                    m_View.ColumnACCER.Visibility = Visibility.Visible;
                    m_View.ColumnACCERInFive.Visibility = Visibility.Collapsed;
                }
                else
                {
                    m_View.ColumnACCER.Visibility = Visibility.Collapsed;
                    m_View.ColumnACCERInFive.Visibility = Visibility.Visible;
                }
                RaisePropertyChanged();
            }
        }

        public string OldListTime { get; set; }

        public MainViewModel(MainWindow view)
        {
            m_View = view;
            DataHelper.GetConfigData();
            if (m_View.m_ViewType == 1)
            {
                view.m_Time = DateTime.Now.Date.ToShortDateString();
                InitDataForOldGps(view.m_Time);
            }
            else if (m_View.m_ViewType == 2)
            {
                InitDataForLockGps();
            }
            else
            {
                InitData();
                timer.Interval = 3000;
                timer.Elapsed -= FlashData;
                timer.Elapsed += FlashData;
                timer.Start();

                //一分钟获取一次数据
                backDataTimer.Interval = 1000;
                backDataTimer.Elapsed -= SufferData;
                backDataTimer.Elapsed += SufferData;
                backDataTimer.Start();
            }
        }
        public void InitDataForOldGps(string time)
        {
            DateTime now = DateTime.Now;
            DateTime tdStopTime = now.Date + ConfigData.GpStopTime;

            if (ConfigData.MidFastUpGPs.ContainsKey(time))
            {
                GPData.Clear();
                int index = 1;
                var gps = ConfigData.MidFastUpGPs[time];
                if(DateTime.Compare(now, tdStopTime) > 0 
                    && ConfigData.MidFastUpGoDataInfo != null 
                    && ConfigData.MidFastUpGoDataInfo.ContainsKey(time)
                    && ConfigData.MidFastUpGoDataInfo[time].Count > 0)
                {
                    GPData = new ObservableCollection<ViewEntity>(ConfigData.MidFastUpGoDataInfo[time]);
                }
                else
                {
                    foreach (var code in gps)
                    {
                        int typeId = 0;
                        if (code.StartsWith("00"))
                        {
                            typeId = 0;
                        }
                        else if (code.StartsWith("60"))
                        {
                            typeId = 1;
                        }
                        else
                        {
                            continue;
                        }
                        string url = $"https://15.push2.eastmoney.com/api/qt/stock/get?fltt=2&invt=2&volt=2&fields=f43,f57,f58,f169,f170,f46,f44,f45&secid={typeId}.{code}";
                        var ent = GetGPDataFOrOne(url).Result;
                        ent.Num = index++;
                        GPData.Add(ent);
                    }
                    if(ConfigData.MidFastUpGoDataInfo == null)
                    {
                        ConfigData.MidFastUpGoDataInfo = new Dictionary<string, List<ViewEntity>>();
                    }
                    if (ConfigData.MidFastUpGoDataInfo.ContainsKey(time))
                    {
                        ConfigData.MidFastUpGoDataInfo[time] = new List<ViewEntity>(GPData.ToList());
                    }
                    else
                    {
                        ConfigData.MidFastUpGoDataInfo.Add(time, new List<ViewEntity>(GPData.ToList()));
                    }
                }
            }
        }
        public void InitDataForLockGps()
        {
            DateTime now = DateTime.Now;
            DateTime tdStopTime = now.Date + ConfigData.GpStopTime;

            GPData.Clear();
            int index = 1;
            if (DateTime.Compare(now, tdStopTime) > 0
    && ConfigData.LockGpDataInfo != null
    && ConfigData.LockGpDataInfo.Count > 0)
            {
                GPData = new ObservableCollection<ViewEntity>(ConfigData.LockGpDataInfo);
            }
            else
            {
                foreach (var code in ConfigData.LockGPs)
                {
                    int typeId = 0;
                    if (code.StartsWith("00"))
                    {
                        typeId = 0;
                    }
                    else if (code.StartsWith("60"))
                    {
                        typeId = 1;
                    }
                    else
                    {
                        continue;
                    }
                    string url = $"https://15.push2.eastmoney.com/api/qt/stock/get?fltt=2&invt=2&volt=2&fields=f43,f57,f58,f169,f170,f46,f44,f45&secid={typeId}.{code}";
                    var ent = GetGPDataFOrOne(url).Result;
                    ent.Num = index++;
                    GPData.Add(ent);
                }
                ConfigData.LockGpDataInfo = new List<ViewEntity>(GPData.ToList());
            }
        }
        public void InitData()
        {
            FlashData(null,null);
        }

        private bool IsResponsing = false;
        public void FlashData(object sender, ElapsedEventArgs e)
        {
            if (IsResponsing)
                return;
            string url = url1ForACCER;
            if (OrderBy == SortType.ACCERInFive)
            {
                url = url1ForACCERInFive;
            }

            try
            {
                IsResponsing = true;

                var ents = GetGPData(url).Result;
                if (OrderBy == SortType.ACCER)
                {
                    ents = ents.OrderByDescending(x => x.CurAccer).ToList();
                }
                else
                {
                    ents = ents.OrderByDescending(x => x.CurAccerInFive).ToList();
                }

                for (int i = 0; i < ents.Count; i++)
                {
                    if (!ConfigData.Show60GP && ents[i].Code.StartsWith("60"))
                    {
                        ents.RemoveAt(i--);
                        continue;
                    }
                    if (!ConfigData.Show00GP && ents[i].Code.StartsWith("00"))
                    {
                        ents.RemoveAt(i--);
                        continue;
                    }
                    if (!ConfigData.Show30GP && ents[i].Code.StartsWith("30"))
                    {
                        ents.RemoveAt(i--);
                        continue;
                    }
                    if (!ConfigData.Show43GP && ents[i].Code.StartsWith("43"))
                    {
                        ents.RemoveAt(i--);
                        continue;
                    }
                    if (!ConfigData.ShowSTGP && ents[i].Name.Contains("ST"))
                    {
                        ents.RemoveAt(i--);
                        continue;
                    }

                    if (!ents[i].Code.StartsWith("60")
                        && !ents[i].Code.StartsWith("00")
                        && !ents[i].Code.StartsWith("30")
                        && !ents[i].Code.StartsWith("43"))
                    {
                        ents.RemoveAt(i--);
                        continue;
                    }
                    if (ents[i].CurPrice > ConfigData.MaxPrice)
                    {
                        ents.RemoveAt(i--);
                        continue;
                    }
                    if (ents[i].CurMarkValue < ConfigData.MinMarkValue)
                    {
                        ents.RemoveAt(i--);
                        continue;
                    }
                    if (ConfigData.FlagGps.Contains(ents[i].Name))
                    {
                        ents.RemoveAt(i--);
                        continue;
                    }
                }
                int index = 1;
                ents.ForEach(x => x.Num = index++);
                GetMidUpGp(ents);

                GPData = new ObservableCollection<ViewEntity>(ents);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                IsResponsing = false;

            }
            IsResponsing = false;
        }

        private void SufferData(object sender, ElapsedEventArgs e)
        {
            DateTime now = DateTime.Now;

            DateTime tdStartTime = now.Date + ConfigData.GpStartTime;
            DateTime tdStopTime = now.Date + ConfigData.GpStopTime;
            string url = $"http://74.push2.eastmoney.com/api/qt/clist/get?&pn=1&pz={total}&po=1&np=1&fltt=2&invt=2&wbp2u=|0|0|0|web&fid=f22&fs=m:0+t:6,m:0+t:80,m:1+t:2,m:1+t:23,m:0+t:81+s:2048&fields=f2,f3,f12,f14,f20,f22,f11";
            var ents = GetGPData(url).Result;

            foreach (var item in ents)
            {
                if (ConfigData.TotalGpSufferData.ContainsKey(item.Code))
                {
                    ConfigData.TotalGpSufferData[item.Code].Add(item.CurMarkUp);
                }
                else
                {
                    ConfigData.TotalGpSufferData.Add(item.Code, new List<double> { item.CurMarkUp });
                }
            }
        }
        private async Task<List<DetailEntity>> GetDetailData(string code)
        {
            List<DetailEntity> prices = new List<DetailEntity>();
            int typeId = 0;
            if (code.StartsWith("00"))
            {
                typeId = 0;
            }
            else if (code.StartsWith("60"))
            {
                typeId = 1;
            }
            else
            {
                return prices;
            }
            string url = $"https://push2.eastmoney.com/api/qt/stock/trends2/get?fields1=f1,f2,f3,f4,f5,f6,f7,f8,f9,f10,f11,f12,f13&fields2=f51,f52,f53,f54,f55,f56,f57,f58&iscr=0&ndays=1&secid={typeId}.{code}";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(url).ConfigureAwait(false);
                    if (response.IsSuccessStatusCode)
                    {
                        response.EnsureSuccessStatusCode();
                        string htmlContent = response.Content.ReadAsStringAsync().Result;
                        if (!string.IsNullOrEmpty(htmlContent))
                        {
                            JObject jobject = JObject.Parse(htmlContent);
                            var PrePrice = double.Parse(jobject["data"]["prePrice"].ToString());
                            var Name = jobject["data"]["name"].ToString();
                            var trends = jobject["data"]["trends"].Children();
                            int index = 1;
                            foreach (var item in trends)
                            {
                                var strs = item.ToString().Split(',');
                                if (strs.Count() == 8)
                                {
                                    DetailEntity ent = new DetailEntity();
                                    var datesStr = strs[0].Split(' ');
                                    var dates = datesStr[0].Split('-');
                                    var times = datesStr[1].Split(':');
                                    ent.EntTimeSpan = new DateTime(int.Parse(dates[0]), int.Parse(dates[1]), int.Parse(dates[2]), int.Parse(times[0]), int.Parse(times[1]), 0);
                                    ent.Price = double.Parse(strs[2]);
                                    double markUp = (ent.Price - PrePrice) / PrePrice * 100.00;
                                    ent.MarkUp = Math.Round(markUp, 2);
                                    ent.Index = index++;
                                    prices.Add(ent);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return prices;
        }
        private async Task<List<ViewEntity>> GetGPData(string url)
        {
            List<ViewEntity> ents = new List<ViewEntity>();
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    response.EnsureSuccessStatusCode();
                    string htmlContent = response.Content.ReadAsStringAsync().Result;
                    if (!string.IsNullOrEmpty(htmlContent))
                    {
                        JObject jobject = JObject.Parse(htmlContent);
                        var childrens = jobject["data"]["diff"].Children();
                        string totalStr = jobject["data"]["total"].ToString();
                        if (!string.IsNullOrEmpty(totalStr))
                        {
                            int.TryParse(totalStr, out total);
                        }
                        
                        foreach (var item in childrens)
                        {
                            ViewEntity ent = new ViewEntity();
                            ent.Code = item["f12"].ToString();
                            ent.Name = item["f14"].ToString();
                            ent.CurPrice = double.TryParse(item["f2"].ToString(), out double curPrice) ? curPrice : 0.0;
                            ent.CurMarkUp = double.TryParse(item["f3"].ToString(), out double curMarkup) ? curMarkup : 0.0;
                            ent.CurAccer = double.TryParse(item["f22"].ToString(), out double curAccer) ? curAccer : 0.0;
                            ent.CurAccerInFive = double.TryParse(item["f11"].ToString(), out double curAccerInFive) ? curAccerInFive : 0.0;
                            ent.CurMarkValue = double.TryParse(item["f20"].ToString(), out double curMarkValue) ? curMarkValue / 100000000.00 : 0.0;
                            ents.Add(ent);
                        }
                    }
                }
            }
            return ents;
        }

        //f43 收盘价  f44 最高价  f45 最低价  f46 今天开盘价  f57 code  f58 name  f169 涨跌价 f170 涨跌幅
        private async Task<ViewEntity> GetGPDataFOrOne(string url)
        {
            ViewEntity ent = new ViewEntity();
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    response.EnsureSuccessStatusCode();
                    string htmlContent = response.Content.ReadAsStringAsync().Result;
                    if (!string.IsNullOrEmpty(htmlContent))
                    {
                        JObject jobject = JObject.Parse(htmlContent);
                        
                        ent.Code = jobject["data"]["f57"].ToString();
                        ent.Name = jobject["data"]["f58"].ToString();
                        ent.CurPrice = double.TryParse(jobject["data"]["f43"].ToString(), out double curPrice) ? curPrice : 0.0;
                        ent.CurMarkUp = double.TryParse(jobject["data"]["f170"].ToString(), out double curMarkup) ? curMarkup : 0.0;
                        ent.HighPrice = double.TryParse(jobject["data"]["f44"].ToString(), out double highPrice) ? highPrice : 0.0;
                        ent.LowPrice = double.TryParse(jobject["data"]["f45"].ToString(), out double lowPrice) ? lowPrice : 0.0;
                        ent.PrePrice = double.TryParse(jobject["data"]["f46"].ToString(), out double prePrice) ? prePrice : 0.0;
                    }
                }
            }
            return ent;
        }
        private void GetMidUpGp(List<ViewEntity> GPList)
        {
            DateTime now = DateTime.Now;
            DateTime tdTargetStartTime = now.Date + ConfigData.GPStartRecordTime;
            DateTime tdTargetEndTime = now.Date + ConfigData.GPEndRecordTime;
            DateTime tdStopTime = now.Date + ConfigData.GpStopTime;

            List<string> midFastGpsList = new List<string>();
            if (ConfigData.MidFastUpGPs.ContainsKey(now.Date.ToShortDateString()))
            {
                midFastGpsList = ConfigData.MidFastUpGPs[now.Date.ToShortDateString()];
            }
            else
            {
                ConfigData.MidFastUpGPs.Add(now.Date.ToShortDateString(), midFastGpsList);
            }

            List<string> analyseGpsList = new List<string>();
            if (ConfigData.AnalyseGPs.ContainsKey(now.Date.ToShortDateString()))
            {
                analyseGpsList = ConfigData.AnalyseGPs[now.Date.ToShortDateString()];
            }
            else
            {
                ConfigData.AnalyseGPs.Add(now.Date.ToShortDateString(), analyseGpsList);
            }

            if (DateTime.Compare(now, tdTargetStartTime) > 0
                && DateTime.Compare(now, tdTargetEndTime) < 0)
            {
                foreach (var item in GPList)
                {
                    if(OrderBy == SortType.ACCER)
                    {
                        if (item.CurAccer >= ConfigData.MinACCER
                             && (!ConfigData.IsFilterGps || (ConfigData.IsFilterGps && item.CurMarkUp >= ConfigData.MinMarkUpForMidFast)))
                        {
                            if (midFastGpsList.Find(x => x == item.Code) == null)
                            {
                                midFastGpsList.Add(item.Code);
                            }
                        }
                        bool isSucc = SelectAnaylseGp(item, item.CurAccer);
                        item.IsAnalyseGp = isSucc;
                        if (isSucc)
                        {
                            if (analyseGpsList.Find(x => x == item.Code) == null)
                            {
                                analyseGpsList.Add(item.Code);
                            }
                        }
                    }
                    else
                    {
                        if (item.CurAccerInFive >= ConfigData.MinACCER
                             && (!ConfigData.IsFilterGps || (ConfigData.IsFilterGps && item.CurMarkUp >= ConfigData.MinMarkUpForMidFast)))
                        {
                            if (midFastGpsList.Find(x => x == item.Code) == null)
                            {
                                midFastGpsList.Add(item.Code);
                            }
                        }
                        bool isSucc = SelectAnaylseGp(item, item.CurAccerInFive);
                        item.IsAnalyseGp = isSucc;
                        if (isSucc)
                        {
                            if (analyseGpsList.Find(x => x == item.Code) == null)
                            {
                                analyseGpsList.Add(item.Code);
                            }
                        }
                    }
                }
            }
            else if(DateTime.Compare(now, tdStopTime) > 0 && !hasWrited)
            {
                hasWrited = true;
                if (ConfigData.IsMinMarkUpForLocalEnable)
                {
                    string url = $"http://74.push2.eastmoney.com/api/qt/clist/get?&pn=1&pz={total}&po=1&np=1&ut=bd1d9ddb04089700cf9c27f6f7426281&fltt=2&invt=2&wbp2u=|0|0|0|web&fid=f22&fs=m:0+t:6,m:0+t:80,m:1+t:2,m:1+t:23,m:0+t:81+s:2048&fields=f1,f2,f3,f4,f5,f6,f7,f8,f9,f10,f12,f13,f14,f15,f16,f17,f18,f20,f21,f23,f24,f25,f22,f11,f62,f128,f136,f115,f152";
                    GPList = GetGPData(url).Result;
                    for (int i = 0; i < midFastGpsList.Count; i++)
                    {
                        var temp = GPList.Find(x => x.Code == midFastGpsList[i]);
                        if (temp == null || temp.CurMarkUp < ConfigData.MinMarkUpForLocal)
                        {
                            midFastGpsList.RemoveAt(i--);
                        }
                    }
                }

                //WriteMidUpGpToFile(GPList);
                //timer.Stop();
            }
        }
        /// <summary>
        /// 筛选优股
        /// </summary>
        /// <param name="gp"></param>
        private bool SelectAnaylseGp(ViewEntity gp,double accer)
        {
            if (accer >= ConfigData.MinACCER && gp.CurMarkUp >= ConfigData.MinMarkUpForMidFast)
            {
                if (!ConfigData.TotalGpSufferData.ContainsKey(gp.Code))
                {
                    var res = GetDetailData(gp.Code).Result;

                    ConfigData.TotalGpSufferData.Add(gp.Code, res.Select(x => x.MarkUp).ToList());
                }
                if (ConfigData.TotalGpSufferData.ContainsKey(gp.Code)
                    && ConfigData.TotalGpSufferData[gp.Code].Count > 5)
                {
                    var list = new List<double>(ConfigData.TotalGpSufferData[gp.Code]);
                    list.RemoveRange(list.Count - 5, 5);
                    if(list.Max() - list.Min() < 2)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }

    public class ViewEntity : BindableBase
    {
        private int m_Num;

        public int Num
        {
            get { return m_Num; }
            set 
            { 
                m_Num = value;
                RaisePropertyChanged();
            }
        }
        private string m_Code;

        public string Code
        {
            get { return m_Code; }
            set
            {
                m_Code = value;
                RaisePropertyChanged();
            }
        }
        private string m_Name;

        public string Name
        {
            get { return m_Name; }
            set
            {
                m_Name = value;
                RaisePropertyChanged();
            }
        }
        private double m_CurPrice;

        public double CurPrice
        {
            get { return m_CurPrice; }
            set
            {
                m_CurPrice = value;
                RaisePropertyChanged();
            }
        }
        private double m_CurMarkUp;
        /// <summary>
        /// 涨幅
        /// </summary>
        public double CurMarkUp
        {
            get { return m_CurMarkUp; }
            set
            {
                m_CurMarkUp = value;
                RaisePropertyChanged();
            }
        }
        private double m_CurAccer;
        /// <summary>
        /// 涨速
        /// </summary>
        public double CurAccer
        {
            get { return m_CurAccer; }
            set
            {
                m_CurAccer = value;
                RaisePropertyChanged();
            }
        }
        private double m_CurAccerInFive;
        /// <summary>
        /// 五分钟涨速
        /// </summary>
        public double CurAccerInFive
        {
            get { return m_CurAccerInFive; }
            set
            {
                m_CurAccerInFive = value;
                RaisePropertyChanged();
            }
        }

        private bool m_IsAnalyseGp;
        /// <summary>
        /// 优股
        /// </summary>
        public bool IsAnalyseGp
        {
            get { return m_IsAnalyseGp; }
            set
            {
                m_IsAnalyseGp = value;
                RaisePropertyChanged();
            }
        }

        private double m_CurMarkValue;
        /// <summary>
        /// 市值
        /// </summary>
        public double CurMarkValue
        {
            get { return m_CurMarkValue; }
            set
            {
                m_CurMarkValue = value;
                RaisePropertyChanged();
            }
        }
        private System.Windows.Media.Brush m_BackColor = new SolidColorBrush(Colors.White);

        public System.Windows.Media.Brush BackColor
        {
            get 
            { 
                if(m_CurAccer > ConfigData.MinACCER)
                {
                    if(IsAnalyseGp)
                        return new SolidColorBrush(Colors.Orange);
                    return new SolidColorBrush(Colors.Red);
                }
                else
                {
                    return new SolidColorBrush(Colors.White);
                }
            }
            set
            {
                m_BackColor = value;
                RaisePropertyChanged();
            }
        }
        private System.Windows.Media.Brush m_BackColorInFive = new SolidColorBrush(Colors.White);

        public System.Windows.Media.Brush BackColorInFive
        {
            get
            {
                if (m_CurAccerInFive > ConfigData.MinACCER)
                {
                    if (IsAnalyseGp)
                        return new SolidColorBrush(Colors.Orange);
                    return new SolidColorBrush(Colors.Red);
                }
                else
                {
                    return new SolidColorBrush(Colors.White);
                }
            }
            set
            {
                m_BackColorInFive = value;
                RaisePropertyChanged();
            }
        }
        private System.Windows.Media.Brush m_BackColorForMarkUp = new SolidColorBrush(Colors.White);

        public System.Windows.Media.Brush BackColorForMarkUp
        {
            get
            {
                if (m_CurMarkUp >= 0)
                {
                    return new SolidColorBrush(Colors.Red);
                }
                else
                {
                    return new SolidColorBrush(Colors.LightGreen);
                }
            }
            set
            {
                m_BackColorForMarkUp = value;
                RaisePropertyChanged();
            }
        }

        
        private double m_HighPrice;
        /// <summary>
        /// 最高价
        /// </summary>
        public double HighPrice
        {
            get { return m_HighPrice; }
            set { m_HighPrice = value; }
        }

        private double m_LowPrice;
        /// <summary>
        /// 最低价
        /// </summary>
        public double LowPrice
        {
            get { return m_LowPrice; }
            set { m_LowPrice = value; }
        }

        private double m_PrePrice;
        /// <summary>
        /// 开盘价
        /// </summary>
        public double PrePrice
        {
            get { return m_PrePrice; }
            set { m_PrePrice = value; }
        }
    }
}
