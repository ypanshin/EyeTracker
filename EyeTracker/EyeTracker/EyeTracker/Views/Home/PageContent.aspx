﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content3" ContentPlaceHolderID="HeaderContent" runat="server">
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server"><%:ViewBag.Url %></asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="Title" runat="server"><%:ViewBag.Url %></asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<div class="panel">
<h2>Content</h2>
<%:ViewBag.Url %>
</div>
</asp:Content>


