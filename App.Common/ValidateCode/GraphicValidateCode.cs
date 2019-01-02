using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Common.ValidateCode
{
    public class GraphicValidateCode
    {

        public GraphicValidateCode()
        {

        }

        /// <summary>
        /// 验证码的最大长度
        /// </summary>
        public int MaxLength
        {
            get { return 10; }
        }

        /// <summary>
        /// 验证码的最小长度
        /// </summary>
        public int MinLength
        {
            get { return 1; }
        }

        private float _fontSize = 14;
        private int _imageHeight = 26;

        /// <summary>
        /// 生成验证码
        /// </summary>
        /// <param name="length">验证码的长度</param>
        /// <returns></returns>
        public string GenerateCode(int length)
        {
            var codeStr = new StringBuilder();
            var random = new Random();
            for (var i = 0; i < length; i++)
            {
                string str;
                var isCharFlag = random.Next(1, length);

                if (isCharFlag > 1)
                {
                    str = ((char)random.Next(65, 91)).ToString();
                }
                else
                {
                    str = random.Next(1, 10).ToString();
                }
                codeStr.Append(str);
            }

            return codeStr.ToString();
        }

        #region 生成校验码图片
        public Bitmap CreateImageCode(string code)
        {
            int fSize = 60;
            int padding = 2;
            int fWidth = fSize + padding;
            var colors = new[]
            {
                Color.Black, Color.Red, Color.DarkBlue, Color.Green,
                Color.Brown, Color.DarkCyan, Color.Purple, Color.Orange
            };
            var fonts = new[] { "Arial", "Georgia" };

            int imageWidth = (code.Length * fWidth) + 4 + padding * 2;
            int imageHeight = fSize * 2 + padding;

            Bitmap image = new Bitmap(imageWidth, imageHeight);

            Graphics g = Graphics.FromImage(image);

            g.Clear(Color.White);

            Random rand = new Random();

            //给背景添加随机生成的噪点
            Pen pen = new Pen(Color.LightBlue, 0);
            int c = code.Length * 10;

            for (int i = 0; i < c; i++)
            {
                int x = rand.Next(image.Width);
                int y = rand.Next(image.Height);

                g.DrawRectangle(pen, x, y, 1, 1);
            }

            int left = 0, top = 0, top1 = 1, top2 = 1;

            int n1 = (imageHeight - fSize - padding * 2);
            int n2 = n1 / 4;
            top1 = n2;
            top2 = n2 * 2;

            Font f;
            Brush b;

            int cindex, findex;


            //随机字体和颜色的验证码字符
            for (int i = 0; i < code.Length; i++)
            {
                cindex = rand.Next(colors.Length - 1);
                findex = rand.Next(fonts.Length - 1);

                f = new System.Drawing.Font(fonts[findex], fSize, System.Drawing.FontStyle.Bold);
                b = new System.Drawing.SolidBrush(colors[cindex]);

                if (i % 2 == 1)
                {
                    top = top2;
                }
                else
                {
                    top = top1;
                }
                left = i * fWidth;

                g.DrawString(code.Substring(i, 1), f, b, left, top);
            }

            //画一个边框 边框颜色为Color.Gainsboro
            g.DrawRectangle(new Pen(Color.Gainsboro, 0), 0, 0, image.Width - 1, image.Height - 1);
            g.Dispose();

            //产生波形（Add By 51aspx.com）
            image = TwistImage(image, true, 8, 5);

            return image;

        }

        /// <summary>
        /// 正弦曲线Wave扭曲图片（Edit By 51aspx.com）
        /// </summary>
        /// <param name="srcBmp">图片路径</param>
        /// <param name="bXDir">如果扭曲则选择为True</param>
        /// <param name="dMultValue">波形的幅度倍数，越大扭曲的程度越高，一般为3</param>
        /// <param name="dPhase">波形的起始相位，取值区间[0-2*PI)</param>
        /// <returns></returns>
        private Bitmap TwistImage(Bitmap srcBmp, bool bXDir, double dMultValue, double dPhase)
        {
            const double pi2 = 6.283185307179586476925286766559;
            var destBmp = new Bitmap(srcBmp.Width, srcBmp.Height);

            // 将位图背景填充为白色
            var graph = Graphics.FromImage(destBmp);
            graph.FillRectangle(new SolidBrush(Color.White), 0, 0, destBmp.Width, destBmp.Height);
            graph.Dispose();

            double dBaseAxisLen = bXDir ? (double)destBmp.Height : (double)destBmp.Width;

            for (int i = 0; i < destBmp.Width; i++)
            {
                for (int j = 0; j < destBmp.Height; j++)
                {
                    double dx = 0;
                    dx = bXDir ? (pi2 * (double)j) / dBaseAxisLen : (pi2 * (double)i) / dBaseAxisLen;
                    dx += dPhase;
                    double dy = Math.Sin(dx);

                    // 取得当前点的颜色
                    int nOldX = 0, nOldY = 0;
                    nOldX = bXDir ? i + (int)(dy * dMultValue) : i;
                    nOldY = bXDir ? j : j + (int)(dy * dMultValue);

                    Color color = srcBmp.GetPixel(i, j);
                    if (nOldX >= 0 && nOldX < destBmp.Width
                     && nOldY >= 0 && nOldY < destBmp.Height)
                    {
                        destBmp.SetPixel(nOldX, nOldY, color);
                    }
                }
            }
            return destBmp;
        }

        #endregion

        /// <summary>
        /// 创建验证码的图片
        /// </summary>
        /// <param name="validateCode"></param>
        public byte[] GenerateValidateGraphic(string validateCode)
        {
            using (var image = CreateImageCode(validateCode))
            {
                using (var stream = new MemoryStream())
                {
                    image.Save(stream, ImageFormat.Jpeg);
                    //输出图片流
                    return stream.ToArray();
                }
            }
        }
    }

    public class ValidateCodeValue
    {
        /// <summary>
        /// 校验码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 校验失效次数
        /// </summary>
        public int Times { get; set; } = 5;
    }
}
