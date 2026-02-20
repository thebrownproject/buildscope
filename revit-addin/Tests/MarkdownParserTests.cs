using Xunit;

namespace BuildScope.Tests
{
    public class MarkdownParserTests
    {
        [Fact]
        public void Parse_PlainText_ReturnsSingleParagraphLine()
        {
            var lines = MarkdownParser.Parse("Hello world");

            Assert.Single(lines);
            Assert.Equal(LineType.Paragraph, lines[0].Type);
            Assert.Single(lines[0].Segments);
            Assert.Equal("Hello world", lines[0].Segments[0].Text);
            Assert.Equal(SegmentType.Normal, lines[0].Segments[0].Type);
        }

        [Fact]
        public void Parse_BoldText_ExtractsBoldSegment()
        {
            var lines = MarkdownParser.Parse("This is **bold** text");

            Assert.Single(lines);
            Assert.Equal(3, lines[0].Segments.Count);
            Assert.Equal("This is ", lines[0].Segments[0].Text);
            Assert.Equal(SegmentType.Normal, lines[0].Segments[0].Type);
            Assert.Equal("bold", lines[0].Segments[1].Text);
            Assert.Equal(SegmentType.Bold, lines[0].Segments[1].Type);
            Assert.Equal(" text", lines[0].Segments[2].Text);
            Assert.Equal(SegmentType.Normal, lines[0].Segments[2].Type);
        }

        [Fact]
        public void Parse_MultipleBold_ExtractsAll()
        {
            var lines = MarkdownParser.Parse("**R2.8** per **H1V3**");

            Assert.Single(lines);
            var segs = lines[0].Segments;
            Assert.Equal(3, segs.Count);
            Assert.Equal("R2.8", segs[0].Text);
            Assert.Equal(SegmentType.Bold, segs[0].Type);
            Assert.Equal(" per ", segs[1].Text);
            Assert.Equal(SegmentType.Normal, segs[1].Type);
            Assert.Equal("H1V3", segs[2].Text);
            Assert.Equal(SegmentType.Bold, segs[2].Type);
        }

        [Fact]
        public void Parse_BulletList_IdentifiesBullets()
        {
            var lines = MarkdownParser.Parse("- Item one\n- Item two\n- Item three");

            Assert.Equal(3, lines.Count);
            foreach (var line in lines)
            {
                Assert.Equal(LineType.Bullet, line.Type);
            }
            Assert.Equal("Item one", lines[0].Segments[0].Text);
            Assert.Equal("Item two", lines[1].Segments[0].Text);
            Assert.Equal("Item three", lines[2].Segments[0].Text);
        }

        [Fact]
        public void Parse_AsteriskBullets_IdentifiesBullets()
        {
            var lines = MarkdownParser.Parse("* First\n* Second");

            Assert.Equal(2, lines.Count);
            Assert.All(lines, l => Assert.Equal(LineType.Bullet, l.Type));
        }

        [Fact]
        public void Parse_Headers_IdentifiesLevels()
        {
            var lines = MarkdownParser.Parse("# H1\n## H2\n### H3");

            Assert.Equal(3, lines.Count);
            Assert.Equal(LineType.Header, lines[0].Type);
            Assert.Equal(1, lines[0].HeaderLevel);
            Assert.Equal("H1", lines[0].Segments[0].Text);

            Assert.Equal(LineType.Header, lines[1].Type);
            Assert.Equal(2, lines[1].HeaderLevel);
            Assert.Equal("H2", lines[1].Segments[0].Text);

            Assert.Equal(LineType.Header, lines[2].Type);
            Assert.Equal(3, lines[2].HeaderLevel);
            Assert.Equal("H3", lines[2].Segments[0].Text);
        }

        [Fact]
        public void Parse_MixedContent_HandlesAll()
        {
            var input = "## Requirements\nWalls must achieve **R2.8**.\n- Insulation required\n- Vapor barrier per **H4V2**";
            var lines = MarkdownParser.Parse(input);

            Assert.Equal(4, lines.Count);
            Assert.Equal(LineType.Header, lines[0].Type);
            Assert.Equal(LineType.Paragraph, lines[1].Type);
            Assert.Equal(LineType.Bullet, lines[2].Type);
            Assert.Equal(LineType.Bullet, lines[3].Type);

            // Bold within bullet
            var bulletSegs = lines[3].Segments;
            Assert.Equal("Vapor barrier per ", bulletSegs[0].Text);
            Assert.Equal("H4V2", bulletSegs[1].Text);
            Assert.Equal(SegmentType.Bold, bulletSegs[1].Type);
        }

        [Fact]
        public void Parse_EmptyString_ReturnsEmpty()
        {
            Assert.Empty(MarkdownParser.Parse(""));
            Assert.Empty(MarkdownParser.Parse(null!));
        }

        [Fact]
        public void Parse_BlankLines_AreSkipped()
        {
            var lines = MarkdownParser.Parse("Line one\n\nLine two\n\n\nLine three");

            Assert.Equal(3, lines.Count);
            Assert.Equal("Line one", lines[0].Segments[0].Text);
            Assert.Equal("Line two", lines[1].Segments[0].Text);
            Assert.Equal("Line three", lines[2].Segments[0].Text);
        }
    }
}
