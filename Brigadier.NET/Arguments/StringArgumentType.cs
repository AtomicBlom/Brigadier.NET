using System.Collections.Generic;
using System.Text;
using Brigadier.NET.Context;
using Brigadier.NET.Exceptions;

namespace Brigadier.NET.Arguments
{
	public class StringArgumentType : ArgumentType<string> {
		private StringArgumentType(StringType type)
		{
			Type = type;
		}

		public static StringArgumentType Word()
		{
			return new StringArgumentType(StringType.SingleWord);
		}

		public static StringArgumentType Phrase()
		{
	        return new StringArgumentType(StringType.QuotablePhrase);
		}	

		public static StringArgumentType GreedyString()
		{
			return new StringArgumentType(StringType.GreedyPhrase);
		}

		public static string GetString<TSource>(CommandContext<TSource> context, string name)
		{
			return context.GetArgument<string>(name);
	    }

		public StringType Type { get; }

		/// <exception cref="CommandSyntaxException" />
		public override string Parse(IStringReader reader)
		{
	        if (Type == StringType.GreedyPhrase) {
				var text = reader.Remaining;
				reader.Cursor = reader.TotalLength;
				return text;
			} else if (Type == StringType.SingleWord) {
				return reader.ReadUnquotedString();
			} else {
				return reader.ReadString();
			}
		}

		public override string ToString()
		{
			return "string()";
		}

		public override IEnumerable<string> Examples => StringExamples[Type];

		public static string EscapeIfRequired(string input)
		{
			foreach (var c in input)
			{
				if (!StringReader.IsAllowedInUnquotedString(c))
				{
					return Escape(input);
				}
			}
			return input;
		}

		private static string Escape(string input)
		{
			var result = new StringBuilder("\"");

			for (var i = 0; i < input.Length; i++)
			{
				var c = input[i];
				if (c == '\\' || c == '"')
				{
					result.Append('\\');
				}
				result.Append(c);
			}

			result.Append("\"");
			return result.ToString();
		}

		public enum StringType
		{
			SingleWord,
			QuotablePhrase,
			GreedyPhrase
		}

		private static readonly Dictionary<StringType, IEnumerable<string>> StringExamples = new Dictionary<StringType, IEnumerable<string>>
		{
			{ StringType.SingleWord, new [] { "word", "words_with_underscores" } },
			{ StringType.QuotablePhrase, new [] {"\"quoted phrase\"", "word", "\"\""}},
			{ StringType.GreedyPhrase, new [] {"word", "words with spaces", "\"and symbols\""}}
		};
	}
}
