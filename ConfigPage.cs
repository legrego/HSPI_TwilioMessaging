﻿using HomeSeerAPI;
using NullGuard;
using Scheduler;
using System;
using System.Collections.Specialized;
using System.Text;
using System.Web;

namespace Hspi
{

    /// <summary>
    /// Helper class to generate configuration page for plugin
    /// </summary>
    /// <seealso cref="Scheduler.PageBuilderAndMenu.clsPageBuilder" />
    [NullGuard(ValidationFlags.Arguments | ValidationFlags.NonPublic)]
    internal class ConfigPage : PageBuilderAndMenu.clsPageBuilder
    {
        protected const string IdPrefix = "id_";

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigPage" /> class.
        /// </summary>
        /// <param name="HS">The hs.</param>
        /// <param name="pluginConfig">The plugin configuration.</param>
        public ConfigPage(IHSApplication HS, PluginConfig pluginConfig) : base(pageName)
        {
            this.HS = HS;
            this.pluginConfig = pluginConfig;
        }

        /// <summary>
        /// Gets the name of the web page.
        /// </summary>
        public static string Name => pageName;

        /// <summary>
        /// Get the web page string for the configuration page.
        /// </summary>
        /// <returns>
        /// System.String.
        /// </returns>
        public string GetWebPage()
        {
            try
            {
                reset();

                AddHeader(HS.GetPageHeader(Name, "Configuration", string.Empty, string.Empty, false, false));

                System.Text.StringBuilder stb = new System.Text.StringBuilder();
                stb.Append(PageBuilderAndMenu.clsPageBuilder.DivStart("pluginpage", ""));
                stb.Append(BuildWebPageBody());
                stb.Append(PageBuilderAndMenu.clsPageBuilder.DivEnd());
                AddBody(stb.ToString());

                AddFooter(HS.GetPageFooter());
                suppressDefaultFooter = true;

                return BuildPage();
            }
            catch (Exception)
            {
                return "error";
            }
        }

        /// <summary>
        /// The user has selected a control on the configuration web page.
        /// The post data is provided to determine the control that initiated the post and the state of the other controls.
        /// </summary>
        /// <param name="data">The post data.</param>
        /// <param name="user">The name of logged in user.</param>
        /// <param name="userRights">The rights of the logged in user.</param>
        /// <returns>Any serialized data that needs to be passed back to the web page, generated by the clsPageBuilder class.</returns>
        public string PostBackProc(string data, [AllowNull]string user, int userRights)
        {
            NameValueCollection parts = HttpUtility.ParseQueryString(data);

            string form = parts["id"];

            if (form == NameToIdWithPrefix(SaveButtonName))
            {
                StringBuilder results = new StringBuilder();

                // Validate
                if (string.IsNullOrWhiteSpace(parts[AuthTokenId]))
                {
                    results.AppendLine("Auth Token is not Valid.<br>");
                }

                if (string.IsNullOrWhiteSpace(parts[AccountSIDId]))
                {
                    results.AppendLine("Account SID is not Valid.<br>");
                }

                if (string.IsNullOrWhiteSpace(parts[FromNumberId]))
                {
                    results.AppendLine("From Number is not Valid.<br>");
                }

                if (results.Length > 0)
                {
                    this.divToUpdate.Add(ErrorDivId, results.ToString());
                }
                else
                {
                    this.divToUpdate.Add(ErrorDivId, string.Empty);
                    this.pluginConfig.AccountSID = parts[AccountSIDId];
                    this.pluginConfig.AuthToken = parts[AuthTokenId];
                    this.pluginConfig.FromNumber = parts[FromNumberId];
                    this.pluginConfig.DebugLogging = parts[DebugLoggingId] == "checked";
                    this.pluginConfig.FireConfigChanged();
                }
            }

            return base.postBackProc(Name, data, user, userRights);
        }

        /// <summary>
        /// Builds the web page body for the configuration page.
        /// The page has separate forms so that only the data in the appropriate form is returned when a button is pressed.
        /// </summary>
        private string BuildWebPageBody()
        {
            int i = 0;
            StringBuilder stb = new StringBuilder();

            var tabs = new clsJQuery.jqTabs("tab1id", PageName);
            var tab1 = new clsJQuery.Tab();
            tab1.tabTitle = "Twilio Settings";
            tab1.tabDIVID = $"tabs{i++}";
            tab1.tabContent = BuildSettingTab();
            tabs.tabs.Add(tab1);

            tabs.postOnTabClick = false;
            stb.Append(tabs.Build());

            return stb.ToString();
        }

