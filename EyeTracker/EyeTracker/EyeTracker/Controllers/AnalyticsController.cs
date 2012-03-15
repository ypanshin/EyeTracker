﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EyeTracker.Common.Logger;
using System.Reflection;
using EyeTracker.Core.Services;
using EyeTracker.Helpers;
using EyeTracker.Model;
using EyeTracker.Domain.Model;
using System.Web.Script.Serialization;
using EyeTracker.Core;
using EyeTracker.Common;
using EyeTracker.Domain;
using System.Collections.ObjectModel;
using EyeTracker.Model.Filter;
using EyeTracker.Model.Master;
using EyeTracker.Model.Pages.Analytics;
using EyeTracker.Controllers.Master;
using EyeTracker.Domain.Common;

namespace EyeTracker.Controllers
{
    [Authorize]
    public class AnalyticsController : Master.AnalyticsMasterController
    {
        private static readonly ApplicationLogging log = new ApplicationLogging(MethodBase.GetCurrentMethod().DeclaringType);
        private IPortfolioService portfolioService;
        private IAccountService accountService;
        private IReportsService reportService;
        private IApplicationService applicationService;
        private IAnalyticsService analyticsService;

        public AnalyticsController()
            : this(new PortfolioService(), 
            new ApplicationService(), 
            new AnalyticsService(), 
            new AccountService(), 
            new ReportsService())
        {
        }

        public AnalyticsController(
            IPortfolioService portfolioService, 
            IApplicationService applicationService, 
            IAnalyticsService analyticsService, 
            IAccountService accountService, 
            IReportsService reportService)
        {
            this.portfolioService = portfolioService;
            this.accountService = accountService;
            this.reportService = reportService;
            this.applicationService = applicationService;
            this.analyticsService = analyticsService;
        }

        public ActionResult Index()
        {
            var res = analyticsService.GetCurrentUserPortfolios();
            if (res.HasError)
            {
                return View("Error");
            }
            return View(new IndexViewModel(res.Value), AnalyticsMasterModel.MenuItem.Portfolios, string.Empty, AfterLoginMasterModel.SelectedMenuItem.Analytics);
        }

        public ActionResult Dashboard(int portfolioId, int? applicationId, DateTime? fromDate, DateTime? toDate, string screenSize, string path, string language, string os, string location)
        {
            if (!fromDate.HasValue)
            {
                fromDate = DateTime.UtcNow.AddDays(-30);
            }

            if(!toDate.HasValue)
            {
                toDate = DateTime.UtcNow;
            }

            int appId = applicationId.HasValue ? applicationId.Value : 0;

            var dataResult = analyticsService.GetDashboardData(portfolioId, applicationId, fromDate.Value, toDate.Value);
            if (dataResult.HasError)
            {
                return View("Error");
            }

            var dashboardData = dataResult.Value;

            var data = new List<object[]>();
            int diffDays = (toDate.Value - fromDate.Value).Days;
            for (int i = 0; i < diffDays; i++)
            {
                int count = 0;
                var curDate = fromDate.Value.AddDays(i);
                if (dashboardData.ViewsData.ContainsKey(curDate))
                {
                    count = dashboardData.ViewsData[curDate];
                }
                data.Add(new object[] { curDate.MilliTimeStamp(), count });
            }

            //Fill chart data
            var usageInitData = new List<object>();
            usageInitData.Add(new
            {
                data = data,
                color = "#461D7C"
            });

            var filterModel = this.GetFilter("Dashboard", fromDate.Value, toDate.Value, dashboardData.Portfolios);
            filterModel.PortfolioId = portfolioId;
            filterModel.AppId = appId;
            return View(new DashboardModel 
                        { 
                            UsageChartData = new JavaScriptSerializer().Serialize(usageInitData), 
                            FilterModel = filterModel
                        },
                        AnalyticsMasterModel.MenuItem.Dashboard,
                        string.Join("/", new string[] { portfolioId.ToString(), filterModel.AppId.ToString(), fromDate.Value.ToString("dd-MMM-yyyy"), toDate.Value.ToString("dd-MMM-yyyy") }),
                        AfterLoginMasterModel.SelectedMenuItem.Analytics);
        }

