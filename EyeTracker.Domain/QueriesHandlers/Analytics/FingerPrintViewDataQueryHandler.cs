﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EyeTracker.Common.Queries.Analytics;
using NHibernate.Linq;
using NHibernate;
using EyeTracker.Domain.Model;
using EyeTracker.Common;
using EyeTracker.Common.QueryResults.Analytics.QueryResults;
using EyeTracker.Common.Commands;
using EyeTracker.Common.QueryResults.Analytics;

namespace EyeTracker.Domain.Queries.Analytics
{
    public class FingerPrintViewDataQueryHandler : FilterBaseQueryHandler, IQueryHandler<FingerPrintViewDataQuery, FingerPrintViewDataResult>
    {
        private IRepository repository;
        private ISecurityContext securityContext;

        public FingerPrintViewDataQueryHandler(IRepository repository, ISecurityContext securityContext)
        {
            this.repository = repository;
            this.securityContext = securityContext;
        }

        public FingerPrintViewDataResult Run(ISession session, FingerPrintViewDataQuery query)
        {
            var data = GetResult<FingerPrintViewDataResult>(session, this.securityContext.CurrentUser.Id, query);
            data.Screens = session.Query<Screen>()
                                        .Where(s => s.Application.Id == data.SelectedApplicationId)
                                        .Select(s => new ScreenResult
                                        {
                                            Id = s.Id,
                                            Path = s.Path,
                                            ApplicationId = s.Application.Id,
                                            Height = s.Height,
                                            Width = s.Width,
                                            FileExtension = s.FileExtension
                                        })
                                        .ToArray();

            data.VisitsData = session.Query<PageView>()
                                    .Where(s => s.Application.Id == data.SelectedApplicationId &&
                                                s.Path.ToLower() == data.SelectedPath.ToLower() &&
                                                s.ScreenWidth == data.SelectedScreenSize.Value.Width &&
                                                s.ScreenHeight == data.SelectedScreenSize.Value.Height &&
                                                s.Date >= query.From && s.Date <= query.To)
                                    .GroupBy(c => c.Date.Date)
                                    .Select(g => new KeyValuePair<DateTime, int>(g.Key, g.Count()))
                                    .ToList().ToDictionary(v => v.Key, v => v.Value);

            data.ClicksData = session.Query<Click>()
                                    .Where(s => s.PageView.Application.Id == data.SelectedApplicationId &&
                                                s.PageView.Path.ToLower() == data.SelectedPath.ToLower() &&
                                                s.PageView.ScreenWidth == data.SelectedScreenSize.Value.Width &&
                                                s.PageView.ScreenHeight == data.SelectedScreenSize.Value.Height &&
                                                s.PageView.Date >= query.From && s.PageView.Date <= query.To)
                                    .GroupBy(c => c.Date.Date)
                                    .Select(g => new KeyValuePair<DateTime, int>(g.Key, g.Count()))
                                    .ToList().ToDictionary(v => v.Key, v => v.Value);

            data.ScrollsData = session.Query<Scroll>()
                                    .Where(s => s.PageView.Application.Id == data.SelectedApplicationId &&
                                                s.PageView.Path.ToLower() == data.SelectedPath.ToLower() &&
                                                s.PageView.ScreenWidth == data.SelectedScreenSize.Value.Width &&
                                                s.PageView.ScreenHeight == data.SelectedScreenSize.Value.Height &&
                                                s.PageView.Date >= query.From && s.PageView.Date <= query.To)
                                    .GroupBy(c => c.PageView.Date)
                                    .Select(g => new KeyValuePair<DateTime, int>(g.Key, g.Count()))
                                    .ToList().ToDictionary(v => v.Key, v => v.Value);
            return data;
        }
    }
}
