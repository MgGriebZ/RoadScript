using System.Text;
using System.Text.RegularExpressions;

namespace RoadScript.Services;

/// <summary>
/// Renders a subset of Markdown to inline HTML for swim lane item descriptions.
/// Supports: **bold**, *italic*, ~~strikethrough~~, `code`, bullets/sub-bullets, URLs, headers
/// </summary>
public static class MarkdownRenderer
{
    private static readonly Regex UrlRegex = new(@"(https?://[^\s<]+)", RegexOptions.Compiled);

    /// <summary>
    /// Convert markdown text to sanitized HTML for rendering inside swim lane items.
    /// </summary>
    public static string RenderToHtml(string? text, string laneColor = "#4b5563")
    {
        if (string.IsNullOrWhiteSpace(text))
            return "";

        var lines = text.Split('\n');
        var sb = new StringBuilder();
        var bulletStack = new List<int>(); // track nesting levels
        bool inCodeBlock = false;
        var codeBlockLines = new List<string>();

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            // Code block toggle
            if (line.TrimStart().StartsWith("```"))
            {
                if (!inCodeBlock)
                {
                    CloseBulletLists(sb, bulletStack);
                    inCodeBlock = true;
                    codeBlockLines.Clear();
                }
                else
                {
                    inCodeBlock = false;
                    var codeContent = EscapeHtml(string.Join("\n", codeBlockLines));
                    sb.Append($"<pre style=\"background:#f3f4f6;border:1px solid #e5e7eb;border-radius:4px;padding:6px 8px;margin:4px 0;overflow-x:auto;\"><code style=\"font-size:13px;font-family:monospace;color:#374151;\">{codeContent}</code></pre>");
                    codeBlockLines.Clear();
                }
                continue;
            }

            if (inCodeBlock)
            {
                codeBlockLines.Add(line);
                continue;
            }

            // Headers
            var headerMatch = Regex.Match(line, @"^(#{1,6})\s+(.*)$");
            if (headerMatch.Success)
            {
                CloseBulletLists(sb, bulletStack);
                var level = headerMatch.Groups[1].Value.Length;
                var content = ApplyInlineFormatting(headerMatch.Groups[2].Value);
                var fontSize = level switch
                {
                    1 => "20px",
                    2 => "18px",
                    3 => "16px",
                    _ => "15px"
                };
                var fontWeight = "700";
                sb.Append($"<div style=\"font-size:{fontSize};font-weight:{fontWeight};color:#1f2937;margin:4px 0 2px 0;line-height:1.3;\">{content}</div>");
                continue;
            }

            // Bullet points: detect level by leading spaces
            var bulletMatch = Regex.Match(line, @"^(\s*)([-•*])\s+(.*)$");
            if (bulletMatch.Success)
            {
                var spaces = bulletMatch.Groups[1].Value.Length;
                var content = ApplyInlineFormatting(bulletMatch.Groups[3].Value.Trim());

                int level = 1;
                if (spaces >= 4) level = 3;
                else if (spaces >= 2) level = 2;

                // Close deeper levels (strictly greater to keep same-level siblings in one list)
                while (bulletStack.Count > 0 && bulletStack[^1] > level)
                {
                    bulletStack.RemoveAt(bulletStack.Count - 1);
                    sb.Append("</ul>");
                }

                // Open new levels as needed
                while (bulletStack.Count < level)
                {
                    var currentLevel = bulletStack.Count + 1;
                    var paddingLeft = currentLevel == 1 ? "14px" : currentLevel == 2 ? "26px" : "38px";
                    var bulletColor = currentLevel == 1 ? laneColor : "#9ca3af";
                    var listType = currentLevel == 1 ? "disc" : currentLevel == 2 ? "circle" : "square";
                    sb.Append($"<ul style=\"margin:2px 0;padding:0;padding-left:{paddingLeft};list-style-type:{listType};color:{bulletColor};\">");
                    bulletStack.Add(currentLevel);
                }

                var textColor = level == 1 ? "#4b5563" : "#6b7280";
                var textSize = level == 1 ? "18px" : "15px";
                sb.Append($"<li style=\"color:{textColor};font-size:{textSize};line-height:1.5;margin-bottom:2px;\"><span style=\"color:{textColor};\">{content}</span></li>");
                continue;
            }

            // Regular text paragraph
            CloseBulletLists(sb, bulletStack);

            var trimmed = line.Trim();
            if (!string.IsNullOrEmpty(trimmed))
            {
                sb.Append($"<p style=\"font-size:18px;color:#4b5563;line-height:1.5;margin:2px 0;\">{ApplyInlineFormatting(trimmed)}</p>");
            }
        }

