using HomeSeerAPI;
using NullGuard;
using Scheduler;
using System;
using System.Collections.Specialized;
using System.Threading;
using Hspi.Pages;
using Hspi.Utils;
using Hspi.Exceptions;
using Twilio.Rest.Api.V2010.Account;

namespace Hspi
{
    /// <summary>
    /// Plugin partial class for managing Twilio HomeSeer events (triggers/actions)
    /// </summary>
    /// <seealso cref="Hspi.HspiBase" />
    internal partial class TwilioMessagingPlugin : HspiBase
    {

        #region "Trigger Override"
        public override bool HasTriggers => true;
        public override int TriggerCount => 1;
        protected override int GetTriggerCount()
        {
            return 1;
        }

        public override bool get_HasConditions(int triggerNumber) => false;

        public override string get_TriggerName(int triggerNumber)
        {
            using (var page = new TriggerPage(HS, this.pluginConfig))
            {
                return page.Name();
            }
        }

        public override string TriggerBuildUI([AllowNull]string uniqueControlId, IPlugInAPI.strTrigActInfo triggerInfo)
        {
            using (var page = GetTriggerPage(triggerInfo))
            {
                return page.BuildEditUI(uniqueControlId, triggerInfo);
            }
        }

        public override IPlugInAPI.strMultiReturn TriggerProcessPostUI([AllowNull] NameValueCollection postData, IPlugInAPI.strTrigActInfo actionInfo)
        {
            using (var page = GetTriggerPage(actionInfo))
            {
                return page.ProcessPostUI(postData, actionInfo);
            }
        }

        public override bool get_TriggerConfigured(IPlugInAPI.strTrigActInfo actionInfo)
        {
            using (var page = GetTriggerPage(actionInfo))
            {
                return page.IsConfigured(actionInfo);
            }
        }

        public override string TriggerFormatUI(IPlugInAPI.strTrigActInfo actionInfo)
        {
            using (var page = GetTriggerPage(actionInfo))
            {
                return page.BuildViewUI(actionInfo);
            }
        }
        #endregion

        #region "Action Override"

        public override int ActionCount()
        {
            return 1;
        }

        public override string get_ActionName(int actionNumber)
        {
            switch (actionNumber)
            {
                case ActionSendMessageTANumber:
                    using (var page = new ActionPage(HS, pluginConfig))
                    {
                        return page.Name();
                    }

                default:
                    return string.Empty;
            }
        }

        public override string ActionBuildUI([AllowNull]string uniqueControlId, IPlugInAPI.strTrigActInfo actionInfo)
        {
            using (var page = GetActionPage(actionInfo))
            {
                return page.BuildEditUI(uniqueControlId, actionInfo);
            }
        }

        public override bool ActionConfigured(IPlugInAPI.strTrigActInfo actionInfo)
        {
            using (var page = GetActionPage(actionInfo))
            {
                return page.IsConfigured(actionInfo);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public override IPlugInAPI.strMultiReturn ActionProcessPostUI([AllowNull] NameValueCollection postData, IPlugInAPI.strTrigActInfo actionInfo)
        {
            using (var page = GetActionPage(actionInfo))
            {
                return page.ProcessPostUI(postData, actionInfo);
            }
        }

        public override string ActionFormatUI(IPlugInAPI.strTrigActInfo actionInfo)
        {
            using (var page = GetActionPage(actionInfo))
            {
                return page.BuildViewUI(actionInfo);
            }
        }

        public override bool HandleAction(IPlugInAPI.strTrigActInfo actionInfo)
        {
            using (var page = GetActionPage(actionInfo))
            {
                try
                {
                    return page.HandleEvent(actionInfo, twilioService);
                }
                catch (Exception ex)
                {
                    LogWarning($"Failed to execute action with {ex.GetFullMessage()}");
                    return false;
                }
            }
        }

        #endregion "Action Override"

        #region "Trigger Refresh Handler"

        private void ScheduleRefreshTrigger(int dueTime = triggerRefreshFrequencyMillis)
        {
            intervalRefreshTimer?.Dispose();
            intervalRefreshTimer = new Timer((x) => RefreshTriggers(), null,
                                             dueTime,
                                             Timeout.Infinite);
        }

        private void RefreshTriggers()
        {
            try
            {
                DoRefresh();
            }
            catch (Exception ex)
            {
                LogError(string.Format("Error performing trigger refresh: {0}", ex.GetFullMessage()));
            }
            finally
            {
                ScheduleRefreshTrigger();
            }
        }

        private void DoRefresh()
        {
            LogDebug("Refreshing Triggers");

            var triggers = Callback.TriggerMatches(Name, TriggerReceiveMessageTANumber, -1);

            if (triggers == null || triggers.Length == 0)
            {
                LogDebug("No triggers exist; aborting refresh");
                return;
            }

            var messages = twilioService.GetMessagesFromTwilio(pluginConfig, triggerRefreshFrequencyMillis / 1000);

            LogDebug(string.Format("Checking triggers against {0} messages", messages.Count));

            foreach (var strTrigActInfo in triggers)
            {
                if (ShutdownCancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var config = ReceiveMessageTriggerConfig.DeserializeTriggerConfig(strTrigActInfo.DataIn);
                if (config.IsValid())
                {
                    string messageToLower = config.Message.ToLower();
                    bool shouldFire = messages.Exists((MessageResource obj) =>
                    {
                        bool bodyMatches = obj.Body.ToLower().Contains(messageToLower);
                        bool fromMatches = config.FromNumber.IsNullOrWhiteSpace() || config.FromNumber == obj.From.ToString();
                        return fromMatches && bodyMatches;
                    });

                    if (shouldFire)
                    {
                        LogDebug("Firing trigger");
                        Callback.TriggerFire(Name, strTrigActInfo);
                    }
                }
                else
                {
                    LogDebug("Skipping trigger with invalid config");
                }
            }
        }
        #endregion

        private IEventPage GetActionPage(IPlugInAPI.strTrigActInfo actionInfo)
        {
            switch (actionInfo.TANumber)
            {
                case ActionSendMessageTANumber:
                    return new ActionPage(HS, pluginConfig);
                default:
                    return new NoOpPage();
            }
        }

        private IEventPage GetTriggerPage(IPlugInAPI.strTrigActInfo actionInfo)
        {
            switch(actionInfo.TANumber)
            {
                case TriggerReceiveMessageTANumber:
                    return new TriggerPage(HS, pluginConfig);
                default:
                    return new NoOpPage();
            }
        }

        private const int triggerRefreshFrequencyMillis = 15000;
    }
}