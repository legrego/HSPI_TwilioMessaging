using System;
using System.Collections.Specialized;
using HomeSeerAPI;
using NullGuard;

namespace Hspi.Pages
{
    internal interface IEventPage: IDisposable
    {
        string Name();

        bool IsConfigured(IPlugInAPI.strTrigActInfo actionInfo);

        string BuildEditUI(string uniqueControlId, IPlugInAPI.strTrigActInfo actionInfo);

        string BuildViewUI(IPlugInAPI.strTrigActInfo actionInfo);

        IPlugInAPI.strMultiReturn ProcessPostUI(NameValueCollection postData, IPlugInAPI.strTrigActInfo actionInfo);

        bool HandleEvent(IPlugInAPI.strTrigActInfo actionInfo, TwilioServiceFacade twilioService);
    }
}
