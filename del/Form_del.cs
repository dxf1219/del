using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace del
{
    public partial class Form_main : Form
    {
        public Form_main()
        {
            InitializeComponent();


            logpath = Application.StartupPath + "\\logs";

            if (!Directory.Exists(logpath))
            {
                Directory.CreateDirectory(logpath);
            }
           
            WriteLogNew.writeLog("软件启动!", logpath, "info");
            SetText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + "软件启动!\n");


            string deltimeinfo = Properties.Settings.Default.deltime;

            deltimebefore = Properties.Settings.Default.deltimebefore;

            if (Properties.Settings.Default.delbyCreateTime)
            {
                label1.Text = "删除开始时间:" + Properties.Settings.Default.deltime + " 删除:" + Properties.Settings.Default.deltimebefore.ToString() + "小时之前!";
            }
            else
            {
                label1.Text = "删除修改时间:" + Properties.Settings.Default.deltime + " 删除:" + Properties.Settings.Default.deltimebefore.ToString() + "小时之前!";
            }


            strdeltimeList = new List<string>();
            if (deltimebefore >= 24)
            {
                strdeltimeList.Add(deltimeinfo);
            }
            else
            {

                int timedelsecond = convertSecond(deltimeinfo);
                int daysecond = convertSecond("24:00:00");
                while (timedelsecond < daysecond)
                {
                    string deltimetemp = convertTime(timedelsecond);
                    strdeltimeList.Add(deltimetemp);
                    timedelsecond += deltimebefore * 3600;
                }
            }

            ttdel = new Thread(new ThreadStart(scanPathThread));
            ttdel.IsBackground = true;
            ttdel.Start();
        }
        private void delfiles(string spath)
        {

            //判断该文件夹下是否有文件
            if (!Directory.Exists(spath))
            {
                WriteLogNew.writeLog("文件夹:" + spath + "路径不存在!" , logpath, "error");
                return;
            }
            string[] files = Directory.GetFiles(spath, "*.*", SearchOption.TopDirectoryOnly);
            WriteLogNew.writeLog("获取文件夹:" + spath + "下的文件数量:" + files.Length, logpath, "info");
            SetText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "获取文件夹:" + spath + "下的文件数量:" + files.Length + "\n");
            foreach (string file in files)
            {
                if (Properties.Settings.Default.delbyCreateTime) //根据创建时间删除
                {
                    DateTime dtdel = File.GetCreationTime(file);
                    if (dtdel.AddHours(deltimebefore) < DateTime.Now) //创建时间 + 10小时 < 当前时间了
                    {
                        //可以删除文件了
                        try
                        {
                            File.Delete(file);
                            WriteLogNew.writeLog("删除文件成功:" + file, logpath, "info");
                            SetText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "删除文件成功:" + file + "\n");
                            Thread.Sleep(50);
                        }
                        catch (Exception ee)
                        {
                            WriteLogNew.writeLog("删除文件失败:" + file + ee.ToString(), logpath, "error");
                            SetText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "删除文件失败:" + file + "\n");
                        }
                    }
                }
                else//根据修改时间删除
                {
                    DateTime dtdel = File.GetLastWriteTime(file);
                    if (dtdel.AddHours(Properties.Settings.Default.deltimebefore) < DateTime.Now) //创建时间 + 10小时 < 当前时间了
                    {
                        //可以删除文件了
                        try
                        {
                            File.Delete(file);
                            WriteLogNew.writeLog("删除文件成功:" + file, logpath, "info");
                            SetText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "删除文件成功:" + file + "\n");
                            Thread.Sleep(50);
                        }
                        catch (Exception ee)
                        {
                            WriteLogNew.writeLog("删除文件失败:" + file + ee.ToString(), logpath, "error");
                            SetText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "删除文件失败:" + file + "\n");
                        }
                    }
                }
            }//foreach(string file in files)
        }
 
        private void getdirs(string spath)
        {
            try
            {
                if (!Directory.Exists(spath))
                {
                    WriteLogNew.writeLog("文件夹:" + spath + "路径不存在!", logpath, "error");
                    return;
                }
                DirectoryInfo theFolder = new DirectoryInfo(spath);

                foreach (DirectoryInfo nextFolder in theFolder.GetDirectories())
                {
                    WriteLogNew.writeLog("文件夹名称:" + nextFolder.FullName, logpath, "info");
                    SetText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "文件夹名称:" + nextFolder.FullName + "\n");
                    delfiles(nextFolder.FullName);
                    Thread.Sleep(100);
                    if (nextFolder.GetDirectories().Length == 0) //没有子文件夹了
                    {
                        if (nextFolder.GetFiles().Length == 0)
                        {
                            WriteLogNew.writeLog("该文件夹为空:" + nextFolder.FullName, logpath, "info");
                            try
                            {
                                nextFolder.Delete();
                                WriteLogNew.writeLog("删除文件夹成功:" + nextFolder.FullName, logpath, "info");
                                SetText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "删除文件夹成功:" + nextFolder.FullName + "\n");
                            }
                            catch (Exception ee)
                            {
                                WriteLogNew.writeLog("删除文件夹失败:" + nextFolder.FullName + ee.ToString(), logpath, "error");
                                SetText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + "删除文件夹失败:" + nextFolder.FullName + "\n");
                            }
                        }
                    }
                    else
                    {
                        getdirs(nextFolder.FullName);
                    }
                }
            }
            catch (Exception  ee )
            {
                WriteLogNew.writeLog("获取文件夹目录异常:" + ee.ToString(), logpath, "error"); 
            }
        }
        private void scanPathThread()
        {
            while (true)
            {
                try
                { 
                    foreach (string deltimet in strdeltimeList)
                    {
                        DateTime dtdeltime = Convert.ToDateTime(deltimet);
                        DateTime dtnow = DateTime.Now;
                        DateTime dtto = dtdeltime.AddSeconds(31);
                        if ((dtnow > dtdeltime) && (dtnow < dtto))
                        {
                            try
                            {
                                foreach (string spath in Properties.Settings.Default.delpaths)
                                {
                                    WriteLogNew.writeLog("开始处理目录:" + spath, logpath, "info");
                                    delfiles(spath);
                                    Thread.Sleep(100);
                                    //判断目录下是否有文件夹
                                    getdirs(spath);
                                }
                            }
                            catch (Exception ee)
                            {
                                WriteLogNew.writeLog("异常:" + ee.ToString(), logpath, "error");

                            }
                            Thread.Sleep(10000);
                        }// if ((dtnow  > dtdeltime) && (dtnow < dtto))
                    }//   foreach (string deltimet in strdeltimeList)
                }
                catch (Exception ee)
                {
                    WriteLogNew.writeLog("异常:" + ee.ToString(), logpath, "error"); 
                }

                Thread.Sleep(30000);
            }
        }
        private string logpath;
        private Thread ttdel;
        private string deltimeinfo;
        private int deltimebefore = 72;
        private List<string> strdeltimeList;
        #region 消息框代理
        private delegate void SetTextCallback(string text);
        private delegate void SetSelectCallback(object Msge);
        private void SetText(string text1)
        {
            string text = text1;
            try
            {
                if (this.richTextBox1.InvokeRequired)
                {
                    SetTextCallback d = new SetTextCallback(SetText);
                    this.Invoke(d, new object[] { text });
                }
                else
                {
                    if (this.richTextBox1.Lines.Length < 10000)
                    {
                        this.richTextBox1.AppendText(text);
                        of_SetRichCursor(richTextBox1);
                    }
                    else
                    {
                        this.richTextBox1.Clear();
                    }
                }
            }
            catch (Exception)
            {
            }
        }
        private void of_SetRichCursor(object msge)
        {
            try
            {
                RichTextBox richbox = (RichTextBox)msge;
                //设置光标的位置到文本尾
                if (richbox.InvokeRequired)
                {
                    SetSelectCallback d = new SetSelectCallback(of_SetRichCursor);
                    this.Invoke(d, new object[] { msge });
                }
                else
                {
                    richbox.Select(richbox.TextLength, 0);
                    //滚动到控件光标处
                    richbox.ScrollToCaret();
                }
            }
            catch (Exception)
            {
            }
        }
        #endregion


        /// <summary>
        /// 将总帧数转换为标准时间格式
        /// </summary>
        /// <param name="second_num">总帧数</param>
        /// <returns></returns>
        public static string convertTime(int second_num)
        {

            int second = second_num % 60;//获取总秒数
            string second_str = second.ToString();
            if (second >= 10)
            {
                second_str = second_str.ToString();
            }
            else
            {
                second_str = "0" + second_str.ToString();
            }

            int second_remain = second_num / 60;//除去帧数和秒数，得到剩余总秒数
            int minute = second_remain % 60;//获得总分钟数
            string minute_str = minute.ToString();
            if (minute >= 10)
            {
                minute_str = minute_str.ToString();
            }
            else
            {
                minute_str = "0" + minute_str.ToString();
            }

            int minute_remain = second_remain / 60;//除去帧数，秒数和分钟数，得到剩余总分钟数
            int hour = minute_remain % 60;//得到总小时数
            string hour_str = hour.ToString();
            if (hour >= 10)
            {
                hour_str = hour_str.ToString();
            }
            else
            {
                hour_str = "0" + hour_str.ToString();
            }

            string time = hour_str + ":" + minute_str + ":" + second_str ;
            //string time = hour_str + ":" + minute_str + ":" + second_str + ":00";
            return time;

        }
        /// <summary>
        /// 将标准时间格式转化为帧数
        /// </summary>
        /// <param name="time">表示时间的字符串</param>
        /// <returns></returns>
        public static int convertSecond(string time)
        {
            int time_all = 0;
      
            int time_h = Convert.ToInt32(time.Split(':')[0]) * 3600 ;
            int time_m = Convert.ToInt32(time.Split(':')[1]) * 60 ;
            int time_s = Convert.ToInt32(time.Split(':')[2]) ;
            time_all = time_h + time_m + time_s ;
            
            return time_all;
        }

    }
}
