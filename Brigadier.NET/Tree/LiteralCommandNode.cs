using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Brigadier.NET.Builder;
using Brigadier.NET.Context;
using Brigadier.NET.Exceptions;
using Brigadier.NET.Suggestion;
using Brigadier.NET.Util;

namespace Brigadier.NET.Tree
{
	public class LiteralCommandNode<TSource> : CommandNode<TSource>, IEquatable<LiteralCommandNode<TSource>>
	{
		public LiteralCommandNode(string literal, Command< TSource > command, Predicate<TSource> requirement, CommandNode<TSource> redirect, RedirectModifier<TSource> modifier, bool forks)
			: base(command, requirement, redirect, modifier, forks)
		{
			Literal = literal;
            LiteralLowerCase = literal.ToLowerInvariant();
        }

        public string Literal { get; }

        public string LiteralLowerCase { get; }

		public override string Name => Literal;

		public override void Parse(StringReader reader, CommandContextBuilder<TSource> contextBuilder)
		{
			var start = reader.Cursor;
			var end = Parse(reader);

			if (end > -1)
			{
				contextBuilder.WithNode(this, StringRange.Between(start, end));
				return;
			}

			throw CommandSyntaxException.BuiltInExceptions.LiteralIncorrect().CreateWithContext(reader, Literal);
		}

		private int Parse(StringReader reader)
		{
			var start = reader.Cursor;
			if (reader.CanRead(Literal.Length))
			{
				var end = start + Literal.Length;
				if (reader.String.Substring(start, end - start).Equals(Literal))
				{
					reader.Cursor = end;
					if (!reader.CanRead() || reader.Peek() == ' ')
					{
						return end;
					}
					else
					{
						reader.Cursor = start;
					}
				}
			}
			return -1;
		}

		public override Task<Suggestions> ListSuggestions(CommandContext<TSource> context, SuggestionsBuilder builder)
		{
			if (Literal.ToLower().StartsWith(builder.RemainingLowerCase))
			{
				return builder.Suggest(Literal).BuildFuture();
			}
			else
			{
				return Suggestions.Empty();
			}
		}

		protected override bool IsValidInput(string input)
		{
			return Parse(new StringReader(input)) > -1;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is LiteralCommandNode<TSource> other && Equals(other);
		}

		public bool Equals(LiteralCommandNode<TSource> other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return string.Equals(Literal, other.Literal);
		}

		public override int GetHashCode()
		{
			return HashCode.Start
				.Hash(Literal);
		}

		public override string UsageText => Literal;

		public override IArgumentBuilder<TSource, CommandNode<TSource>> CreateBuilder()
		{
			var builder = LiteralArgumentBuilder<TSource>.LiteralArgument(Literal);
			builder.Requires(Requirement);
			builder.Forward(Redirect, RedirectModifier, IsFork);
			if (Command != null)
			{
				builder.Executes(Command);
			}

			return builder;
		}

		protected override string SortedKey => Literal;

		public override IEnumerable<string> Examples => new [] { Literal};
	}
}