        // Close any remaining open lists
        CloseBulletLists(sb, bulletStack);

        // Close unclosed code block
        if (inCodeBlock && codeBlockLines.Count > 0)
        {
            var codeContent = EscapeHtml(string.Join("\n", codeBlockLines));
            sb.Append($"<pre style=\"background:#f3f4f6;border:1px solid #e5e7eb;border-radius:4px;padding:6px 8px;margin:4px 0;overflow-x:auto;\"><code style=\"font-size:13px;font-family:monospace;color:#374151;\">{codeContent}</code></pre>");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Check if a description has any visible content (for hasDetails-style checks).
    /// </summary>
    public static bool HasContent(string? description)
    {
        return !string.IsNullOrWhiteSpace(description);
    }

    /// <summary>
    /// Check if description has sub-level content (nested bullets, multiple lines).
    /// Used for sizing calculations that used to check for Subs.
    /// </summary>
    public static bool HasSubContent(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return false;

        var lines = description.Split('\n');
        return lines.Any(l => Regex.IsMatch(l, @"^\s{2,}[-•*]\s"));
    }

    /// <summary>
    /// Migrate legacy Detail/Subs list to a markdown description string.
    /// </summary>
    public static string MigrateDetailsToMarkdown(List<RoadScript.Models.Detail>? details)
    {
        if (details == null || details.Count == 0)
            return "";

        var sb = new StringBuilder();
        for (int i = 0; i < details.Count; i++)
        {
            var detail = details[i];
            var hasText = !string.IsNullOrWhiteSpace(detail.Text);
            var hasSubs = detail.Subs != null && detail.Subs.Any(s => !string.IsNullOrWhiteSpace(s));

            if (hasText)
            {
                sb.Append($"- {detail.Text}");
                sb.Append('\n');
            }
            else if (hasSubs)
            {
                // Emit placeholder parent bullet so sub-bullets aren't orphaned
                sb.Append("- ...");
                sb.Append('\n');
            }

            if (detail.Subs != null)
            {
                foreach (var sub in detail.Subs)
                {
                    if (!string.IsNullOrWhiteSpace(sub))
                    {
                        sb.Append($"  - {sub}");
                        sb.Append('\n');
                    }
                }
            }
        }

        return sb.ToString().TrimEnd('\n');
    }

    private static void CloseBulletLists(StringBuilder sb, List<int> bulletStack)
    {
        while (bulletStack.Count > 0)
        {
            bulletStack.RemoveAt(bulletStack.Count - 1);
            sb.Append("</ul>");
        }
    }

    private static string ApplyInlineFormatting(string line)
    {
        // Escape HTML first to prevent injection
        line = EscapeHtml(line);

        // URLs: turn into clickable links
        line = UrlRegex.Replace(line, match =>
        {
            var url = match.Groups[1].Value;
            var display = url.Length > 50 ? url[..47] + "..." : url;
            return $"<a href=\"{url}\" target=\"_blank\" rel=\"noopener noreferrer\" style=\"color:#3b82f6;text-decoration:underline;\">{display}</a>";
        });

        // Bold: **text** or __text__
        line = Regex.Replace(line, @"\*\*(.+?)\*\*", "<strong style=\"font-weight:700;\">$1</strong>");
        line = Regex.Replace(line, @"__(.+?)__", "<strong style=\"font-weight:700;\">$1</strong>");

        // Strikethrough: ~~text~~
        line = Regex.Replace(line, @"~~(.+?)~~", "<s style=\"text-decoration:line-through;opacity:0.7;\">$1</s>");

        // Italic: *text* or _text_ (but not part of ** or __)
        line = Regex.Replace(line, @"(?<!\*)\*(?!\*)(.+?)(?<!\*)\*(?!\*)", "<em style=\"font-style:italic;\">$1</em>");
        line = Regex.Replace(line, @"(?<!_)_(?!_)(.+?)(?<!_)_(?!_)", "<em style=\"font-style:italic;\">$1</em>");

        // Inline code: `text`
        line = Regex.Replace(line, @"`([^`]+)`",
            "<code style=\"padding:1px 4px;background:#f3f4f6;color:#374151;border-radius:3px;font-size:0.9em;font-family:monospace;border:1px solid #e5e7eb;\">$1</code>");

        return line;
    }

    private static string EscapeHtml(string text)
    {
        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#039;");
    }
}
