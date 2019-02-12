// ArxOne.Ftp.Platform.UnixAltFtpPlatform
using ArxOne.Ftp;
using ArxOne.Ftp.Platform;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ArxOne.Ftp.Platform
{
    /// <summary>
    /// Extended implementation for Unix platforms
    /// </summary>
    public class UnixAltFtpPlatform : FtpPlatform
    {
        private string[] unixDateFormats1 = new string[2]
        {
        "MMM'-'d'-'yyyy",
        "MMM'-'dd'-'yyyy"
        };

        private string[] unixDateFormats2 = new string[5]
        {
        "MMM'-'d'-'yyyy'-'HH':'mm",
        "MMM'-'dd'-'yyyy'-'HH':'mm",
        "MMM'-'d'-'yyyy'-'H':'mm",
        "MMM'-'dd'-'yyyy'-'H':'mm",
        "MMM'-'dd'-'yyyy'-'H'.'mm"
        };

        private string[] unixAltDateFormats1 = new string[2]
        {
        "MMM'-'d'-'yyyy",
        "MMM'-'dd'-'yyyy"
        };

        private string[] unixAltDateFormats2 = new string[4]
        {
        "MMM'-'d'-'yyyy'-'HH':'mm:ss",
        "MMM'-'dd'-'yyyy'-'HH':'mm:ss",
        "MMM'-'d'-'yyyy'-'H':'mm:ss",
        "MMM'-'dd'-'yyyy'-'H':'mm:ss"
        };

        private string[] windowsDateFormats = new string[3]
        {
        "MM'-'dd'-'yy hh':'mmtt",
        "MM'-'dd'-'yy HH':'mm",
        "MM'-'dd'-'yyyy hh':'mmtt"
        };

        private string[][] ibmDateFormats = new string[3][]
        {
        new string[3]
        {
            "dd'/'MM'/'yy' 'HH':'mm':'ss",
            "dd'/'MM'/'yyyy' 'HH':'mm':'ss",
            "dd'.'MM'.'yy' 'HH':'mm':'ss"
        },
        new string[3]
        {
            "yy'/'MM'/'dd' 'HH':'mm':'ss",
            "yyyy'/'MM'/'dd' 'HH':'mm':'ss",
            "yy'.'MM'.'dd' 'HH':'mm':'ss"
        },
        new string[3]
        {
            "MM'/'dd'/'yy' 'HH':'mm':'ss",
            "MM'/'dd'/'yyyy' 'HH':'mm':'ss",
            "MM'.'dd'.'yy' 'HH':'mm':'ss"
        }
        };

        private string[] nonstopDateFormats = new string[1]
        {
        "d'-'MMM'-'yy HH':'mm':'ss"
        };

        private static int MIN_EXPECTED_FIELD_COUNT_UNIXALT = 8;

        private static char SYMLINK_CHAR = 'l';

        private static char ORDINARY_FILE_CHAR = '-';

        private static char DIRECTORY_CHAR = 'd';

        private static string[] SplitString(string str)
        {
            List<string> list = new List<string>(str.Split(null));
            for (int num = list.Count - 1; num >= 0; num--)
            {
                if (list[num].Trim().Length == 0)
                {
                    list.RemoveAt(num);
                }
            }
            return list.ToArray();
        }
        /// <summary>
        /// Escapes the path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public override string EscapePath(string path)
        {
            return EscapePath(path, " []()");
        }

        /// <summary>
        /// Escapes the path.
        /// </summary>
        /// <param name="directoryLine">The path.</param>
        /// /// <param name="parent">The parent path.</param>
        /// <returns></returns>
        public override FtpEntry Parse(string directoryLine, FtpPath parent)
        {
            try
            {
                char c = directoryLine[0];
                if (c != ORDINARY_FILE_CHAR && c != DIRECTORY_CHAR && c != SYMLINK_CHAR)
                {
                    return null;
                }
                string[] array = SplitString(directoryLine);
                if (array.Length < MIN_EXPECTED_FIELD_COUNT_UNIXALT)
                {
                    StringBuilder stringBuilder = new StringBuilder("Unexpected number of fields in listing '");
                    stringBuilder.Append(directoryLine).Append("' - expected minimum ").Append(MIN_EXPECTED_FIELD_COUNT_UNIXALT)
                        .Append(" fields but found ")
                        .Append(array.Length)
                        .Append(" fields");
                    throw new FormatException(stringBuilder.ToString());
                }
                int num = 0;
                c = array[num++][0];
                FtpEntryType type = FtpEntryType.File;
                if (c == DIRECTORY_CHAR)
                {
                    type = FtpEntryType.Directory;
                }
                else if (c == SYMLINK_CHAR)
                {
                    type = FtpEntryType.Link;
                }
                string text2 = array[num++];
                if (char.IsDigit(array[num][0]))
                {
                    string s = array[num++];
                    try
                    {
                        int.Parse(s);
                    }
                    catch (FormatException)
                    {
                    }
                }
                string text3 = array[num++];
                long value = 0L;
                string s2 = array[num++];
                try
                {
                    value = long.Parse(s2);
                }
                catch (FormatException)
                {
                }
                int num7 = num;
                DateTime dateTime = DateTime.MinValue;
                StringBuilder stringBuilder2 = new StringBuilder(array[num++]);
                stringBuilder2.Append('-').Append(array[num++]).Append('-');
                string text = array[num++];
                if (text.IndexOf(':') < 0)
                {
                    stringBuilder2.Append(text);
                    try
                    {
                        dateTime = DateTime.ParseExact(stringBuilder2.ToString(), unixAltDateFormats1, CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.None);
                    }
                    catch (FormatException)
                    {
                    }
                }
                else
                {
                    int year = CultureInfo.InvariantCulture.Calendar.GetYear(DateTime.Now);
                    stringBuilder2.Append(year).Append('-').Append(text);
                    try
                    {
                        dateTime = DateTime.ParseExact(stringBuilder2.ToString(), unixAltDateFormats2, CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.None);
                    }
                    catch (FormatException)
                    {
                    }
                    if (dateTime > DateTime.Now.AddDays(2.0))
                    {
                        dateTime = dateTime.AddYears(-1);
                    }
                }
                string name = null;
                int num11 = 0;
                bool flag = true;
                for (int i = num7; i < num7 + 3; i++)
                {
                    num11 = directoryLine.IndexOf(array[i], num11);
                    if (num11 < 0)
                    {
                        flag = false;
                        break;
                    }
                    num11 += array[i].Length;
                }
                if (flag)
                {
                    name = directoryLine.Substring(num11).Trim();
                }
                return new FtpEntry(parent, name, value, type, dateTime, null);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}