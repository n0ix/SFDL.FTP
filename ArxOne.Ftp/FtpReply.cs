#region Arx One FTP
// Arx One FTP
// A simple FTP client
// https://github.com/ArxOne/FTP
// Released under MIT license http://opensource.org/licenses/MIT
#endregion
namespace ArxOne.Ftp
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text.RegularExpressions;

    /// <summary>
    /// FTP Reply line
    /// </summary>
    [DebuggerDisplay("FTP {Code.Code} {Lines[0]}")]
    public class FtpReply
    {

        /// <summary>
        /// Gets or sets the code.
        /// </summary>
        /// <value>The issued FTPCommand for this FtpReply</value>
        public string IssuedFtpCommand { get; set; }

        /// <summary>
        /// Gets or sets the code.
        /// </summary>
        /// <value>The issued FTPCommand Parameters for this FtpReply</value>
        public string[] IssuedFtpCommandParameters { get; set; }

        /// <summary>
        /// Gets or sets the code.
        /// </summary>
        /// <value>The code.</value>
        public FtpReplyCode Code { get; set; }

        /// <summary>
        /// Gets or sets the lines.
        /// </summary>
        /// <value>The lines.</value>
        public string[] Lines { get; private set; }

        /// <summary>
        /// Parses the line.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <returns></returns>
        internal bool ParseLine(string line)
        {
            Match m;

            if ((m = Regex.Match(line, "^(?<code>[0-9]{3}) (?<message>.*)$")).Success)
            {
                Code = new FtpReplyCode(int.Parse(m.Groups["code"].Value));
                AppendLine(m.Groups["message"].Value);

                if (Lines.Length > 0)
                {

                    string lastline = "";
                    string firstline = "";

                    firstline = Lines[0].ToString().Trim().ToLower();

                    lastline = Lines[Lines.Length - 1].ToString().Trim().ToLower();

                    if (lastline.Equals ("end") && !firstline.StartsWith("211"))
                    {
                        Lines = null;
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }
                else
                {
                    return false;
                }
              
            }
            else
            {
                AppendLine(line);
                return true;
            }

                      
        }

        /// <summary>
        /// Appends the line.
        /// </summary>
        /// <param name="line">The line.</param>
        private void AppendLine(string line)
        {
            var lines = new List<string>();
            if (Lines != null)
                lines.AddRange(Lines);
            lines.Add(line);
            Lines = lines.ToArray();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpReply"/> class.
        /// </summary>
        public FtpReply()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpReply"/> class.
        /// </summary>
        /// <param name="lines">The lines.</param>
        public FtpReply(IEnumerable<string> lines)
        {
            foreach (var line in lines)
            {
                if(!ParseLine(line))
                    break;
            }
        }
    }
}