        private FilterModel GetFilter(string action, DateTime fromDate, DateTime toDate, IEnumerable<PortfolioDetails> portfolios)
        {
            var js = new JavaScriptSerializer();
            return new FilterModel
            {
                PortfoliosData = string.Format("{{{0}}}", string.Join(",", portfolios.Select(p => string.Format("{0}:{1}", p.Id, js.Serialize(p.Applications.Select(a => new { id = a.Id, desc = a.Description })))))),
                Portfolios = portfolios.Select(p => new SelectListItem() { Text = p.Description, Value = p.Id.ToString() }),
                FormAction = action,
                Date = new DateModel
                {
                    DateFrom = fromDate,
                    DateTo = toDate,
                },
                ShowDateSelector = true,
                Applications = new SelectorModel
                {
                    Title = "Add Application",
                    Items = Enumerable.Range(8, 30).Select((num, i) => new SelectorItem { Id = num, Text = "App " + num, Index = i }),
                    SelectedItems = new List<SelectorItem>()
                },
                ScreenSizes = new SelectorModel
                {
                    Title = "Screen Sizes",
                    Items = Enumerable.Range(8, 30).Select((num, i) => new SelectorItem { Id = num, Text = "App " + num, Index = i }),
                    SelectedItems = new List<SelectorItem>()
                }
            };
        }

        public ActionResult Usage(int portfolioId, int? applicationId, DateTime? fromDate, DateTime? toDate)
        {
            if (!fromDate.HasValue)
            {
                fromDate = DateTime.UtcNow.AddDays(-30);
            }

            if (!toDate.HasValue)
            {
                toDate = DateTime.UtcNow;
            }

            int appId = applicationId.HasValue ? applicationId.Value : 0;

            var reportRes = reportService.GetVisitsData(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow, portfolioId, null, DataGrouping.Day);
            if (reportRes.HasError)
            {
                return View("Error");
            }
            var usageData = reportRes.Value;

            var data = new List<object[]>();
            int diffDays = (toDate.Value - fromDate.Value).Days;
            for (int i = 0; i < diffDays; i++)
            {
                int count = 0;
                var curDate = fromDate.Value.AddDays(i);
                if (usageData.ContainsKey(curDate))
                {
                    count = usageData[curDate];
                }
                data.Add(new object[] { curDate.MilliTimeStamp(), count });
            }

            //Fill chart data
            var usageInitData = new List<object>();
            usageInitData.Add(new
            {
                data = data,
                color = "#461D7C"
            });

            var filterModel = this.GetFilter("Dashboard", fromDate.Value, toDate.Value, null/*usageData.Portfolios*/);
            filterModel.PortfolioId = portfolioId;
            filterModel.AppId = appId;
            return View(new DashboardModel
                        {
                            UsageChartData = new JavaScriptSerializer().Serialize(usageInitData),
                            FilterModel = filterModel
                        },
                        AnalyticsMasterModel.MenuItem.Dashboard,
                        string.Join("/", new string[] { portfolioId.ToString(), filterModel.AppId.ToString(), fromDate.Value.ToString("dd-MMM-yyyy"), toDate.Value.ToString("dd-MMM-yyyy") }),
                        AfterLoginMasterModel.SelectedMenuItem.Analytics);
        }

        public ActionResult FingerPrint(int portfolioId, int? applicationId, DateTime? fromDate, DateTime? toDate)
        {
            if (!fromDate.HasValue)
            {
                fromDate = DateTime.UtcNow.AddDays(-30);
            }

            if (!toDate.HasValue)
            {
                toDate = DateTime.UtcNow;
            }
 
            int appId = applicationId.HasValue ? applicationId.Value : 0;

            /*
            var dataResult = analyticsService.GetClickHeatMapData(portfolioId, applicationId, fromDate.Value, toDate.Value);
            if (dataResult.HasError)
            {
                return View("Error");
            }
            */
            return View();
        }

        public ActionResult EyeTracker(int portfolioId, int? applicationId, DateTime? fromDate, DateTime? toDate)
        {
            if (!fromDate.HasValue)
            {
                fromDate = DateTime.UtcNow.AddDays(-30);
            }

            if (!toDate.HasValue)
            {
                toDate = DateTime.UtcNow;
            }

            int appId = applicationId.HasValue ? applicationId.Value : 0;
            /*
            var dataResult = analyticsService.GetEyeTrackerData(portfolioId, applicationId, fromDate.Value, toDate.Value);
            if (dataResult.HasError)
            {
                return View("Error");
            }
            */
            return View();
        }
    }
}
