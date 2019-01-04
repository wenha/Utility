using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace App.Pay
{
    public class Log
    {
        //在网站根目录下创建日志目录
        public string path;

        public Log(string path)
        {
            this.path = HttpContext.Current.Request.PhysicalApplicationPath + path;
        }
        /**
         * 向日志文件写入调试信息
         * @param className 类名
         * @param content 写入内容
         */
        public void Debug(string className, string content)
        {
            WriteLog("DEBUG", className, content);
        }

        /**
        * 向日志文件写入运行时信息
        * @param className 类名
        * @param content 写入内容
        */
        public void Info(string className, string content)
        {
            WriteLog("INFO", className, content);
        }

        /**
        * 向日志文件写入出错信息
        * @param className 类名
        * @param content 写入内容
        */
        public void Error(string className, string content)
        {
            WriteLog("ERROR", className, content);
        }

        /**
        * 实际的写日志操作
        * @param type 日志记录类型
        * @param className 类名
        * @param content 写入内容
        */
        protected void WriteLog(string type, string className, string content)
        {
            if (!Directory.Exists(path))//如果日志目录不存在就创建
            {
                Directory.CreateDirectory(path);
            }

            string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");//获取当前系统时间
            string filename = path + "/" + DateTime.Now.ToString("yyyy-MM-dd") + ".log";//用日期对日志文件命名

            //创建或打开日志文件，向日志文件末尾追加记录
            StreamWriter mySw = File.AppendText(filename);

            //向日志文件写入内容
            string write_content = time + " " + type + " " + className + ": " + content;
            mySw.WriteLine(write_content);

            //关闭日志文件
            mySw.Close();
        }
    }
}
