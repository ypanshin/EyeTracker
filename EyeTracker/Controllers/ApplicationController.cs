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
using EyeTracker.Common;
using EyeTracker.Model;
using EyeTracker.Core;
using EyeTracker.Common.Logger;
using System.Reflection;
using EyeTracker.Core.Services;
using EyeTracker.DAL.Domain;
using EyeTracker.Helpers;
using EyeTracker.Domain.Model;
using System.Web.Script.Serialization;
using EyeTracker.Domain.Model.Events;
using EyeTracker.Domain;

namespace EyeTracker.Controllers
{
    [Authorize]
    public class ApplicationController : Controller
    {
        private static readonly ApplicationLogging log = new ApplicationLogging(MethodBase.GetCurrentMethod().DeclaringType);
        
        private IApplicationService service;
        private IPortfolioService portfolioService;
        private IAnalyticsService analyticsService;
        private IReportsService reportService;

        public ApplicationController()
            : this(new ApplicationService(), 
            new PortfolioService(), 
            new AnalyticsService(), 
            new ReportsService())
        {
        }

        public ApplicationController(IApplicationService service, 
            IPortfolioService portfolioService, 
            IAnalyticsService analyticsService, 
            IReportsService reportService)
        {
            this.service = service;
            this.portfolioService = portfolioService;
            this.analyticsService = analyticsService;
            this.reportService = reportService;
        }

        public ActionResult Index(int portfolioId)
        {
            var appRes = service.GetAll(portfolioId);
            if (appRes.HasError)
            {
                return View("Error");
            }

            ViewBag.PortfolioId = portfolioId;
            var columnHeaders = new List<HTMLTable.Cell>() {
                    new HTMLTable.Cell() { Value = "Description" }, 
                    new HTMLTable.Cell() { Value = "Type" }, 
                    new HTMLTable.Cell() { Value = "% Change" },
                    new HTMLTable.Cell() { Value = "" } 
                };
            var data = new List<List<HTMLTable.Cell>>();

            if (appRes.Value.Count > 0)
            {
                //Create table
                foreach (var curApp in appRes.Value)
                {
                    var cells = new List<HTMLTable.Cell>();
                    cells.Add(new HTMLTable.Cell() { Value = string.Format("<a href=\"/Application/Dashboard/{0}/{1}\">{2}</a>", portfolioId, curApp.Id, curApp.Description) });
                    cells.Add(new HTMLTable.Cell() { Value = curApp.Type.ToString() });
                    cells.Add(new HTMLTable.Cell() { Value = "0.00%" });
                    cells.Add(new HTMLTable.Cell() { Value = string.Format("<a href=\"/Application/Edit/{0}/{1}\">edit</a>&nbsp;<a href=\"/Application/Remove/{0}/{1}\">remove</a>", portfolioId, curApp.Id) });
                    data.Add(cells);
                }
            }
            else
            {
                data.Add(new List<HTMLTable.Cell>() { new HTMLTable.Cell() { ColSpan = 8, StyleClass = "no-data", Value = "No Applications" } });
            }

            ViewData["caption"] = new HTMLTable.Cell() { Value = "Accounts" };
            ViewData["columnHeaders"] = columnHeaders;
            ViewData["data"] = data;
            return View();
        }

        public ActionResult New(int portfolioId)
        {
            ViewBag.Screens = new List<Screen>();
            ViewBag.PortfolioId = portfolioId;
            ViewData["TypesList"] = Enum.GetValues(typeof(ApplicationType)).Cast<ApplicationType>().Select(i => new SelectListItem() { Text = i.ToString(), Value = ((int)i).ToString() });
            ViewBag.PackageLink = "http://mobillify.com";
            ViewBag.PropertyId = "**-******-***";
            ViewBag.CodeSample = "<script type=\"text/javascript\">\nvar _gaq = _gaq || [];_\ngaq.push(['_setAccount', '**-******-***']);";
            return View("NewEdit",new ApplicationModel());
        }