        private string BuildSettingTab()
        {
            StringBuilder stb = new StringBuilder();
            stb.Append(PageBuilderAndMenu.clsPageBuilder.FormStart("ftmSettings", "IdSettings", "Post"));

            stb.Append(@"<br>");
            stb.Append(@"<div>");
            stb.Append(@"<table class='full_width_table'");
            stb.Append("<tr height='5'><td style='width:25%'></td><td style='width:75%'></td></tr>");
            stb.Append($"<tr><td class='tablecell'>Account SID:</td><td class='tablecell' style='width: 100px'>{HtmlTextBox(AccountSIDId, pluginConfig.AccountSID, 40)}</td></tr>");
            stb.Append($"<tr><td class='tablecell'>Auth Token:</td><td class='tablecell' style='width: 100px'>{HtmlTextBox(AuthTokenId, pluginConfig.AuthToken, 40)}</td></tr>");
            stb.Append($"<tr><td class='tablecell'>From Number:</td><td class='tablecell' style='width: 100px'>{HtmlTextBox(FromNumberId, pluginConfig.FromNumber, 40)}</td></tr>");
            stb.Append($"<tr><td class='tablecell'>Debug Logging Enabled:</td><td colspan=2 class='tablecell'>{FormCheckBox(DebugLoggingId, string.Empty, this.pluginConfig.DebugLogging)}</ td ></ tr > ");
            stb.Append($"<tr><td colspan=2><div id='{ErrorDivId}' style='color:Red'></div></td><td></td></tr>");
            stb.Append($"<tr><td colspan=2>{FormButton("Save", SaveButtonName, "Save Settings")}</td><td></td></tr>");
            stb.Append("<tr height='5'><td colspan=2></td></tr>");
            stb.Append($"<tr><td colspan=2></td></tr>");
            stb.Append(@"<tr><td colspan=2><div>Register an account at <a href='http://twilio.com' title='Twilio' target='_blank'>Twilio</a> to get started</div></td></tr>");
            stb.Append(@"<tr height='5'><td colspan=2></td></tr>");
            stb.Append(@" </table>");
            stb.Append(@"</div>");
            stb.Append(PageBuilderAndMenu.clsPageBuilder.FormEnd());

            return stb.ToString();
        }

        private static string NameToId(string name)
        {
            return name.Replace(' ', '_');
        }

        private static string NameToIdWithPrefix(string name)
        {
            return $"{ IdPrefix}{NameToId(name)}";
        }

        protected static string HtmlTextBox(string name, string defaultText, int size = 25)
        {
            return $"<input type=\'text\' id=\'{NameToIdWithPrefix(name)}\' size=\'{size}\' name=\'{name}\' value=\'{defaultText}\'>";
        }

        protected string FormCheckBox(string name, string label, bool @checked)
        {
            var checkbox = new clsJQuery.jqCheckBox(name, label, PageName, true, true)
            {
                id = NameToIdWithPrefix(name),
                @checked = @checked,
            };
            return checkbox.Build();
        }

        protected string FormButton(string name, string label, string toolTip)
        {
            var button = new clsJQuery.jqButton(name, label, PageName, true)
            {
                id = NameToIdWithPrefix(name),
                toolTip = toolTip,
            };
            button.toolTip = toolTip;
            button.enabled = true;

            return button.Build();
        }

        private const string SaveButtonName = "Save";
        private const string CallsPerDayId = "CallsPerDayId";
        private const string DebugLoggingId = "DebugLoggingId";
        private const string UnitId = "UnitId";
        private const string AccountSIDId = "AccountSIDId";
        private const string AuthTokenId = "AuthTokenId";
        private const string FromNumberId = "FromNumberId";
        private const string ErrorDivId = "message_id";
        private const string ImageDivId = "image_id";
        private const string RefreshIntervalId = "RefreshIntervalId";
        private static readonly string pageName = $"{TwilioMessagingData.PlugInName} Configuration".Replace(' ', '_');
        private readonly IHSApplication HS;
        private readonly PluginConfig pluginConfig;
    }
}