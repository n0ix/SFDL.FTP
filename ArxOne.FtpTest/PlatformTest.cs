﻿#region Arx One FTP
// Arx One FTP
// A simple FTP client
// https://github.com/ArxOne/FTP
// Released under MIT license http://opensource.org/licenses/MIT
#endregion

namespace ArxOne.FtpTest
{
    using System;
    using System.Linq;
    using Ftp;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    internal static class PlatformTest
    {
        public static void SpaceNameTest(string platform, string protocol = "ftp", FtpProtection? protection = null)
        {
            NameTest(platform, "A and B", "C and D", protocol, protection);
        }

        public static void BracketNameTest(string platform, string protocol = "ftp", FtpProtection? protection = null)
        {
            NameTest(platform, "X[]Y", "Z{}[]T", protocol, protection);
        }

        public static void ParenthesisNameTest(string platform, string protocol = "ftp", FtpProtection? protection = null)
        {
            NameTest(platform, "i()j", "k()l", protocol, protection);
        }

        private static void NameTest(string platform, string folderName, string childName, string protocol = "ftp", FtpProtection? protection = null)
        {
            if (string.Equals(platform, "FileZilla", StringComparison.InvariantCultureIgnoreCase))
                Assert.Inconclusive("FileZilla does not support escaping for special names (and yes, this is a shame)");
            var testHost = TestHost.Get(protocol, platform);
            using (var ftpClient = new FtpClient(testHost.Uri, testHost.Credential, new FtpClientParameters { ChannelProtection = protection }))
            {
                var folder = (ftpClient.ServerType == FtpServerType.Windows ? "/" : "/tmp/") + folderName;
                var file = folder + "/" + childName;
                try
                {
                    ftpClient.Mkd(folder);
                    using (var s = ftpClient.Stor(file))
                        s.WriteByte(123);

                    var c = ftpClient.ListEntries(folder).SingleOrDefault();
                    Assert.IsNotNull(c);
                    Assert.AreEqual(childName, c.Name);
                    var c2 = ftpClient.StatEntries(folder).SingleOrDefault();
                    Assert.IsNotNull(c2);
                    Assert.AreEqual(childName, c2.Name);

                    using (var r = ftpClient.Retr(file))
                    {
                        Assert.AreEqual(123, r.ReadByte());
                        Assert.AreEqual(-1, r.ReadByte());
                    }
                }
                finally
                {
                    ftpClient.Dele(file);
                    ftpClient.Rmd(folder);
                }
            }
        }

        public static void ListTest(string platform, bool passive, string protocol = "ftp", string directory = "/", bool directoryExists = true, FtpProtection? protection = null)
        {
            if (!directoryExists && string.Equals(platform, "PureFTPd", StringComparison.InvariantCultureIgnoreCase))
                Assert.Inconclusive("PureFTPd always gives a valid response, even if the directory does not exist");

            var ftpTestHost = TestHost.Get(protocol, platform);
            using (var ftpClient = new FtpClient(ftpTestHost.Uri, ftpTestHost.Credential, new FtpClientParameters { Passive = passive, ChannelProtection = protection }))
            {
                if (string.Equals(platform, "FileZilla", StringComparison.InvariantCultureIgnoreCase)
                    && ftpClient.Protocol != FtpProtocol.Ftp)
                    Assert.Inconclusive("FileZilla causes me problems that I don't understand here (help welcome)");

                var list = ftpClient.ListEntries(directory);
                // a small requirement: have a /tmp folderS
                Assert.IsTrue(list.Any(e => e.Name == "tmp"));
            }
        }

        public static void StatTest(string platform, string protocol = "ftp", FtpProtection? protection = null)
        {
            if (string.Equals(platform, "FileZilla", StringComparison.InvariantCultureIgnoreCase))
                Assert.Inconclusive("FileZilla does not support escaping for special names (and yes, it just sucks)");
            var ftpTestHost = TestHost.Get(protocol, platform);
            using (var ftpClient = new FtpClient(ftpTestHost.Uri, ftpTestHost.Credential, new FtpClientParameters { ChannelProtection = protection }))
            {
                var list = ftpClient.StatEntries("/");
                // a small requirement: have a /tmp folderS
                Assert.IsTrue(list.Any(e => e.Name == "tmp"));
            }
        }

        public static void StatNoDotTest(string platform, string protocol = "ftp", FtpProtection? protection = null)
        {
            if (string.Equals(platform, "FileZilla", StringComparison.InvariantCultureIgnoreCase))
                Assert.Inconclusive("FileZilla does not support escaping for special names (and yes, it just sucks)");
            var ftpTestHost = TestHost.Get(protocol, platform);
            using (var ftpClient = new FtpClient(ftpTestHost.Uri, ftpTestHost.Credential, new FtpClientParameters { ChannelProtection = protection }))
            {
                var list = ftpClient.StatEntries("/");
                Assert.IsFalse(list.Any(e => e.Name == "." || e.Name == ".."));
            }
        }

        public static void CreateFileTest(string platform, bool passive, string protocol = "ftp", FtpProtection? protection = null)
        {
            var ftpesTestHost = TestHost.Get(protocol, platform);
            using (var ftpClient = new FtpClient(ftpesTestHost.Uri, ftpesTestHost.Credential, new FtpClientParameters { Passive = passive, ChannelProtection = protection }))
            {
                var directory = ftpClient.ServerType == FtpServerType.Windows ? "/" : "/tmp/";
                var path = directory + "file." + Guid.NewGuid();
                using (var s = ftpClient.Stor(path))
                {
                    s.WriteByte(65);
                }
                using (var r = ftpClient.Retr(path))
                {
                    Assert.IsNotNull(r);
                    Assert.AreEqual(65, r.ReadByte());
                    Assert.AreEqual(-1, r.ReadByte());
                }
                ftpClient.Dele(path);
            }
        }

        public static void MlstTest(string platform, bool passive = true, string protocol = "ftp", FtpProtection? protection = null)
        {
            var ftpTestHost = TestHost.Get(protocol, platform);
            using (var ftpClient = new FtpClient(ftpTestHost.Uri, ftpTestHost.Credential, new FtpClientParameters { Passive = passive, ChannelProtection = protection }))
            {
                ExpectFeature(ftpClient, "MLST");
                var m = ftpClient.Mlst("/");
            }
        }

        public static void MlstEntryTest(string platform, bool passive = true, string protocol = "ftp", FtpProtection? protection = null)
        {
            var ftpTestHost = TestHost.Get(protocol, platform);
            using (var ftpClient = new FtpClient(ftpTestHost.Uri, ftpTestHost.Credential, new FtpClientParameters { Passive = passive, ChannelProtection = protection }))
            {
                ExpectFeature(ftpClient, "MLST");
                var e = ftpClient.MlstEntry("/");
            }
        }

        public static void MlsdTest(string platform, bool passive = true, string protocol = "ftp", FtpProtection? protection = null)
        {
            var ftpTestHost = TestHost.Get(protocol, platform);
            using (var ftpClient = new FtpClient(ftpTestHost.Uri, ftpTestHost.Credential, new FtpClientParameters { Passive = passive, ChannelProtection = protection }))
            {
                ExpectFeature(ftpClient, "MLSD");
                var list = ftpClient.Mlsd("/").ToList();
            }
        }

        public static void MlsdEntriesTest(string platform, bool passive = true, string protocol = "ftp", FtpProtection? protection = null)
        {
            var ftpTestHost = TestHost.Get(protocol, platform);
            using (var ftpClient = new FtpClient(ftpTestHost.Uri, ftpTestHost.Credential, new FtpClientParameters { Passive = passive, ChannelProtection = protection }))
            {
                ExpectFeature(ftpClient, "MLSD");
                var list = ftpClient.MlsdEntries("/").ToList();
            }
        }

        private static void ExpectFeature(FtpClient ftpClient, string feature)
        {
            if (!ftpClient.ServerFeatures.HasFeature(feature))
                Assert.Inconclusive("This server does not support {0} feature", feature);
        }
    }
}