        [HttpPost]
        public JsonResult New(ApplicationModel model)
        {
            object res = null;
            if (ModelState.IsValid)
            {
                var portfolioRes = portfolioService.Get(model.PortfolioId);
                if (portfolioRes.HasError)
                {
                    res = new { HasError = true };
                }
                else
                {
                    var app = new Application(portfolioRes.Value, model.Description, model.Type);
                    var appRes = service.Add(app);
                    if (appRes.HasError)
                    {
                        res = new { HasError = true };
                    }
                    else
                    {
                        string key = GetAppKey(app.Type);
                        res = new { 
                            HasError = false, 
                            code = string.Format("{0}-{1:000000}-{2:0000}", key, model.PortfolioId, appRes.Value),
                            appId = appRes.Value
                        };
                    }
                }
            }
            else
            {
                res = new { };
            }
            return Json(res);
        }

        private static string GetAppKey(ApplicationType type)
        {
            string key = "";
            switch (type)
            {
                case ApplicationType.Android:
                    key = "MA";
                    break;
                case ApplicationType.Web:
                    key = "WP";
                    break;
                case ApplicationType.iPhone:
                    key = "MI";
                    break;
                case ApplicationType.WebMobile:
                    key = "WM";
                    break;
                case ApplicationType.WindowsMobile:
                    key = "MW";
                    break;
            }
            return key;
        }

        public ActionResult Edit(int portfolioId, int appId)
        {
            var appRes = service.Get(appId);
            if (appRes.HasError)
            {
                return View("Error");
            }
            else
            {
                var app = appRes.Value;
                var model = new ApplicationModel { 
                    Id = app.Id,
                    Description = app.Description,
                    PortfolioId = portfolioId,
                    Type = app.Type
                };
                ViewBag.Screens = appRes.Value.Screens;
                ViewBag.PortfolioId = portfolioId;
                ViewData["TypesList"] = Enum.GetValues(typeof(ApplicationType)).Cast<ApplicationType>().Select(i => new SelectListItem() { Text = i.ToString(), Value = ((int)i).ToString() });
                ViewBag.PackageLink = "http://mobillify.com";
                ViewBag.PropertyId = string.Format("{0}-{1:000000}-{2:0000}",GetAppKey(app.Type), portfolioId, appId);
                ViewBag.CodeSample = "<script type=\"text/javascript\">\nvar _gaq = _gaq || [];_\ngaq.push(['_setAccount', '" + ViewBag.PropertyId + "']);";
                return View("NewEdit", model);
            }
        }

        [HttpPost]
        public ActionResult Edit(ApplicationModel model)
        {
            if (ModelState.IsValid)
            {
                var portfolioRes = portfolioService.Get(model.PortfolioId);
                if (portfolioRes.HasError)
                {
                    return View("Error");
                }
                else
                {
                    var appRes = service.Update(model.Id, model.Description);
                    if (appRes.HasError)
                    {
                        return View("Error");
                    }
                    else
                    {
                        return RedirectToRoute("ApplicationDef");
                    }
                }
            }
            else
            {
                return View("NewEdit", model);
            }
        }

        [HttpPost]
        public ActionResult AddScreen(ScreenDetailsModel screenDetails)
        {
            object res = null;
            if (ModelState.IsValid)
            {
                var file = Request.Files["screen_img"];
                var screen = new Screen { 
                    ApplicationId = screenDetails.AppId,
                    Width = screenDetails.Width,
                    Height = screenDetails.Height,
                    FileExtension = Path.GetExtension(file.FileName)
                };
                var addRes = service.AddScreen(screen);
                if (!addRes.HasError)
                {
                    string tmpFileFullName = null;
                    if (file.ContentLength > 0)
                    {
                        tmpFileFullName = Path.Combine(HttpContext.Server.MapPath("/Users_Resources/Screens/"), string.Format("{0}_{1}X{2}{3}", screenDetails.AppId, screen.Width, screen.Height, screen.FileExtension));
                        file.SaveAs(tmpFileFullName);
                    }
                    res = new { HasError = false, ScreenId = addRes.Value };
                }
                else
                {
                    res = new { HasError = true, ScreenId = -1 };
                }
            }
            var actionResult = base.Json(res);
            actionResult.ContentType = "text/html";
            return actionResult;
        }

