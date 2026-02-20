using System.Text.RegularExpressions;

namespace BuildSpec
{
    public enum SegmentType { Normal, Bold }
    public enum LineType { Paragraph, Bullet, Header }

    public class MarkdownSegment
    {
        public string Text { get; set; } = "";
        public SegmentType Type { get; set; }
    }

    public class MarkdownLine
    {
        public List<MarkdownSegment> Segments { get; set; } = new();
        public LineType Type { get; set; }
        public int HeaderLevel { get; set; }
    }

    public static class MarkdownParser
    {
        private static readonly Regex BoldPattern = new(@"\*\*(.+?)\*\*", RegexOptions.Compiled);

        public static List<MarkdownLine> Parse(string text)
        {
            if (string.IsNullOrEmpty(text))
                return new List<MarkdownLine>();

            var lines = text.Split('\n');
            var result = new List<MarkdownLine>();

            foreach (var rawLine in lines)
            {
                var trimmed = rawLine.TrimEnd();
                if (string.IsNullOrWhiteSpace(trimmed))
                    continue;

                var line = new MarkdownLine();

                if (trimmed.StartsWith("### "))
                {
                    line.Type = LineType.Header;
                    line.HeaderLevel = 3;
                    line.Segments = ParseInlineFormatting(trimmed[4..]);
                }
                else if (trimmed.StartsWith("## "))
                {
                    line.Type = LineType.Header;
                    line.HeaderLevel = 2;
                    line.Segments = ParseInlineFormatting(trimmed[3..]);
                }
                else if (trimmed.StartsWith("# "))
                {
                    line.Type = LineType.Header;
                    line.HeaderLevel = 1;
                    line.Segments = ParseInlineFormatting(trimmed[2..]);
                }
                else if (trimmed.StartsWith("- ") || trimmed.StartsWith("* "))
                {
                    line.Type = LineType.Bullet;
                    line.Segments = ParseInlineFormatting(trimmed[2..]);
                }
                else
                {
                    line.Type = LineType.Paragraph;
                    line.Segments = ParseInlineFormatting(trimmed);
                }

                result.Add(line);
            }

            return result;
        }

        public static List<MarkdownSegment> ParseInlineFormatting(string text)
        {
            var segments = new List<MarkdownSegment>();
            var matches = BoldPattern.Matches(text);

            int pos = 0;
            foreach (Match match in matches)
            {
                if (match.Index > pos)
                {
                    segments.Add(new MarkdownSegment
                    {
                        Text = text[pos..match.Index],
                        Type = SegmentType.Normal
                    });
                }

                segments.Add(new MarkdownSegment
                {
                    Text = match.Groups[1].Value,
                    Type = SegmentType.Bold
                });

                pos = match.Index + match.Length;
            }

            if (pos < text.Length)
            {
                segments.Add(new MarkdownSegment
                {
                    Text = text[pos..],
                    Type = SegmentType.Normal
                });
            }

            return segments;
        }
    }
}
