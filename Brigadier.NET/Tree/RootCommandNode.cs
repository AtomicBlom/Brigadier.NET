using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Brigadier.NET.Builder;
using Brigadier.NET.Context;
using Brigadier.NET.Suggestion;
using Brigadier.NET.Util;

namespace Brigadier.NET.Tree
{
	public class RootCommandNode<TSource> : CommandNode<TSource>, IEquatable<RootCommandNode<TSource>>
	{
		public RootCommandNode() : base(null, (c) => true, null, s => new [] { s.Source }, false)
		{
			
		}

		public override string Name => string.Empty;

		public override string UsageText => string.Empty;

		public override void Parse(StringReader reader, CommandContextBuilder<TSource> contextBuilder)
		{
		}

		public override Task<Suggestions> ListSuggestions(CommandContext<TSource> context, SuggestionsBuilder builder)
		{
			return Suggestions.Empty();
		}

		protected override bool IsValidInput(string input)
		{
			return false;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is RootCommandNode<TSource> other && Equals(other);
		}

		public bool Equals(RootCommandNode<TSource> other)
		{
			return true;
		}

		public override int GetHashCode()
		{
			return HashCode.Start;
		}

		public override IArgumentBuilder<TSource, CommandNode<TSource>> CreateBuilder()
		{
			throw new InvalidOperationException("Cannot convert root into a builder");
		}

		protected override string SortedKey => string.Empty;

		public override IEnumerable<string> Examples => new string[0];

		public override string ToString() => "<root>";
	}
}
