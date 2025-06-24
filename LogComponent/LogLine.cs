namespace LogComponent;

/// <summary>
/// Represents a single line in the log, with a timestamp and text content.
/// </summary>
public record LogLine(DateTime Timestamp, string Text)
{
    /// <summary>
    /// Returns a formatted version of the log line.
    /// </summary>
    public string LineText()
    {
        var prefix = string.IsNullOrWhiteSpace(Text) ? "" : Text + ". ";
        return prefix + CreateLineText();
    }

    /// <summary>
    /// Override this in derived records for custom formatting.
    /// </summary>
    public virtual string CreateLineText() => "";
}