        public ActionResult Remove(int portfolioId, int appId)
        {
            var res = service.Remove(appId);
            if (res.HasError)
            {
                return View("Error");
            }
            else
            {
                return RedirectToAction("");
            }
        }
        public FileResult Screen(int appId, int width, int height, string file)
        {
            var res = service.GetScreen(appId, width, height);
            if (!res.HasError && res.Value != null)
            {
                string tmpFileFullName = Path.Combine(HttpContext.Server.MapPath("/Users_Resources/Screens/"), string.Format("{0}_{1}X{2}{3}", res.Value.ApplicationId, res.Value.Width, res.Value.Height, res.Value.FileExtension));
                return base.File(tmpFileFullName, MIMEAssistant.GetMIMEType(tmpFileFullName));
            }
            else
            {
                throw new HttpException(404, "Not found");
            }
        }

        public ActionResult Dashboard(int portfolioId, int appId)
        {
            var points = new Dictionary<DateTime, int> { { DateTime.Now.AddDays(-5), 40 }, { DateTime.Now.AddDays(-4), 30 }, { DateTime.Now.AddDays(-3), 10 }, { DateTime.Now.AddDays(-2), 50 }, { DateTime.Now.AddDays(-1), 40 } };
            //Fill chart data
            var chartInitData = new List<object>();
            chartInitData.Add(new
            {
                data = points.OrderBy(curItem => curItem.Key).Select(curItem => new object[] { curItem.Key.MilliTimeStamp(), curItem.Value }),
                color = "#461D7C"
            });
            ViewBag.ChartInitData = new JavaScriptSerializer().Serialize(chartInitData);
            ViewBag.PortfolioId = portfolioId;
            ViewBag.ApplicationId = appId;
            return View();
        }

        public ActionResult Usage(int portfolioId, int appId)
        {
            var reportRes = reportService.GetVisitsData(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow, null, appId, DataGrouping.Day);
            if (reportRes.HasError)
            {
                return View("Error");
            }
            //Fill chart data
            var chartInitData = new List<object>();
            chartInitData.Add(new
            {
                data = reportRes.Value.OrderBy(curItem => curItem.Key).Select(curItem => new object[] { curItem.Key.MilliTimeStamp(), curItem.Value }),
                color = "#461D7C"
            });
            ViewBag.ChartInitData = new JavaScriptSerializer().Serialize(chartInitData);
            ViewBag.PortfolioId = portfolioId;
            ViewBag.ApplicationId = appId;
            return View();
        }

        public ActionResult EyeTracker(int portfolioId, int appId)
        {
            ViewBag.PortfolioId = portfolioId;
            ViewBag.ApplicationId = appId;

            DateTime fromDate = DateTime.UtcNow.AddDays(-30);
            DateTime toDate = DateTime.UtcNow;
            var res = service.GetEyeTrackerData(appId, fromDate, toDate);
            if (res.HasError)
            {
                return View("Error");
            }
            else
            {
                if (res.Value.PageUris.Count() == 0 || res.Value.ScreenSizes.Count() == 0)
                {
                    ViewBag.NoData = true;
                }
                else
                {
                    ViewBag.NoData = false;
                    ViewData["ScreenSizes"] = new List<SelectListItem>(res.Value.ScreenSizes.Select(s => new SelectListItem { Text = string.Format("{0} X {1}", s.Width, s.Height), Value = string.Format("{0}X{1}", s.Width, s.Height) }));
                    ViewData["PageUris"] = new List<SelectListItem>(res.Value.PageUris.Select(s => new SelectListItem { Text = s, Value = s }));
                    ViewBag.EyeTrackerImageUrl = string.Format("/Application/ViewHeatMapImage/{0}/?appId={0}&pageUri={1}&clientWidth={2}&clientHeight={3}&fromDate={4}&toDate={5}&preview=true", appId, HttpUtility.UrlEncode(res.Value.PageUris.First()), res.Value.ScreenSizes.First().Width, res.Value.ScreenSizes.First().Height, HttpUtility.UrlEncode(fromDate.ToString("HH:mm dd-MMM-yyyy")), HttpUtility.UrlEncode(toDate.ToString("HH:mm dd-MMM-yyyy")));
                }
                return View("Image");
            }
        }

