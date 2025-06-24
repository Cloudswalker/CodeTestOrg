using System.Text;

namespace LogComponent
{
    /// <summary>
    /// This is the object that the diff. loggers (filelogger, consolelogger etc.) will operate on. The LineText() method will be called to get the text (formatted) to log
    /// </summary>
    public class LogLine
    {
        public LogLine()
        {
            this.Text = "";
        }

        /// <summary>
        /// Return a formatted line
        /// </summary>
        /// <returns></returns>
        public virtual string LineText()
        {
            StringBuilder sb = new StringBuilder();

            if (this.Text.Length > 0)
            {
                sb.Append(this.Text);
                sb.Append(". ");
            }

            sb.Append(this.CreateLineText());

            return sb.ToString();
        }

        public virtual string CreateLineText()
        {
            return "";
        }

        /// <summary>
        /// The text to be display in logline
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// The Timestamp is initialized when the log is added. Th
        /// </summary>
        public virtual DateTime Timestamp { get; set; }
    }
}