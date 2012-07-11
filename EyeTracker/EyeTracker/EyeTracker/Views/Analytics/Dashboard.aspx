﻿<%@ Page Title="" Language="C#" 
MasterPageFile="~/Views/Shared/Analytics.Master" 
Inherits="ViewPage<ViewModelWrapper<AfterLoginMasterModel, AnalyticsMasterModel, EyeTracker.Model.Pages.Analytics.DashboardModel>>" %>

<asp:Content ContentPlaceHolderID="PageTitleContent" runat="server">Dashboard</asp:Content>

<asp:Content ContentPlaceHolderID="HeaderContent" runat="server">
<script src="<%: Url.Content("~/Scripts/ThridParty/jquery-ui.min.js")%>" type="text/javascript"></script>
<script src="<%: Url.Content("~/Scripts/ThridParty/DateFormat.js")%>" type="text/javascript"></script>
<script src="<%: Url.Content("~/Scripts/ThridParty/Flot/jquery.flot.min.js")%>" type="text/javascript"></script>
<script src="<%: Url.Content("~/Scripts/ThridParty/Flot/jquery.flot.resize.min.js")%>" type="text/javascript"></script>
<script src="<%: Url.Content("~/Scripts/analytics.dashboard.js")%>" type="text/javascript"></script>
<script src="<%: Url.Content("~/Scripts/analytics.selector.js")%>" type="text/javascript"></script>
<link href="<%: Url.Content("~/Content/after.login.master.css")%>" rel="stylesheet" type="text/css" />
<link href="<%: Url.Content("~/Content/themes/cupertino/jquery-ui.css") %>" rel="stylesheet" type="text/css" />
<link href="<%: Url.Content("~/Content/shared/filter.css")%>" rel="stylesheet" type="text/css" />
<script src="<%: Url.Content("~/Scripts/filter.js")%>" type="text/javascript"></script>
<link href="<%: Url.Content("~/Content/analytics.dashboard.css")%>" rel="stylesheet" type="text/css" />
<script type="text/javascript">
    var usageChartData = <%= Model.View.UsageChartData %>;
</script>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
<% Html.RenderPartial("Filter", Model.View); %>
<table class="dashboard">
<tr>
    <td colspan="2">
        <div class="charts">
            <div class="title"><span>Usage</span></div>
            <div id="usage_charts_place_holder" style="height:200px;width:875px;"></div>
        </div>
    </td>
</tr>
<%--<tr>  
    <td>
        <div class="title"><span>Visitors</span></div>
        <div id="visitors_charts_place_holder" style="height:200px;width:437px;"></div>
    </td>
    <td>
        <div class="title"><span>Map Overlay</span></div>
        <img src="/Content/images/world_map.jpg" class="item" />
    </td>
</tr>--%>
<tr>  
<%--    <td>
        <div class="title"><span>Trafic Sources</span></div>
        <div class="item"></div>
    </td>--%>
    <td colspan="2">
        <div class="title"><span>Content Overview</span></div>
        <section id="content" class="content-overview">

        <table class="fc aP">
            <thead>
                <tr>
                    <th class="bu"></th>
                    <th class="F3 ">Pathes</th>
                    <th class="pr">Visits</th>
                </tr>
            </thead>
            <tbody>
                <%if (Model.View.ContentOverviewData.Any())
                {
                    foreach (var item in Model.View.ContentOverviewData)
                    { %>
                        <tr class="ID-row-0-0-0 ACTION-select TARGET-row-0 <%= item.IndexIsOdd ? "Mbb" : "" %>">
                            <td class="hJ">
                                <%= item.Index %>
                            </td>
                            <td class="HD ">
                                <div class="Xp Ccb">
                                    <a href="<%= item.GetPathUrl(Model.SubMaster.FilterUrlPart) %>"><%= item.Path %></a></div>
                            </td>
                            <td class="pr">
                                <%= item.Views %>
                            </td>
                        </tr>
					<% }
                }%>
            </tbody>
        </table>
        </section>
        <div class="item"></div>
    </td>
</tr>
</table>
</asp:Content>



