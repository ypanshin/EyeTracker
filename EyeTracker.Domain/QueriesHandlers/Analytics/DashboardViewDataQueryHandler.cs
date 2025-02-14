﻿using System;
using System.Collections.Generic;
using System.Linq;
using EyeTracker.Common;
using EyeTracker.Common.Queries.Analytics;
using EyeTracker.Common.QueryResults.Analytics.QueryResults;
using EyeTracker.Domain.Model;
using NHibernate;
using NHibernate.Linq;
using EyeTracker.Common.QueryResults.Analytics;

namespace EyeTracker.Domain.Queries.Analytics
{
    public class DashboardViewDataQueryHandler : FilterBaseQueryHandler, IQueryHandler<DashboardViewDataQuery, DashboardViewDataResult>
    {
        private IRepository repository;
        private ISecurityContext securityContext;

        public DashboardViewDataQueryHandler(IRepository repository, ISecurityContext securityContext)
        {
            this.repository = repository;  
            this.securityContext = securityContext;
        }

        public DashboardViewDataResult Run(ISession session, DashboardViewDataQuery query)
        {
            var res = GetResult<DashboardViewDataResult>(session, securityContext.CurrentUser.Id);
            var dataQuery = session.Query<PageView>()
                                .Where(pv => pv.Application.Id == query.ApplicationId && 
                                                pv.Date >= query.From && 
                                                pv.Date <= query.To);

            if (query.ScreenSize.HasValue)
            {
                dataQuery = dataQuery.Where(pv => pv.ClientHeight == query.ScreenSize.Value.Height && pv.ClientWidth == query.ScreenSize.Value.Width);
            }

            if (!string.IsNullOrEmpty(query.Path))
            {
                dataQuery = dataQuery.Where(pv => pv.Path == query.Path);
            }

            res.Data = dataQuery.GroupBy(g => g.Date.Date)
                                .Select(g => new KeyValuePair<DateTime, int>(g.Key, g.Count()))
                                .ToList()
                                .ToDictionary(v => v.Key, v => v.Value);

            res.ContentOverview = dataQuery.GroupBy(v => new { v.Path, v.Application.Id })
                                            .Select(g => new ContentOverviewResult
                                            {
                                                ApplicationId = g.Key.Id,
                                                Path = g.Key.Path,
                                                Views = g.Count()
                                            })
                                            .ToArray();
            return res;
        }
    }
}
