using System.Text.RegularExpressions;

public static class TextFormatter
{
    public static string ApplyRichTextFormatting(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        input = Regex.Replace(input, @"\*\*\*(.+?)\*\*\*", "<b><i>$1</i></b>");

        input = Regex.Replace(input, @"\*\*(.+?)\*\*", "<b>$1</b>");

        input = Regex.Replace(input, @"\*(.+?)\*", "<i>$1</i>");

        input = Regex.Replace(input, @"__(.+?)__", "<b>$1</b>");

        input = Regex.Replace(input, @"_(.+?)_", "<i>$1</i>");

        input = Regex.Replace(input, @"\[(.+?)\]\((.+?)\)", "<b>$1</b>");

        return input;
    }
}
