using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brigadier.NET.Context;
using Brigadier.NET.Util;

namespace Brigadier.NET.Suggestion
{
	public class Suggestions : IEquatable<Suggestions>
	{
		private static readonly Suggestions NoSuggestions = new Suggestions(StringRange.At(0), new List<Suggestion>());

		public Suggestions(StringRange range, List<Suggestion> suggestions)
		{
			Range = range;
			List = suggestions;
		}

		public StringRange Range { get; }

		public List<Suggestion> List { get; }

		public bool IsEmpty()
		{
			return List.Count == 0;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is Suggestions other 
			       && Equals(other);
		}

		public bool Equals(Suggestions other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(Range, other.Range) 
			       && Equals(List, other.List);
		}

		public override int GetHashCode()
		{
			return HashCode.Start
				.Hash(Range)
				.Hash(List);
		}

		public override string ToString()
		{
			return $"Suggestions{{range={Range}, suggestions={List}}}";
		}

		public static Task<Suggestions> Empty()
		{
			return Task.FromResult(NoSuggestions);
		}

		public static Suggestions Merge(string command, ICollection<Suggestions> input)
		{
			if (input.Count == 0)
			{
				return NoSuggestions;
			}
			else if (input.Count == 1)
			{
				return input.Single();
			}

			ISet<Suggestion> texts = new HashSet<Suggestion>();
			foreach (var suggestions in input)
			{
				foreach (var suggestion in suggestions.List)
				{
					texts.Add(suggestion);
				}
			}
			return Create(command, texts.ToArray());
		}

		public static Suggestions Create(string command, Suggestion[] suggestions)
		{
			if (suggestions.Length == 0)
			{
				return NoSuggestions;
			}
			var start = int.MaxValue;
			var end = int.MinValue;
			foreach (var suggestion in suggestions)
			{
				start = Math.Min(suggestion.Range.Start, start);
				end = Math.Max(suggestion.Range.End, end);
			}
			var range = new StringRange(start, end);
			var texts = new HashSet<Suggestion>();
			foreach (var suggestion in suggestions)
			{
				texts.Add(suggestion.Expand(command, range));
			}
			var sorted = new List<Suggestion>(texts);
			sorted.Sort((a, b) => a.CompareToIgnoreCase(b));
			return new Suggestions(range, sorted);
		}
	}
}
