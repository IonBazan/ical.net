//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Ical.Net.Serialization;

namespace Ical.Net.Utility;

internal static class TextUtil
{
    /// <summary>
    /// Content line length according to RFC https://tools.ietf.org/html/rfc5545#section-3.1
    /// </summary>
    public const int LineLength = 75;

    /// <summary>
    /// Folds lines to 75 octets. It appends a CrLf, and prepends the next line with a space.
    /// per RFC https://tools.ietf.org/html/rfc5545#section-3.1
    /// </summary>
    public static void FoldLines(this StringBuilder result, string inputLine)
    {
        var utf8 = Encoding.UTF8;
        if (utf8.GetByteCount(inputLine) <= LineLength)
        {
            result.Append(inputLine);
            result.Append(SerializationConstants.LineBreak);
            return;
        }

        var lineArray = new char[LineLength]; // 75 characters
        var charIndex = 0;
        var byteIndex = 0;

        foreach (var ch in inputLine)
        {
            var chBytes = utf8.GetByteCount([ch]);

            // if current char couldn't be placed on current line
            // without a break, write it on a new one
            if (byteIndex + chBytes > LineLength)
            {
                result.Append(lineArray, 0, charIndex);
                result.Append(SerializationConstants.LineBreak);

                charIndex = 0;
                lineArray[charIndex++] = ' ';
                byteIndex = 1;
            }

            lineArray[charIndex++] = ch;
            byteIndex += chBytes;
        }

        // Append the remaining characters (if any, i.e. charIndex > 0) to the result
        result.Append(lineArray, 0, charIndex);

        result.Append(SerializationConstants.LineBreak);
    }

    /// <summary>
    /// Folds the line that was appended to <paramref name="result"/> starting at <paramref name="start"/>.
    /// </summary>
    public static void FoldLines(this StringBuilder result, int start)
    {
        if (IsShortAsciiLine(result, start))
        {
            result.Append(SerializationConstants.LineBreak);
            return;
        }

        var line = result.ToString(start, result.Length - start);
        result.Length = start;
        result.FoldLines(line);
    }

    private static bool IsShortAsciiLine(StringBuilder sb, int start)
    {
        if (sb.Length - start > LineLength)
        {
            return false;
        }

        for (var i = start; i < sb.Length; i++)
        {
            if (sb[i] > 0x7F)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Writes <paramref name="value"/> as a zero-padded number of <paramref name="digits"/> digits at <paramref name="offset"/>.
    /// </summary>
    public static void WriteDigits(Span<char> buffer, int offset, int value, int digits)
    {
        for (var i = offset + digits - 1; i >= offset; i--)
        {
            buffer[i] = (char) ('0' + value % 10);
            value /= 10;
        }
    }

    public static IEnumerable<string> Chunk(string str, int chunkSize = 73)
    {
        for (var index = 0; index < str.Length; index += chunkSize)
        {
            yield return str.Substring(index, Math.Min(chunkSize, str.Length - index));
        }
    }

    /// <summary> Removes blank lines from a string with normalized (\r\n) line endings </summary>
    public static string RemoveEmptyLines(string s)
    {
        var len = -1;
        while (len != s.Length)
        {
            s = s.Replace("\r\n\r\n", SerializationConstants.LineBreak);
            len = s.Length;
        }
        return s;
    }

    internal static readonly Regex NormalizeToCrLf = new Regex(@"((\r(?=[^\n]))|((?<=[^\r])\n))", RegexOptions.Compiled, RegexDefaults.Timeout);

    /// <summary>
    /// Normalizes line endings, converting "\r" into "\r\n" and "\n" into "\r\n".
    /// </summary>
    public static TextReader Normalize(string s, SerializationContext ctx)
    {
        // Replace \r and \n with \r\n.
        s = NormalizeToCrLf.Replace(s, SerializationConstants.LineBreak);

        s = RemoveEmptyLines(UnwrapLines(s));

        return new StringReader(s);
    }

    internal static readonly Regex NewLineMatch = new Regex(@"(\r\n[ \t])", RegexOptions.Compiled, RegexDefaults.Timeout);

    /// <summary> Unwraps lines from the RFC 5545 "line folding" technique. </summary>
    public static string UnwrapLines(string s) => NewLineMatch.Replace(s, string.Empty);

    public static bool Contains(this string haystack, string needle, StringComparison stringComparison)
        => haystack.IndexOf(needle, stringComparison) >= 0;
}
