using Brigadier.NET.Exceptions;

namespace Brigadier.NET.Context
{
	[PublicAPI]
	public static class ContextChain
	{
		public static bool TryFlatten<TSource>(this CommandContext<TSource> rootContext, [NotNullWhen(true)] out ContextChain<TSource>? chain)
		{
			return ContextChain<TSource>.TryFlatten(rootContext, out chain);
		}

		public static int RunExecutable<TSource>(this CommandContext<TSource> executable, TSource source, ResultConsumer<TSource> resultConsumer, bool forkedMode)
		{
			return ContextChain<TSource>.RunExecutable(executable, source, resultConsumer, forkedMode);
		}

		public static IList<TSource> RunModifier<TSource>(this CommandContext<TSource> modifier, TSource source, ResultConsumer<TSource> resultConsumer, bool forkedMode)
		{
			return ContextChain<TSource>.RunModifier(modifier, source, resultConsumer, forkedMode);
		}
	}

	[PublicAPI]
	public class ContextChain<TSource>
	{
		// Ideally modifiers & executable could be separated by type, but both delegates expect full context
		private readonly List<CommandContext<TSource>> _modifiers;
		private readonly CommandContext<TSource> _executable;

		private ContextChain<TSource>? _nextStageCache;

		public ContextChain(List<CommandContext<TSource>> modifiers, CommandContext<TSource> executable)
		{
			if (executable.Command == null)
			{
				throw new ArgumentException("Last command in chain must be executable", nameof(executable));
			}
			_modifiers = modifiers;
			_executable = executable;
		}
		
		public static bool TryFlatten(CommandContext<TSource> rootContext, [NotNullWhen(true)] out ContextChain<TSource>? chain)
		{
			var modifiers = new List<CommandContext<TSource>>();
			var current = rootContext;
			while (true)
			{
				var child = current.Child;
				if (child == null)
				{
					// Last entry must be executable command
					if (current.Command == null)
					{
						chain = null;
						return false;
					}
					chain = new ContextChain<TSource>(modifiers, current);
					return true;
				}
				modifiers.Add(current);
				current = child;
			}
		}

		/// <summary>
		/// Run a modifier context and return the produced sources for the next stage.
		/// </summary>
		public static IList<TSource> RunModifier(CommandContext<TSource> modifier, TSource source, ResultConsumer<TSource> resultConsumer, bool forkedMode)
		{
			var sourceModifier = modifier.RedirectModifier;

			// Note: source currently in context is irrelevant at this point, since we might have updated it earlier
			if (sourceModifier == null)
			{
				// Simple redirect, just propagate source to next node
				return new List<TSource> { source };
			}

			var contextToUse = modifier.CopyFor(source);
			try
			{
				return sourceModifier(contextToUse);
			}
			catch (CommandSyntaxException)
			{
				resultConsumer(contextToUse, false, 0);
				if (forkedMode)
				{
					return Array.Empty<TSource>();
				}
				throw;
			}
		}

		/// <summary>
		/// Execute the final (executable) context for a given source.
		/// </summary>
		public static int RunExecutable(CommandContext<TSource> executable, TSource source, ResultConsumer<TSource> resultConsumer, bool forkedMode)
		{
			var contextToUse = executable.CopyFor(source);
			try
			{
				var result = executable.Command!(contextToUse);
				resultConsumer(contextToUse, true, result);
				return forkedMode ? 1 : result;
			}
			catch (CommandSyntaxException)
			{
				resultConsumer(contextToUse, false, 0);
				if (forkedMode)
				{
					return 0;
				}
				throw;
			}
		}

		/// <summary>
		/// Execute the entire chain for the initial source.
		/// </summary>
		public int ExecuteAll(TSource source, ResultConsumer<TSource> resultConsumer)
		{
			if (_modifiers.Count == 0)
			{
				// Fast path – only executable stage
				return RunExecutable(_executable, source, resultConsumer, forkedMode: false);
			}

			var forkedMode = false;
			IList<TSource> currentSources = new List<TSource> { source };

			foreach (var modifier in _modifiers)
			{
				forkedMode |= modifier.IsForked();

				var nextSources = new List<TSource>();
				foreach (var s in currentSources)
				{
					var produced = RunModifier(modifier, s, resultConsumer, forkedMode);
					if (produced.Count > 0)
					{
						nextSources.AddRange(produced);
					}
				}
				if (nextSources.Count == 0)
				{
					return 0;
				}
				currentSources = nextSources;
			}

			var total = 0;
			foreach (var execSource in currentSources)
			{
				total += RunExecutable(_executable, execSource, resultConsumer, forkedMode);
			}
			return total;
		}

		public Stage CurrentStage => _modifiers.Count == 0 ? Stage.Execute : Stage.Modify;

		public CommandContext<TSource> TopContext => _modifiers.Count == 0 ? _executable : _modifiers[0];

		public ContextChain<TSource>? NextStage()
		{
			var modifierCount = _modifiers.Count;
			if (modifierCount == 0)
			{
				return null;
			}

			return _nextStageCache ??= new ContextChain<TSource>(_modifiers.GetRange(1, modifierCount - 1), _executable);
		}

		public enum Stage
		{
			Modify,
			Execute
		}
	}
}
