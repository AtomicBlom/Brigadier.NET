using System;
using System.Globalization;
using System.Text;
using Brigadier.NET.Exceptions;

namespace Brigadier.NET
{
	[PublicAPI]
	public class StringReader : IStringReader
    {
        private static readonly char SyntaxEscape = '\\';
        private static readonly char SyntaxDoubleQuote = '"';
        private static readonly char SyntaxSingleQuote = '\'';

        public StringReader(StringReader other)
        {
            String = other.String;
            Cursor = other.Cursor;
        }

        public StringReader(string input)
        {
            String = input;
        }


        public string String { get; }

        public int Cursor { get; set; }

        public int RemainingLength => String.Length - Cursor;


        public int TotalLength => String.Length;


        public string Read => String.Substring(0, Cursor);


        public string Remaining => String.Substring(Cursor);


        public bool CanRead(int length) => Cursor + length <= String.Length;


        public bool CanRead() => CanRead(1);


        public char Peek()
        {
            return String[Cursor];
        }


        public char Peek(int offset)
        {
            return String[Cursor + offset];
        }

        public char Next()
        {
            return String[Cursor++];
        }

        public void Skip()
        {
            Cursor++;
        }

        private static bool IsAllowedNumber(char c)
        {
            return c >= '0' && c <= '9' || c == '.' || c == '-';
        }

        private static bool IsQuotedStringStart(char c)
        {
            return c == SyntaxDoubleQuote || c == SyntaxSingleQuote;
        }

        public void SkipWhitespace()
        {
            while (CanRead() && char.IsWhiteSpace(Peek()))
            {
                Skip();
            }
        }

        /// <exception cref="CommandSyntaxException" />
        public int ReadInt()
        {
            var start = Cursor;
            while (CanRead() && IsAllowedNumber(Peek()))
            {
                Skip();
            }

            var span = String.AsSpan(start, Cursor - start);
            if (span.Length == 0)
            {
                throw CommandSyntaxException.BuiltInExceptions.ReaderExpectedInt().CreateWithContext(this);
            }

            try
            {
                return int.Parse(span, NumberStyles.Integer, CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                Cursor = start;
                throw CommandSyntaxException.BuiltInExceptions.ReaderInvalidInt().CreateWithContext(this, span.ToString());
            }
        }

        /// <exception cref="CommandSyntaxException" />
        public long ReadLong()
        {
            var start = Cursor;
            while (CanRead() && IsAllowedNumber(Peek()))
            {
                Skip();
            }

            var span = String.AsSpan(start, Cursor - start);
            if (span.Length == 0)
            {
                throw CommandSyntaxException.BuiltInExceptions.ReaderExpectedLong().CreateWithContext(this);
            }

            try
            {
                return long.Parse(span, NumberStyles.Integer, CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                Cursor = start;
                throw CommandSyntaxException.BuiltInExceptions.ReaderInvalidLong().CreateWithContext(this, span.ToString());
            }
        }

        /// <exception cref="CommandSyntaxException" />
        public double ReadDouble()
        {
            var start = Cursor;
            while (CanRead() && IsAllowedNumber(Peek()))
            {
                Skip();
            }

            var span = String.AsSpan(start, Cursor - start);
            if (span.Length == 0)
            {
                throw CommandSyntaxException.BuiltInExceptions.ReaderExpectedDouble().CreateWithContext(this);
            }

            try
            {
                return double.Parse(span, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                Cursor = start;
                throw CommandSyntaxException.BuiltInExceptions.ReaderInvalidDouble().CreateWithContext(this, span.ToString());
            }
        }

        /// <exception cref="CommandSyntaxException" />
        public float ReadFloat()
        {
            var start = Cursor;
            while (CanRead() && IsAllowedNumber(Peek()))
            {
                Skip();
            }

            var span = String.AsSpan(start, Cursor - start);
            if (span.Length == 0)
            {
                throw CommandSyntaxException.BuiltInExceptions.ReaderExpectedFloat().CreateWithContext(this);
            }

            try
            {
                return float.Parse(span, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                Cursor = start;
                throw CommandSyntaxException.BuiltInExceptions.ReaderInvalidFloat().CreateWithContext(this, span.ToString());
            }
        }

        public static bool IsAllowedInUnquotedString(char c)
        {
            return c >= '0' && c <= '9'
                   || c >= 'A' && c <= 'Z'
                   || c >= 'a' && c <= 'z'
                   || c == '_' || c == '-'
                   || c == '.' || c == '+';
        }

        public string ReadUnquotedString()
        {
            var start = Cursor;
            while (CanRead() && IsAllowedInUnquotedString(Peek()))
            {
                Skip();
            }

            var span = String.AsSpan(start, Cursor - start);
            return span.ToString();
        }

        /// <exception cref="CommandSyntaxException" />
        public string ReadQuotedString()
        {
            if (!CanRead())
            {
                return "";
            }
            var next = Peek();
            if (!IsQuotedStringStart(next))
            {
                throw CommandSyntaxException.BuiltInExceptions.ReaderExpectedStartOfQuote().CreateWithContext(this);
            }

            Skip();
            return ReadStringUntil(next);
        }

        private string ReadStringUntil(char terminator)
        {
            int start = Cursor;
            bool escaped = false;
            while (CanRead())
            {
                char c = Next();
                if (escaped)
                {
                    if (c == terminator || c == SyntaxEscape)
                    {
                        escaped = false;
                    }
                    else
                    {
                        Cursor--;
                        throw CommandSyntaxException.BuiltInExceptions.ReaderInvalidEscape().CreateWithContext(this, c.ToString());
                    }
                }
                else if (c == SyntaxEscape)
                {
                    escaped = true;
                }
                else if (c == terminator)
                {
                    // Return the substring between start and the character before the terminator
                    return String.Substring(start, Cursor - start - 1);
                }
            }
            throw CommandSyntaxException.BuiltInExceptions.ReaderExpectedEndOfQuote().CreateWithContext(this);
        }

        /// <exception cref="CommandSyntaxException" />
        public string ReadString()
        {
            if (!CanRead())
            {
                return "";
            }
            var next = Peek();
            if (IsQuotedStringStart(next))
            {
                Skip();
                return ReadStringUntil(next);
            }
            return ReadUnquotedString();
        }

        /// <exception cref="CommandSyntaxException" />
        public bool ReadBoolean()
        {
            var start = Cursor;
            var value = ReadString();
            if (value.Length == 0)
            {
                throw CommandSyntaxException.BuiltInExceptions.ReaderExpectedBool().CreateWithContext(this);
            }

            if (value.Equals("true"))
            {
                return true;
            }
            else if (value.Equals("false"))
            {
                return false;
            }
            else
            {
                Cursor = start;
                throw CommandSyntaxException.BuiltInExceptions.ReaderInvalidBool().CreateWithContext(this, value);
            }
        }

        /// <exception cref="CommandSyntaxException" />
        public void Expect(char c)
        {
            if (!CanRead() || Peek() != c)
            {
                throw CommandSyntaxException.BuiltInExceptions.ReaderExpectedSymbol().CreateWithContext(this, c.ToString());
            }

            Skip();
        }
    }
}