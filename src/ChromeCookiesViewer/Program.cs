using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ChromeCookiesViewer
{
    class Program
    {
        private enum UsageType
        {
            None,
            Save,
            Print,
            Delete
        }

        static void Main(string[] args)
        {
            UsageType usageType = UsageType.None;

            Regex hostRegex = null;
            Regex nameRegex = null;

            string outputFile = "";
            bool includeSameName = false;
            bool excludeExpired = false;
            int i = 0;

            /* Mandatory */
            if (args.Length >= 2 && (args[0] == "*" || IsValidRegex(args[0])))
            {
                hostRegex = new Regex(args[0] == "*" ? "(.*?)" : args[0], RegexOptions.Compiled);

                if (args[1].ToLower() == "-p")
                {
                    usageType = UsageType.Print;
                    i = 2;
                }
                else if (args[1].ToLower() == "-d")
                {
                    usageType = UsageType.Delete;
                    i = 2;
                }
                else if (args.Length >= 3 && args[1].ToLower() == "-s")
                {
                    usageType = UsageType.Save;
                    outputFile = args[2];
                    i = 3;
                }
            }

            if (usageType != UsageType.None)
            {
                var optionList = new List<string>();

                /* Check options */
                for (; i < args.Length; i++)
                {
                    var arg = args[i].ToLower();

                    if (arg.StartsWith("-") && !optionList.Contains(arg))
                    {
                        switch (arg)
                        {
                            case "-n":
                                if (i + 1 < args.Length && IsValidRegex(args[i + 1]))
                                {
                                    nameRegex = new Regex(args[i + 1], RegexOptions.Compiled);
                                    i++;
                                }
                                else
                                    usageType = UsageType.None;
                                break;

                            case "-a":
                                includeSameName = true;
                                break;

                            case "-e":
                                excludeExpired = true;
                                break;

                            default:
                                usageType = UsageType.None;
                                break;
                        }
                    }
                    else
                        usageType = UsageType.None;

                    if (usageType == UsageType.None)
                        break;

                    optionList.Add(arg);
                }
            }

            Console.WriteLine();

            if (usageType == UsageType.None)
            {
                Console.WriteLine("Allows you to export or delete the cookies stored by Google Chrome Web Browser.\r\n");

                Console.WriteLine("Usage: ChromeCookiesViewer <host> [-s <file> | -p | -d] [OPTIONS]\r\n");

                Console.WriteLine("   <host>        Regex for filtering the host name associated to the cookies.");
                Console.WriteLine("                 Use the asterisk (*) wildcard for all hosts.\r\n");
                Console.WriteLine("   -s <file>     Save the cookies in the specified text file (path can be absolute or relative).\r\n");
                Console.WriteLine("   -p            Print cookies in console.\r\n");
                Console.WriteLine("   -d            Delete cookies from the browser (Google Chrome must be closed).\r\n");

                Console.WriteLine("Options:\r\n");
				
                Console.WriteLine("   -n <name>     Regex for filtering the cookie names.\r\n");
                Console.WriteLine("   -a            Include multiple cookies with the same name.");
                Console.WriteLine("                 Without this option, in case of cookies with the same name,");
                Console.WriteLine("                 only the most up-to-date one will be extracted.\r\n");
                Console.WriteLine("   -e            Exclude expired cookies.");

                return;
            }

            try
            {
                var sb = new StringBuilder();
                var now = DateTime.Now;
                var foundCookies = new List<Cookie>();

                var allCookies = ChromeManager.GetCookies().Where(x => hostRegex.IsMatch(x.HostKey)).OrderBy(x => x.Name);

                if (includeSameName)
                    allCookies = allCookies.ThenBy(x => x.Creation);
                else
                    allCookies = allCookies.ThenByDescending(x => x.LastUpdate);

                foreach (var cookie in allCookies)
                {
                    /* Evaluate options */
                    if ((includeSameName || !foundCookies.Any(x => x.Name == cookie.Name)) &&
                        (!excludeExpired || cookie.Expires.Ticks == 0 || cookie.Expires > now) &&
                        (nameRegex == null || nameRegex.IsMatch(cookie.Name)))
                    {
                        if (usageType == UsageType.Save)
                        {
                            if (sb.Length > 0)
                                sb.Append("; ");

                            sb.Append(cookie.Name + "=" + cookie.Value);
                        }
                        else if (usageType == UsageType.Print)
                            Console.WriteLine(
                                "Name: " + cookie.Name + ", Value: " +
                                cookie.Value + ", Host: " + cookie.HostKey + ", Expires: " +
                                (cookie.Expires.Ticks == 0 ? "Never" : cookie.Expires.ToString("dd/MM/yyyy HH:mm:ss")) + "\r\n");

                        foundCookies.Add(cookie);
                    }
                }

                if (foundCookies.Count > 0)
                {
                    switch (usageType)
                    {
                        case UsageType.Save:
                            File.WriteAllText(outputFile, sb.ToString());
                            Console.WriteLine("Cookies saved: " + foundCookies.Count);
                            break;

                        case UsageType.Print:
                            Console.WriteLine("Cookies found: " + foundCookies.Count);
                            break;

                        case UsageType.Delete:
                            int deleted;

                            try
                            {
                                deleted = ChromeManager.DeleteCookies(foundCookies);
                            }
                            catch (SqliteException ex)
                            {
                                if (ex.SqliteErrorCode == 10) /* DB locked */
                                    throw new Exception("Cannot delete cookies while Google Chrome is running. You have to close it in order to proceed.", ex);

                                throw;
                            }

                            if (deleted > 0)
                            {
                                Console.WriteLine("Cookies deleted: " + deleted);

                                if (deleted != foundCookies.Count)
                                    Console.WriteLine("WARN: not all cookies have been deleted!");
                            }
                            else
                                Console.WriteLine("No cookies deleted.");
                            break;

                        default:
                            throw new Exception("Undefined usage");
                    }
                }
                else
                    Console.WriteLine("No cookies found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
            }
        }

        private static bool IsValidRegex(string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern))
                return false;

            try
            {
                Regex.Match("", pattern);
            }
            catch (ArgumentException)
            {
                return false;
            }

            return true;
        }
    }
}
