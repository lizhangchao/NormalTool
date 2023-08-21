using GPSpeedView.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GPSpeedView.GPHelpers
{
    public class AlGpHelper
    {
        public static string CodeFilePath = "DataTxt//ValidCodes.txt";
        /// <summary>
        /// 是否有高概率
        /// 规则： 20个交易日内无涨停(涨幅超过7个点)，无明显振幅（暂定振幅超7个点）,涨幅均值在正负4-5个点区间
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static bool HasHighChance(string code,List<GpDayEntity> ents = null)
        {
            if(ents == null)
            {
                ents = GetHistoryDayInfoByCode(code).Result;
                if (ents.Count == 0) return false;
            }

            if (ents.Count < 30)
                return false;
            // 获取最近20个交易日数据
            var last20Info = ents.GetRange(ents.Count - 30, 30);

            if (last20Info.Find(x => x.MarkUp >= 5) != null
                || last20Info.Find(x => x.MarkUp <= -5) != null)
                return false;
            //if (last20Info.Find(x => x.Amplitude >= 7) != null)
            //    return false;
            double tempTotalMarkUp = 0;
            last20Info.ForEach(x => tempTotalMarkUp += x.MarkUp);
            if(Math.Abs(tempTotalMarkUp / 20.0) >= 5)
                return false;
            return true;
        }
        /// <summary>
        ///  获取上次机会首板日期
        /// </summary>
        /// <returns></returns>
        public static DateTime GetLastHighChanceFirstTop(string code)
        {
            var ents = GetHistoryDayInfoByCode(code).Result;

            int index = ents.FindLastIndex(x => x.MarkUp >= 7);
            while (index >= 0)
            {
                var tempList = ents.GetRange(0, index);
                if (HasHighChance(code, tempList))
                    return ents[index].Time;
                index = tempList.FindLastIndex(x => x.MarkUp >= 7);
            }
            return DateTime.MinValue;
        }
        /// <summary>
        /// 根据代号获取股票历史日线数据
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static async Task<List<GpDayEntity>> GetHistoryDayInfoByCode(string code)
        {
            List<GpDayEntity> ents = new List<GpDayEntity>();

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
                return ents;
            }
            bool hasEx = false;
            do
            {
                try
                {
                    string url = string.Format("http://push2his.eastmoney.com/api/qt/stock/kline/get?fields1=f1,f2,f3,f4,f5,f6,f7,f8,f9,f10,f11,f12,f13&fields2=f51,f52,f53,f54,f55,f56,f57,f58,f59,f60,f61&beg=0&end=20500101&rtntype=6&secid={0}.{1}&klt=101&fqt=1", typeId, code);
                    using (HttpClient client = new HttpClient())
                    {
                        client.Timeout = TimeSpan.FromSeconds(60);
                        HttpResponseMessage response = await client.GetAsync(url).ConfigureAwait(true);
                        if (response.IsSuccessStatusCode)
                        {
                            response.EnsureSuccessStatusCode();
                            string htmlContent = response.Content.ReadAsStringAsync().Result;
                            if (!string.IsNullOrEmpty(htmlContent))
                            {
                                JObject jobject = JObject.Parse(htmlContent);
                                var name = jobject["data"]["name"].ToString();
                                var childrens = jobject["data"]["klines"].Children();

                                foreach (var item in childrens)
                                {
                                    var strs = item.ToString().Split(',');
                                    if (strs.Length != 11)
                                        continue;
                                    GpDayEntity ent = new GpDayEntity();
                                    ent.Code = code;
                                    ent.Name = name;
                                    double temp = 0;
                                    if (!DateTime.TryParse(strs[0].ToString(), out DateTime time))
                                        continue;
                                    ent.Time = time;
                                    ent.PrePrice = double.TryParse(strs[1].ToString(), out temp) ? temp : 0.0;
                                    ent.ClsPrice = double.TryParse(strs[2].ToString(), out temp) ? temp : 0.0;
                                    ent.HighPrice = double.TryParse(strs[3].ToString(), out temp) ? temp : 0.0;
                                    ent.LowPrice = double.TryParse(strs[4].ToString(), out temp) ? temp : 0.0;
                                    ent.Volume = double.TryParse(strs[5].ToString(), out temp) ? temp : 0.0;
                                    ent.TurnVolume = double.TryParse(strs[6].ToString(), out temp) ? temp : 0.0;
                                    ent.Amplitude = double.TryParse(strs[7].ToString(), out temp) ? temp : 0.0;
                                    ent.MarkUp = double.TryParse(strs[8].ToString(), out temp) ? temp : 0.0;
                                    ent.MarkUpPrice = double.TryParse(strs[9].ToString(), out temp) ? temp : 0.0;
                                    ent.ChangeHands = double.TryParse(strs[10].ToString(), out temp) ? temp : 0.0;

                                    ents.Add(ent);
                                }
                            }
                            hasEx = false;
                        }
                    }
                }
                catch (AggregateException)
                {
                    hasEx = true;
                }
            } while (hasEx);

            return ents;
        }
        /// <summary>
        /// 获取本地存储的股票代号
        /// </summary>
        /// <returns></returns>
        public static List<string> GetValidGpCodes()
        {
            List<string> codes = new List<string>();

            string str = File.ReadAllText(CodeFilePath);
            var strs = str.Split(';');
            foreach (var item in strs)
            {
                codes.Add(item);
            }
            return codes;
        }

        public static int HistoryGpNum;
        public static void LoadHistoryGpInfo(BackgroundWorker worker)
        {
            try
            {
                if (!Directory.Exists("HistoryInfo"))
                {
                    Directory.CreateDirectory("HistoryInfo");
                }
                var codes = GetValidGpCodes();
                HistoryGpNum = codes.Count;
                foreach (var item in codes)
                {
                    bool hasEx = false;
                    List<GpDayEntity> list = null;
                    do
                    {
                        try
                        {
                            list = GetHistoryDayInfoByCode(item).Result;
                            hasEx = false;
                        }
                        catch (AggregateException)
                        {
                            hasEx = true;
                        }
                    } while (hasEx);

                    string json = JsonConvert.SerializeObject(list);
                    File.WriteAllText("HistoryInfo//" + item + ".json", json);
                    int i = codes.IndexOf(item);
                    worker.ReportProgress((i + 1) * 100 / codes.Count,i + 1);
                }
            }
            catch (Exception)
            {
                return;
            }

            MessageBox.Show("加载完成");
        }
    }
}
