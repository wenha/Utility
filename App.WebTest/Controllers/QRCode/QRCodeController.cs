using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using App.Common.QRCode;

namespace App.WebTest.Controllers.QRCode
{
    /// <summary>
    /// Mvc
    /// </summary>
    public class QRCodeController : Controller
    {
        /// <summary>
        /// 显示二维码
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            var qrCode = new QRCodeHelper();
            var qrImg = qrCode.GenerateQRCode("https://www.cnblogs.com/wenha");
            
            var base64 = "data:image/jpeg;base64," + Convert.ToBase64String(qrImg);

            ViewBag.CodeImg = base64;
            return View();
        }
    }
}