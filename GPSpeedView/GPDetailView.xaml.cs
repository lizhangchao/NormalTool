using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GPSpeedView
{
    
    /// <summary>
    /// GPDetailView.xaml 的交互逻辑
    /// </summary>
    public partial class GPDetailView : Window
    {
        private string GPCode;
        private double PrePrice;
        private string Name;
        private DispatcherTimer timer = new DispatcherTimer();

        private double AcWidth = 360;
        private double AcHeight = 260;

        public GPDetailView(string code)
        {
            InitializeComponent();
            canvasBorder.Width = AcWidth;
            canvasBorder.Height = AcHeight;
            GPCode = code;
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }
        private bool IsResponsing = false;
        private void Timer_Tick(object sender, EventArgs e)
        {
            var list = GetData().Result;

            //chart.DataSource = null;
            //chart.DataSource = list;
            //title.Content = Name + "(" + GPCode + ")";
            var curMarkup = list.Last().MarkUp;
            CurGPLabel.Content = Name + "(" + GPCode + ")";
            CurPriceLabel.Content = list.Last().Price.ToString("F2");
            CurMarkUpLabel.Content = curMarkup.ToString() + "%";
            if(curMarkup > 0)
            {
                CurPriceLabel.Foreground = new SolidColorBrush(Colors.Red);
                CurMarkUpLabel.Foreground = new SolidColorBrush(Colors.Red);
            }
            else if (curMarkup < 0)
            {
                CurPriceLabel.Foreground = new SolidColorBrush(Colors.Green);
                CurMarkUpLabel.Foreground = new SolidColorBrush(Colors.Green);
            }
            else
            {
                CurPriceLabel.Foreground = new SolidColorBrush(Colors.Black);
                CurMarkUpLabel.Foreground = new SolidColorBrush(Colors.Black);
            }
            DrawLines(list);
        }

        private async Task<List<DetailEntity>> GetData()
        {
            List<DetailEntity> prices = new List<DetailEntity>();
            if (IsResponsing)
                return prices;
            int typeId = 0;
            if (GPCode.StartsWith("00"))
            {
                typeId = 0;
            }
            else if (GPCode.StartsWith("60"))
            {
                typeId = 1;
            }
            else
            {
                return prices; 
            }
            string url = $"https://push2.eastmoney.com/api/qt/stock/trends2/get?fields1=f1,f2,f3,f4,f5,f6,f7,f8,f9,f10,f11,f12,f13&fields2=f51,f52,f53,f54,f55,f56,f57,f58&iscr=0&ndays=1&secid={typeId}.{GPCode}";

            try
            {
                IsResponsing = true;


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
                            PrePrice = double.Parse(jobject["data"]["prePrice"].ToString());
                            Name = jobject["data"]["name"].ToString();
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
                MessageBox.Show(ex.Message);
                IsResponsing = false;

            }
            IsResponsing = false;
            return prices;
        }
        private void DrawLines(List<DetailEntity> entities)
        {
            canvas.Children.Clear();

            // 计算折线图的最大值和最小值
            double maxValue = entities.Select(x => x.MarkUp).Max();
            double minValue = entities.Select(x => x.MarkUp).Min();

            if(Math.Abs(maxValue)>= Math.Abs(minValue))
            {
                maxValue = Math.Abs(maxValue);
                minValue = -maxValue;
            }
            else
            {
                maxValue = Math.Abs(minValue);
                minValue = -maxValue;
            }
            // 计算折线图数据点的间距
            double dataPointWidth = AcWidth / 241;

            // 绘制折线
            for (int i = 0; i < entities.Count - 1; i++)
            {
                double x1 = i * dataPointWidth;
                double y1 = AcHeight - ((entities[i].MarkUp - minValue) / (maxValue - minValue)) * AcHeight;

                double x2 = (i + 1) * dataPointWidth;
                double y2 = AcHeight - ((entities[i + 1].MarkUp - minValue) / (maxValue - minValue)) * AcHeight;

                Line line = new Line
                {
                    X1 = x1,
                    Y1 = y1,
                    X2 = x2,
                    Y2 = y2,
                    Stroke = Brushes.Gray,
                    StrokeThickness = 1.5
                };

                canvas.Children.Add(line);
            }
            Line dotedline = new Line
            {
                X1 = 0,
                Y1 = AcHeight / 2,
                X2 = AcWidth,
                Y2 = AcHeight / 2,
                Stroke = Brushes.Gray,
                StrokeThickness = 1.5,
                StrokeDashArray = new DoubleCollection(new List<double> {1,1 })
            };
            canvas.Children.Add(dotedline);

            Label maxMarkLabel = new Label();
            maxMarkLabel.Content = maxValue + "%";
            canvas.Children.Add(maxMarkLabel);
            Canvas.SetLeft(maxMarkLabel, AcWidth - 50);
            Canvas.SetTop(maxMarkLabel, 0);

            Label minMarkLabel = new Label();
            minMarkLabel.Content = minValue + "%";
            canvas.Children.Add(minMarkLabel);
            Canvas.SetLeft(minMarkLabel, AcWidth - 50);
            Canvas.SetTop(minMarkLabel, AcHeight - 25);

            Label maxPriceLabel = new Label();
            maxPriceLabel.Content = (PrePrice * (1 + maxValue / 100.0)).ToString("F2");
            canvas.Children.Add(maxPriceLabel);
            Canvas.SetLeft(maxPriceLabel, 0);
            Canvas.SetTop(maxPriceLabel, 0);

            Label minPriceLabel = new Label();
            minPriceLabel.Content = (PrePrice * (1 + minValue / 100.0)).ToString("F2");
            canvas.Children.Add(minPriceLabel);
            Canvas.SetLeft(minPriceLabel, 0);
            Canvas.SetTop(minPriceLabel, AcHeight - 25);

            Label midPriceLabel = new Label();
            midPriceLabel.Content = (PrePrice).ToString("F2");
            canvas.Children.Add(midPriceLabel);
            Canvas.SetLeft(midPriceLabel, 0);
            Canvas.SetTop(midPriceLabel, AcHeight/2);

            if(m_Line != null)
            {
                canvas.Children.Remove(m_Line);
                canvas.Children.Add(m_Line);
            }
        }

        private void canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            timer.Stop();
            this.Close();
        }

        private Line m_Line;
        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            var pos = e.GetPosition(this);
            // 计算折线图数据点的间距
            double dataPointWidth = AcWidth / 241;
            int indexX = (int)(pos.X / dataPointWidth);

            if(m_Line == null)
            {
                m_Line = new Line
                {
                    X1 = indexX * dataPointWidth - 50,
                    Y1 = 0,
                    X2 = indexX * dataPointWidth - 50,
                    Y2 = AcHeight,
                    Stroke = Brushes.LightBlue,
                    StrokeThickness = 1.5
                };
            }
            else
            {
                m_Line.X1 = indexX * dataPointWidth - 50;
                m_Line.X2 = indexX * dataPointWidth - 50;
            }
            canvas.Children.Remove(m_Line);
            canvas.Children.Add(m_Line);
        }

        private void canvas_MouseLeftButtonUp_1(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(this);
            // 计算折线图数据点的间距
            double dataPointWidth = AcWidth / 241;
            int indexX = (int)(pos.X / dataPointWidth);

            if (m_Line == null)
            {
                m_Line = new Line
                {
                    X1 = indexX * dataPointWidth - 50,
                    Y1 = 0,
                    X2 = indexX * dataPointWidth - 50,
                    Y2 = AcHeight,
                    Stroke = Brushes.LightBlue,
                    StrokeThickness = 1.5
                };
            }
            else
            {
                m_Line.X1 = indexX * dataPointWidth - 50;
                m_Line.X2 = indexX * dataPointWidth - 50;
            }
            canvas.Children.Remove(m_Line);
            canvas.Children.Add(m_Line);
        }
    }

    public class DetailEntity
    {
        public int Index { get; set; }
        public DateTime EntTimeSpan { get; set; }
        public double Price { get; set; }
        public double MarkUp { get; set; }
    }
}
