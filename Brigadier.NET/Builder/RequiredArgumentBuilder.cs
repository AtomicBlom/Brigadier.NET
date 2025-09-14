using Brigadier.NET.ArgumentTypes;
using Brigadier.NET.Suggestion;
using Brigadier.NET.Tree;

namespace Brigadier.NET.Builder;

[PublicAPI]
public static class RequiredArgumentBuilderExtensions
{
	// ReSharper disable once UnusedParameter.Global
	// context is used to infer generic parameters in API
	public static RequiredArgumentBuilder<TSource, T> Argument<TSource, T>(this IArgumentContext<TSource> context, string name, IArgumentType<T> type) where T : notnull
	{
		return RequiredArgumentBuilder<TSource, T>.RequiredArgument(name, type);
	}
}

[PublicAPI]
public class RequiredArgumentBuilder<TSource, T> : ArgumentBuilder<TSource, RequiredArgumentBuilder<TSource, T>, ArgumentCommandNode<TSource, T>> where T : notnull
{
	private SuggestionProvider<TSource>? _suggestionsProvider;

	private RequiredArgumentBuilder(string name, IArgumentType<T> type)
	{
		Name = name;
		Type = type;
	}

	public static RequiredArgumentBuilder<TSource, T> RequiredArgument(string name, IArgumentType<T> type)
	{
		return new RequiredArgumentBuilder<TSource, T>(name, type);
	}

	public RequiredArgumentBuilder<TSource, T> Suggests(SuggestionProvider<TSource>? provider)
	{
		_suggestionsProvider = provider;
		return This;
	}

	public IArgumentType<T> Type { get; }

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