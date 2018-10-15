using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Brigadier.NET.Context;

namespace Brigadier.NET.Suggestion
{
	public class SuggestionsBuilder
	{
		private readonly List<Suggestion> _result = new List<Suggestion>();

		public SuggestionsBuilder(string input, int start)
		{
			Input = input;
			Start = start;
			Remaining = input.Substring(start);
		}

		public string Input { get; }

		public int Start { get; }

		public string Remaining { get; }

		public Suggestions Build()
		{
			return Suggestions.Create(Input, _result.ToArray());
		}

		[Obsolete("Accidentally ported Java concept, prefer Build")]
		public Task<Suggestions> BuildFuture()
		{
			return Task.FromResult(Build());
		}

		public SuggestionsBuilder Suggest(string text)
		{
			if (text.Equals(Remaining))
			{
				return this;
			}
			_result.Add(new Suggestion(StringRange.Between(Start, Input.Length), text));
			return this;
		}

		public SuggestionsBuilder Suggest(string text, IMessage tooltip)
		{
			if (text.Equals(Remaining))
			{
				return this;
			}
			_result.Add(new Suggestion(StringRange.Between(Start, Input.Length), text, tooltip));
			return this;
		}

		public SuggestionsBuilder Suggest(int value)
		{
			_result.Add(new IntegerSuggestion(StringRange.Between(Start, Input.Length), value));
			return this;
		}

		public SuggestionsBuilder Suggest(int value, IMessage tooltip)
		{
			_result.Add(new IntegerSuggestion(StringRange.Between(Start, Input.Length), value, tooltip));
			return this;
		}

		public SuggestionsBuilder Add(SuggestionsBuilder other)
		{
			_result.AddRange(other._result);
			return this;
		}

		public SuggestionsBuilder CreateOffset(int start)
		{
			return new SuggestionsBuilder(Input, start);
		}

		public SuggestionsBuilder Restart()
		{
			return new SuggestionsBuilder(Input, Start);
		}
	}
}
