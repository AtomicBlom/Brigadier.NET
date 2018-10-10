using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Brigadier.NET.ArgumentTypes;
using Brigadier.NET.Builder;
using Brigadier.NET.Context;
using Brigadier.NET.Exceptions;
using Brigadier.NET.Suggestion;
using Brigadier.NET.Util;

namespace Brigadier.NET.Tree
{
	public abstract class ArgumentCommandNode<TSource> : CommandNode<TSource> {
		protected ArgumentCommandNode(Command<TSource> command, Predicate<TSource> requirement, CommandNode<TSource> redirect, RedirectModifier<TSource> modifier, bool forks) 
			: base(command, requirement, redirect, modifier, forks)
		{
		}
	}

	public class ArgumentCommandNode<TSource, T> : ArgumentCommandNode<TSource>, IEquatable<ArgumentCommandNode<TSource, T>>
	{
		private const string UsageArgumentOpen = "<";
		private const string UsageArgumentClose = ">";

		private readonly string _name;

		public ArgumentCommandNode(string name, ArgumentType<T> type, Command<TSource> command, Predicate<TSource> requirement, CommandNode<TSource> redirect, RedirectModifier<TSource> modifier, bool forks, SuggestionProvider<TSource> customSuggestions) :
			base(command, requirement, redirect, modifier, forks)
		{
			_name = name;
			Type = type;
			CustomSuggestions = customSuggestions;
		}

		public ArgumentType<T> Type { get; }

		public override string Name => _name;

		public override string UsageText => $"{UsageArgumentOpen}{Name}{UsageArgumentClose}";

		public SuggestionProvider<TSource> CustomSuggestions { get; }

		/// <exception>CommandSyntaxException</exception>
		public override void Parse(StringReader reader, CommandContextBuilder<TSource> contextBuilder)
		{
			var start = reader.Cursor;
			var result = Type.Parse(reader);
			var parsed = new ParsedArgument<TSource, T>(start, reader.Cursor, result);

			contextBuilder.WithArgument(_name, parsed);
			contextBuilder.WithNode(this, parsed.Range);
		}

		public override Task<Suggestions> ListSuggestions(CommandContext<TSource> context, SuggestionsBuilder builder)
		{
			if (CustomSuggestions == null)
			{
				return Type.ListSuggestions(context, builder);
			}
			else
			{
				return CustomSuggestions(context, builder);
			}
		}

		public override IArgumentBuilder<TSource, CommandNode<TSource>> CreateBuilder()
		{
			var builder = RequiredArgumentBuilder<TSource, T>.RequiredArgument(_name, Type);
			builder.Requires(Requirement);
			builder.Forward(Redirect, RedirectModifier, IsFork);
			builder.Suggests(CustomSuggestions);
			if (Command != null)
			{
				builder.Executes(Command);
			}

			return builder;
		}

		protected override bool IsValidInput(string input)
		{
			try
			{
				var reader = new StringReader(input);
				Type.Parse(reader);
				return !reader.CanRead() || reader.Peek() == ' ';
			}
			catch (CommandSyntaxException)
			{
				return false;
			}
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is ArgumentCommandNode<TSource, T> other && Equals(other);
		}

		public bool Equals(ArgumentCommandNode<TSource, T> other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return string.Equals(_name, other._name) && Equals(Type, other.Type);
		}

		public override int GetHashCode()
		{
			return HashCode.Start
				.Hash(_name)
				.Hash(Type);
		}

		protected override string SortedKey => _name;

		public override IEnumerable<string> Examples => Type.Examples;

		public override string ToString()
		{
			return $"<argument {_name}:{Type}>";
		}
	}
}
