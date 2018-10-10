using System.Collections.Generic;
using System.Text;
using Brigadier.NET.Context;
using Brigadier.NET.Exceptions;

namespace Brigadier.NET.ArgumentTypes
{
	public class StringArgumentType : ArgumentType<string> {
		internal StringArgumentType(StringArgType type)
		{
			Type = type;
		}

		public StringArgType Type { get; }

		/// <exception cref="CommandSyntaxException" />
		public override string Parse(IStringReader reader)
		{
	        if (Type == StringArgType.GreedyPhrase) {
				var text = reader.Remaining;
				reader.Cursor = reader.TotalLength;
				return text;
			} else if (Type == StringArgType.SingleWord) {
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

		private static readonly Dictionary<StringArgType, IEnumerable<string>> StringExamples = new Dictionary<StringArgType, IEnumerable<string>>
		{
			{ StringArgType.SingleWord, new [] { "word", "words_with_underscores" } },
			{ StringArgType.QuotablePhrase, new [] {"\"quoted phrase\"", "word", "\"\""}},
			{ StringArgType.GreedyPhrase, new [] {"word", "words with spaces", "\"and symbols\""}}
		};
	}
}
