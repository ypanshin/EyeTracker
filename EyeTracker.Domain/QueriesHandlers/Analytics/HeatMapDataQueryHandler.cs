﻿using System.Collections.Generic;
using System.Linq;
using EyeTracker.Common.Queries.Content;
using EyeTracker.Domain.Model.Content;
using NHibernate;
using NHibernate.Linq;
using EyeTracker.Common.Queries.Analytics;
using EyeTracker.Common.QueryResults.Analytics;
using EyeTracker.Common;
using EyeTracker.Domain.Model;
using System;
using NHibernate.Transform;

namespace EyeTracker.Domain.Queries
{
    public class HeatMapDataQueryHandler : IQueryHandler<HeatMapDataQuery, IEnumerable<HeatMapDataResult>>
    {
        private ISecurityContext securityContext;

        public HeatMapDataQueryHandler(ISecurityContext securityContext)
        {
            this.securityContext = securityContext;
        }

        public IEnumerable<HeatMapDataResult> Run(ISession session, HeatMapDataQuery query)
        {
            //ViewPart viewPart = null;
            //Application application = null;
            //ViewPartData viewPartData = null;
            //var result = session.QueryOver<PageView>()
            //    .JoinAlias(p => p.ViewParts, () => viewPart)
            //    .JoinAlias(p => p.Application, () => application)
            //    .Where(p => application.Id == query.AplicationId &&
            //                p.Path == query.Path &&
            //                p.ClientWidth == query.ClientWidth &&
            //                p.ClientHeight == query.ClientHeight &&
            //                p.Date >= query.FromDate &&
            //                p.Date <= query.ToDate)
            //    .SelectList(list => list
            //    .Select(p => p.ScreenWidth).WithAlias(() => viewPartData.ScreenWidth)
            //    .Select(p => p.ScreenHeight).WithAlias(() => viewPartData.ScreenHeight)
            //    .Select(p => viewPart.X).WithAlias(() => viewPartData.X)
            //    .Select(p => viewPart.Y).WithAlias(() => viewPartData.Y)
            //    .Select(p => viewPart.StartDate).WithAlias(() => viewPartData.StartDate)
            //    .Select(p => viewPart.FinishDate).WithAlias(() => viewPartData.FinishDate))
            //    .TransformUsing(Transformers.AliasToBean<ViewPartData>())
            //    .List<ViewPartData>()
            //    .GroupBy(d => new { d.X, d.Y, d.ScreenHeight, d.ScreenWidth})
            //    .Select(g => new HeatMapDataResult { ScrollLeft = g.Key.X, ScrollTop = g.Key.Y, ScreenHeight = g.Key.ScreenHeight, ScreenWidth = g.Key.ScreenWidth, TimeSpan = g.Count() })
            //    .ToList();

            //var result = session.Query<PageView>()
            //                .Where(p => p.Application.Id == query.AplicationId &&
            //                        p.Path == query.Path &&
            //                        p.ClientWidth == query.ClientWidth &&
            //                        p.ClientHeight == query.ClientHeight &&
            //                        p.Date >= query.FromDate &&
            //                        p.Date <= query.ToDate &&
            //                        p.ViewParts.Any())
            //                .Select(p => p)
            //                .ToArray()
            //                .SelectMany(p => p.ViewParts.Select(v => new { p.ScreenWidth, p.ScreenHeight, v.X, v.Y, v.StartDate, v.FinishDate }))
            //                .GroupBy(d => new { d.X, d.Y, d.ScreenHeight, d.ScreenWidth})
            //                .Select(g => new HeatMapDataResult { ScrollLeft = g.Key.X, ScrollTop = g.Key.Y, ScreenHeight = g.Key.ScreenHeight, ScreenWidth = g.Key.ScreenWidth, TimeSpan = g.Count() })
            //                .ToArray();

            return null;
        }

        private class ViewPartData
        {
            public DateTime StartDate { get; set; }
            public DateTime FinishDate { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public int ScreenWidth { get; set; }
            public int ScreenHeight { get; set; }
        }
    }
}
