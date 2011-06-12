﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Text;
using System.Runtime.Serialization;
using System.Drawing;
using EyeTracker.Models;
using System.Drawing.Imaging;
using System.Runtime.Serialization.Json;
using EyeTracker.Core.Models;
using EyeTracker.Common;
using EyeTracker.Model;
using EyeTracker.Core;
using EyeTracker.Common.Logger;
using System.Reflection;

namespace EyeTracker.Controllers
{
    public class AnalyticsController : Controller
    {
        private static readonly ApplicationLogging log = new ApplicationLogging(MethodBase.GetCurrentMethod().DeclaringType);
        private IAnalyticsService service;

        public AnalyticsController()
            :this(new AnalyticsService())
        {
        }

        public AnalyticsController(IAnalyticsService service)
        {
            this.service = service;
        }

        public ActionResult Index()
        {
            return View();
        }

        public FileResult JavaScript(string clientId)
        {
            var dir = Server.MapPath("/Scripts");
            var path = Path.Combine(dir, "AnalyticsTemplate.js");
            var file = new FileInfo(path);
            string content = string.Empty;
            if (file.Exists)
            {
                using (var stream = file.OpenText())
                {
                    content = stream.ReadToEnd();
                    string url = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Request.ApplicationPath == "/" ? "" : Request.ApplicationPath);
                    content = content.Replace("{VISIT_HANDLER_URL}", url + Url.Action("Visit"));
                    content = content.Replace("{PACKAGE_HANDLER_URL}", url +  Url.Action("Package"));
                    content = content.Replace("{CLIENT_ID}", clientId);
                }
            }
            return base.File(System.Text.Encoding.UTF8.GetBytes(content), "text/javascript");
        }

        public FileResult ClickHeatMapImage(long appId, string pageUri, int clientWidth, int clientHeight, DateTime fromDate, DateTime toDate)
        {
            byte[] imageData = null;
            log.WriteInformation("ClickHeatMapImage: appId:{0}, pageUri:{1}, clientWidth:{2}, clientHeight:{3}, fromDate:{4}, toDate:{5}", appId, pageUri, clientWidth, clientHeight, fromDate, toDate);
            var opResult = service.GetClickHeatMapData(appId, pageUri, clientWidth, clientHeight, fromDate, toDate);
            if (!opResult.WasError)
            {
                Image bgImg = GetBackgroundImage(appId, clientWidth, clientHeight);
                Image image = HeatMapImage.CreateClickHeatMap(opResult.Value, clientWidth, clientHeight, bgImg);
                using (MemoryStream mStream = new MemoryStream())
                {
                    image.Save(mStream, ImageFormat.Png);
                    imageData = mStream.ToArray();
                }

            }
            return base.File(imageData, "Image/png");
        }

        public FileResult ViewHeatMapImage(long appId, string pageUri, int clientWidth, int clientHeight, DateTime fromDate, DateTime toDate)
        {
            byte[] imageData = null;
            log.WriteInformation("ViewHeatMapImage: appId:{0}, pageUri:{1}, clientWidth:{2}, clientHeight:{3}, fromDate:{4}, toDate:{5}",appId, pageUri, clientWidth, clientHeight, fromDate, toDate);
            var opResult = service.GetViewHeatMapData(appId, pageUri, clientWidth, clientHeight, fromDate, toDate);
            if (!opResult.WasError)
            {
                Image bgImg = GetBackgroundImage(appId, clientWidth, clientHeight);
                Image image = HeatMapImage.CreateViewHeatMap(opResult.Value, clientWidth, clientHeight, bgImg);
                using (MemoryStream mStream = new MemoryStream())
                {
                    image.Save(mStream, ImageFormat.Png);
                    imageData = mStream.ToArray();
                }
            }
            return base.File(imageData, "Image/png");
        }


        public JsonResult Visit(string json)
        {
            OperationResult<long> res = null;
            log.WriteInformation("-->Visit:{0}", json);
            try
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(VisitInfoViewModel));

                var ms = new MemoryStream(Encoding.Unicode.GetBytes(json));
                var visitInfo = serializer.ReadObject(ms) as VisitInfoViewModel;
                visitInfo.Ip = Request.UserHostAddress;
                res = service.AddVisitInfo(visitInfo);

                log.WriteInformation("Visit:{0}-->", res);
                Response.AddHeader("Access-Control-Allow-Origin", "*");
            }
            catch (Exception exp)
            {
                log.WriteError(exp, "Visit");
                res = new OperationResult<long>(ErrorNumber.General);
            }
            return base.Json(res);
        }

    
        public JsonResult Debug(string json)
        {
            OperationResult<long> res = null;
            try
            {
                Messenger.SendEmail(null, new List<string>() { "ypanshin@gmail.com" }, "Found Driver License Practical Test Appointment", "On dates:"+json);
                res = new OperationResult<long>(ErrorNumber.None);
            }
            catch (Exception exp)
            {
                log.WriteError(exp, "Visit");
                res = new OperationResult<long>(ErrorNumber.General);
            }
            return base.Json(res, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Package(string json)
        {
            OperationResult res = null;
            try
            {
                log.WriteInformation("-->Package:{0}", json);
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(AnalyticsPackage));

                MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(json));
                AnalyticsPackage packageObject = serializer.ReadObject(ms) as AnalyticsPackage;
                res = new OperationResult();
                foreach (var curClikInfo in packageObject.Clicks)
                {
                    var curRes = service.AddClickInfo(packageObject.VisitId, curClikInfo);
                    if (curRes.WasError) res = curRes;
                }
                foreach (var curViewPartInfo in packageObject.ViewParts)
                {
                    var curRes = service.AddViewPartInfo(packageObject.VisitId, curViewPartInfo);
                    if (curRes.WasError) res = curRes;
                }

                log.WriteInformation("Package:{0}-->", res);
                Response.AddHeader("Access-Control-Allow-Origin", "*");
            }
            catch (Exception exp)
            {
                log.WriteError(exp, "Package");
                res = new OperationResult<long>(ErrorNumber.General);
            }
            return base.Json(res);
        }

        private Image GetBackgroundImage(long appId, int clientWidth, int clientHeight)
        {
            string bgPath = Path.Combine(Server.MapPath("/Users_Resources/Screens"), string.Format("{0}.{1}.{2}.png", appId, clientWidth, clientHeight));
            Image bgImg = null;
            if (System.IO.File.Exists(bgPath)) bgImg = Image.FromFile(bgPath);
            return bgImg;
        }
    }
}
