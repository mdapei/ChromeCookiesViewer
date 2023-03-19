using System;

namespace ChromeCookiesViewer
{
    public class Cookie
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string HostKey { get; set; }
        public string Path { get; set; }

        private long _creationUTC;
        private long _lastUpdateUTC;
        private long _expiresUTC;

        public long CreationUTC
        {
            get
            {
                return _creationUTC;
            }
            set
            {
                _creationUTC = value;
                Creation = UnixTimeStampToDateTime(CookieTimeStampToUnixTime(value));
            }
        }
        
        public long LastUpdateUTC
        {
            get
            {
                return _lastUpdateUTC;
            }
            set
            {
                _lastUpdateUTC = value;
                LastUpdate = UnixTimeStampToDateTime(CookieTimeStampToUnixTime(value));
            }
        }
        
        public long ExpiresUTC
        {
            get
            {
                return _expiresUTC;
            }
            set
            {
                _expiresUTC = value;
                Expires = UnixTimeStampToDateTime(CookieTimeStampToUnixTime(value));
            }
        }

        public DateTime Creation { get; private set; }

        public DateTime LastUpdate { get; private set; }

        public DateTime Expires { get; private set; }

        private static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            /* Unix timestamp is seconds past epoch */
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }

        /* Chrome's cookies timestamp's epoch starts 1601-01-01T00:00:00Z (why???)
         * So, it's 11644473600 seconds before the UNIX epoch */
        private static long CookieTimeStampToUnixTime(long cookieTimeStamp)
        {
            if (cookieTimeStamp <= 0)
                return 0;

            return (cookieTimeStamp / 1000000) - 11644473600;
        }
    }
}
