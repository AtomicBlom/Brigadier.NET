using Brigadier.NET.ArgumentTypes;
using Brigadier.NET.Suggestion;
using Brigadier.NET.Tree;

namespace Brigadier.NET.Builder
{
	public static class RequiredArgumentBuilderExtensions
	{
		// ReSharper disable once UnusedParameter.Global
		// context is used to infer generic parameters in API
		public static RequiredArgumentBuilder<TSource, T> Argument<TSource, T>(this IArgumentContext<TSource> context, string name, ArgumentType<T> type)
		{
			return RequiredArgumentBuilder<TSource, T>.RequiredArgument(name, type);
		}
	}

	public class RequiredArgumentBuilder<TSource, T> : ArgumentBuilder<TSource, RequiredArgumentBuilder<TSource, T>, ArgumentCommandNode<TSource, T>>
	{
		private SuggestionProvider<TSource> _suggestionsProvider;

		private RequiredArgumentBuilder(string name, ArgumentType<T> type)
		{
			Name = name;
			Type = type;
		}

		public static RequiredArgumentBuilder<TSource, T> RequiredArgument(string name, ArgumentType<T> type)
		{
			return new RequiredArgumentBuilder<TSource, T>(name, type);
		}

		public RequiredArgumentBuilder<TSource, T> Suggests(SuggestionProvider<TSource> provider)
		{
			_suggestionsProvider = provider;
			return This;
		}

		public ArgumentType<T> Type { get; }

		public string Name { get; }

		public override ArgumentCommandNode<TSource, T> Build()
		{
			var result = new ArgumentCommandNode<TSource, T>(Name, Type, Command, Requirement, RedirectTarget, RedirectModifier, IsFork, _suggestionsProvider);

			foreach (var argument in Arguments)
			{
				result.AddChild(argument);
			}

			return result;
		}

	}
}
