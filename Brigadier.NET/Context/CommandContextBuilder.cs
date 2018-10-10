using System;
using System.Collections.Generic;
using System.Linq;
using Brigadier.NET.Tree;

namespace Brigadier.NET.Context
{
	public class CommandContextBuilder<TSource>
	{
		private readonly IDictionary<string, IParsedArgument> _arguments;
		private RedirectModifier<TSource> _modifier;
		private bool _forks;

		public CommandContextBuilder(CommandDispatcher<TSource> dispatcher, TSource source, CommandNode<TSource> rootNode, int start)
		{
			RootNode = rootNode;
			Dispatcher = dispatcher;
			Source = source;
			Range = StringRange.At(start);
			_arguments = new Dictionary<string, IParsedArgument>();
			Nodes = new List<ParsedCommandNode<TSource>>();
		}

		public CommandContextBuilder(CommandDispatcher<TSource> dispatcher, TSource source, CommandNode<TSource> rootNode, StringRange range, IDictionary<string, IParsedArgument> arguments, List<ParsedCommandNode<TSource>> nodes)
		{
			Dispatcher = dispatcher;
			Source = source;
			RootNode = rootNode;
			Range = range;
			_arguments = new Dictionary<string, IParsedArgument>(arguments);
			Nodes = new List<ParsedCommandNode<TSource>>(nodes);
		}

		public CommandContextBuilder<TSource> WithSource(TSource source)
		{
			Source = source;
			return this;
		}

		public TSource Source { get; private set; }

		public CommandNode<TSource> RootNode { get; }

		public CommandContextBuilder<TSource> WithArgument(string name, IParsedArgument argument)
		{
			_arguments.Add(name, argument);
			return this;
		}

		public IDictionary<string, IParsedArgument> GetArguments()
		{
			return _arguments;
		}

		public CommandContextBuilder<TSource> WithCommand(Command<TSource> command)
		{
			Command = command;
			return this;
		}

		public CommandContextBuilder<TSource> WithNode(CommandNode<TSource> node, StringRange range)
		{
			Nodes.Add(new ParsedCommandNode<TSource>(node, range));
			Range = StringRange.Encompassing(Range, range);
			_modifier = node.RedirectModifier;
			_forks = node.IsFork;
			return this;
		}

		public CommandContextBuilder<TSource> Copy()
		{
			var copy = new CommandContextBuilder<TSource>(Dispatcher, Source, RootNode, Range, _arguments, Nodes)
			{
				Command = Command,
				Child = Child,
				_forks = _forks
			};
			
			return copy;
		}

		public CommandContextBuilder<TSource> WithChild(CommandContextBuilder<TSource> child)
		{
			Child = child;
			return this;
		}

		public CommandContextBuilder<TSource> Child { get; private set; }

		public CommandContextBuilder<TSource> LastChild
		{
			get
			{
				var result = this;
				while (result.Child != null)
				{
					result = result.Child;
				}
				return result;
			}
		}
		

		public Command<TSource> Command { get; private set; }

		public List<ParsedCommandNode<TSource>> Nodes { get; }

		public CommandContext<TSource> Build(string input)
		{
			return new CommandContext<TSource>(Source, input, _arguments, Command, RootNode, Nodes, Range, Child?.Build(input), _modifier, _forks);
		}

		public CommandDispatcher<TSource> Dispatcher { get; }

		public StringRange Range { get; private set; }

		public SuggestionContext<TSource> FindSuggestionContext(int cursor)
		{
			if (Range.Start <= cursor)
			{
				if (Range.End < cursor)
				{
					if (Child != null)
					{
						return Child.FindSuggestionContext(cursor);
					}
					else if (Nodes.Count > 0)
					{
						var last = Nodes[Nodes.Count - 1];
						return new SuggestionContext<TSource>(last.Node, last.Range.End + 1);
					}
					else
					{
						return new SuggestionContext<TSource>(RootNode, Range.Start);
					}
				}
				else
				{
					var prev = RootNode;
					foreach (var node in Nodes)
					{
						var nodeRange = node.Range;
						if (nodeRange.Start <= cursor && cursor <= nodeRange.End)
						{
							return new SuggestionContext<TSource>(prev, nodeRange.Start);
						}
						prev = node.Node;
					}
					if (prev == null)
					{
						throw new InvalidOperationException("Can't find node before cursor");
					}
					return new SuggestionContext<TSource>(prev, Range.Start);
				}
			}
			throw new InvalidOperationException("Can't find node before cursor");
		}
	}

}
