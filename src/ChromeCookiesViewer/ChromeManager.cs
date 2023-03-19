using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ChromeCookiesViewer
{
    public static class ChromeManager
    {
        private static string CHROME_COOKIE_PATH = Path.Combine
            (Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Google\Chrome\User Data\Default\Network\Cookies");

        public static List<Cookie> GetCookies()
        {
            var data = new List<Cookie>();

            if (File.Exists(CHROME_COOKIE_PATH))
            {
                using (var conn = new SqliteConnection($"Data Source={CHROME_COOKIE_PATH}"))
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM cookies";

                    byte[] key = AesGcm256.GetKey();

                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        var columns = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();
                        
                        byte[] nonce, ciphertextTag;

                        while (reader.Read())
                        {
                            AesGcm256.prepare((byte[])reader["encrypted_value"], out nonce, out ciphertextTag);
                            string value = AesGcm256.decrypt(ciphertextTag, key, nonce);

                            data.Add(new Cookie()
                            {
                                Name = (string)reader["name"],
                                Value = value,
                                HostKey = (string)reader["host_key"],
                                Path = (string)reader["path"],
                                CreationUTC = (long)reader["creation_utc"],
                                LastUpdateUTC = (long)reader["last_update_utc"],
                                ExpiresUTC = (long)reader["expires_utc"]
                            });
                        }
                    }

                    conn.Close();
                }
            }

            return data;
        }

        public static int DeleteCookies(List<Cookie> cookies)
        {
            int deleted = 0;

            if (File.Exists(CHROME_COOKIE_PATH))
            {
                using (var conn = new SqliteConnection($"Data Source={CHROME_COOKIE_PATH}"))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();

                    foreach (var cookie in cookies)
                    {
                        cmd.CommandText = "DELETE FROM cookies WHERE " + 
                            "host_key = '" + cookie.HostKey + "' AND name = '" + cookie.Name + "' AND creation_utc = " + cookie.CreationUTC;

                        deleted += cmd.ExecuteNonQuery();
                    }

                    conn.Close();
                }
            }

            return deleted;
        }
    }
}