        public ActionResult Fingerprint(int portfolioId, int appId)
        {
            ViewBag.PortfolioId = portfolioId;
            ViewBag.ApplicationId = appId;

            DateTime fromDate = DateTime.UtcNow.AddDays(-30);
            DateTime toDate = DateTime.UtcNow;
            var res = service.GetEyeTrackerData(appId, fromDate, toDate);
            if (res.HasError)
            {
                return View("Error");
            }
            else
            {
                if (res.Value.PageUris.Count() == 0 || res.Value.ScreenSizes.Count() == 0)
                {
                    ViewBag.NoData = true;
                }
                else
                {
                    ViewBag.NoData = false;
                    ViewData["ScreenSizes"] = new List<SelectListItem>(res.Value.ScreenSizes.Select(s => new SelectListItem { Text = string.Format("{0} X {1}", s.Width, s.Height), Value = string.Format("{0}X{1}", s.Width, s.Height) }));
                    ViewData["PageUris"] = new List<SelectListItem>(res.Value.PageUris.Select(s => new SelectListItem { Text = s, Value = s }));
                    ViewBag.EyeTrackerImageUrl = string.Format("/Application/ClickHeatMapImage/{0}/?appId={0}&pageUri={1}&clientWidth={2}&clientHeight={3}&fromDate={4}&toDate={5}&preview=true", appId, HttpUtility.UrlEncode(res.Value.PageUris.First()), res.Value.ScreenSizes.First().Width, res.Value.ScreenSizes.First().Height, HttpUtility.UrlEncode(fromDate.ToString("HH:mm dd-MMM-yyyy")), HttpUtility.UrlEncode(toDate.ToString("HH:mm dd-MMM-yyyy")));
                }
                return View("Image");
            }
        }

        public FileResult ClickHeatMapImage(long appId, string pageUri, int clientWidth, int clientHeight, DateTime fromDate, DateTime toDate, bool preview)
        {
            byte[] imageData = null;
            var opResult = analyticsService.GetClickHeatMapData(appId, pageUri, clientWidth, clientHeight, fromDate, toDate);
            if (!opResult.HasError)
            {
                Image bgImg = GetBackgroundImage(appId, clientWidth, clientHeight);
                Image image = HeatMapImage_.CreateClickHeatMap(opResult.Value, clientWidth, clientHeight, bgImg);
                using (MemoryStream mStream = new MemoryStream())
                {
                    image.Save(mStream, ImageFormat.Png);
                    imageData = mStream.ToArray();
                }
                image.Dispose();

            }
            if (imageData == null)
            {
                throw new HttpException(404, "Not found");
            }
            else
            {
                return base.File(imageData, "Image/png");
            }
        }

        public FileResult ViewHeatMapImage(long appId, string pageUri, int clientWidth, int clientHeight, DateTime fromDate, DateTime toDate, bool preview)
        {
            byte[] imageData = null;
            var opResult = analyticsService.GetViewHeatMapData(appId, pageUri, clientWidth, clientHeight, fromDate, toDate);
            if (!opResult.HasError)
            {
                Image bgImg = GetBackgroundImage(appId, clientWidth, clientHeight);
                Image image = HeatMapImage_.CreateViewHeatMap(opResult.Value, clientWidth, clientHeight, bgImg);
                using (MemoryStream mStream = new MemoryStream())
                {
                    image.Save(mStream, ImageFormat.Png);
                    imageData = mStream.ToArray();
                }
            }
            if (imageData == null)
            {
                throw new HttpException(404, "Not found");
            }
            else
            {
                return base.File(imageData, "Image/png");
            }
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
