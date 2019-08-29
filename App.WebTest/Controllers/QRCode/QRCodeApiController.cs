using App.Common.QRCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace App.WebTest.Controllers.QRCode
{
    /// <summary>
    /// WebApi
    /// </summary>
    public class QRCodeApiController : ApiController
    {
        /// <summary>
        /// 获取二维码图片（base64格式）
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetQRCode()
        {
            var qrCode = new QRCodeHelper();

            var url = "https://www.cnblogs.com/wenha";
            var image = qrCode.GenerateQRCode(url, "博客园地址");

            var resp = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("data:image/jpeg;base64," + Convert.ToBase64String(image))
            };

            return resp;
        }

        /// <summary>
        /// 获取二维码图片（图片格式）
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetQRCodeImg()
        {

            var qrCode = new QRCodeHelper();

            var url = "https://www.cnblogs.com/wenha";
            var image = qrCode.GenerateQRCode(url, "博客园地址");

            var resp = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(image)
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpg");
            return resp;
        }
    }
}
