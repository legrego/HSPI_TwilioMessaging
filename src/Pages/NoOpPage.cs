using System;
using System.Collections.Specialized;
using HomeSeerAPI;
using NullGuard;

namespace Hspi.Pages
{
    internal class NoOpPage: IEventPage
    {

        public string BuildEditUI([AllowNull] string uniqueControlId, IPlugInAPI.strTrigActInfo actionInfo)
        {
            return string.Empty;
        }

        public string BuildViewUI(IPlugInAPI.strTrigActInfo actionInfo)
        {
            return string.Empty;
        }

        public void Dispose()
        {

        }

        public bool IsConfigured(IPlugInAPI.strTrigActInfo actionInfo)
        {
            return false;
        }

        public string Name()
        {
            return string.Empty;
        }

        public IPlugInAPI.strMultiReturn ProcessPostUI([AllowNull] NameValueCollection postData, IPlugInAPI.strTrigActInfo actionInfo)
        {
            return new IPlugInAPI.strMultiReturn();
        }

        public bool HandleEvent(IPlugInAPI.strTrigActInfo actionInfo, TwilioServiceFacade twilioService)
        {
            return false;
        }
    }
}
