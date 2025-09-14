using Brigadier.NET.Exceptions;

namespace Brigadier.NET.ArgumentTypes
{
	[PublicAPI]
	public class StringArgumentType : IArgumentType<string> {
		internal StringArgumentType(StringArgType type)
		{
			Type = type;
		}

		public StringArgType Type { get; }

		/// <exception cref="CommandSyntaxException" />
		public string Parse(IStringReader reader)
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

		public IEnumerable<string> Examples => StringExamples[Type];
		
		public override string ToString()
		{
			return "string()";
		}

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
			{ StringArgType.SingleWord, ["word", "words_with_underscores"] },
			{ StringArgType.QuotablePhrase, ["\"quoted phrase\"", "word", "\"\""] },
			{ StringArgType.GreedyPhrase, ["word", "words with spaces", "\"and symbols\""] }
		};
	}
}
