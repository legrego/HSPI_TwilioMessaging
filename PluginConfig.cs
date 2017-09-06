using HomeSeerAPI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;

namespace Hspi
{

    /// <summary>
    /// Class to store PlugIn Configuration
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    internal class PluginConfig : IDisposable
    {
        public event EventHandler<EventArgs> ConfigChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginConfig"/> class.
        /// </summary>
        /// <param name="HS">The homeseer application.</param>
        public PluginConfig(IHSApplication HS)
        {
            this.HS = HS;

            authToken = GetValue(AuthTokenKey, string.Empty);
            debugLogging = GetValue(DebugLoggingKey, false);
            accountSID = GetValue<string>(AccountSIDKey, string.Empty);
            fromNumber = GetValue<string>(FromNumberKey, string.Empty);
        }

        /// <summary>
        /// Gets or sets the Account SID for Twilio
        /// </summary>
        /// <value>
        /// The Account SID.
        /// </value>
        public string AccountSID
        {
            get
            {
                configLock.EnterReadLock();
                try
                {
                    return accountSID;
                }
                finally
                {
                    configLock.ExitReadLock();
                }
            }

            set
            {
                configLock.EnterWriteLock();
                try
                {
                    SetValue(AccountSIDKey, value, ref accountSID);
                }
                finally
                {
                    configLock.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// Gets or sets the Auth Token.
        /// </summary>
        /// <value>
        /// The auth token.
        /// </value>
        public string AuthToken
        {
            get
            {
                configLock.EnterReadLock();
                try
                {
                    return authToken;
                }
                finally
                {
                    configLock.ExitReadLock();
                }
            }

            set
            {
                configLock.EnterWriteLock();
                try
                {
                    SetValue(AuthTokenKey, value, ref authToken);
                }
                finally
                {
                    configLock.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether debug logging is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [debug logging]; otherwise, <c>false</c>.
        /// </value>
        public bool DebugLogging
        {
            get
            {
                configLock.EnterReadLock();
                try
                {
                    return debugLogging;
                }
                finally
                {
                    configLock.ExitReadLock();
                }
            }

            set
            {
                configLock.EnterWriteLock();
                try
                {
                    SetValue(DebugLoggingKey, value, ref debugLogging);
                }
                finally
                {
                    configLock.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// Gets or sets the From Number value.
        /// </summary>
        public string FromNumber
        {
            get
            {
                configLock.EnterReadLock();
                try
                {
                    return fromNumber;
                }
                finally
                {
                    configLock.ExitReadLock();
                }
            }

            set
            {
                configLock.EnterWriteLock();
                try
                {
                    SetValue(FromNumberKey, value, ref fromNumber);
                }
                finally
                {
                    configLock.ExitWriteLock();
                }
            }
        }

        private T GetValue<T>(string key, T defaultValue)
        {
            return GetValue(key, defaultValue, DefaultSection);
        }

        private T GetValue<T>(string key, T defaultValue, string section)
        {
            string stringValue = HS.GetINISetting(section, key, null, FileName);

            if (stringValue != null)
            {
                try
                {
                    T result = (T)System.Convert.ChangeType(stringValue, typeof(T), CultureInfo.InvariantCulture);
                    return result;
                }
                catch (Exception)
                {
                    return defaultValue;
                }
            }
            return defaultValue;
        }

        private void SetValue<T>(string key, T value, ref T oldValue)
        {
            SetValue<T>(key, value, ref oldValue, DefaultSection);
        }

        private void SetValue<T>(string key, T value, ref T oldValue, string section)
        {
            if (!value.Equals(oldValue))
            {
                string stringValue = System.Convert.ToString(value, CultureInfo.InvariantCulture);
                HS.SaveINISetting(section, key, stringValue, FileName);
                oldValue = value;
            }
        }

        /// <summary>
        /// Fires event that configuration changed.
        /// </summary>
        public void FireConfigChanged()
        {
            if (ConfigChanged != null)
            {
                var ConfigChangedCopy = ConfigChanged;
                ConfigChangedCopy(this, EventArgs.Empty);
            }
        }

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    configLock.Dispose();
                }
                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
        }

        #endregion IDisposable Support

        private const string AccountSIDKey = "AccountSID";
        private const string AuthTokenKey = "AuthToken";
        private const string FromNumberKey = "FromNumber";
        private const string DebugLoggingKey = "DebugLogging";
        private readonly static string FileName = $"{Path.GetFileName(System.Reflection.Assembly.GetEntryAssembly().Location)}.ini";
        private const string DefaultSection = "Settings";
        
        private readonly IHSApplication HS;
        private string accountSID;
        private bool debugLogging;
        private string authToken;
        private string fromNumber;
        private bool disposedValue = false;
        private readonly ReaderWriterLockSlim configLock = new ReaderWriterLockSlim();
    };
}