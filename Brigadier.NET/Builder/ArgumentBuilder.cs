using System;
using System.Collections.Generic;
using System.Linq;
using Brigadier.NET.Tree;

namespace Brigadier.NET.Builder
{
	public interface IArgumentBuilder<TSource, out TNode>
		where TNode : CommandNode<TSource>
	{
		TNode Build();
	}

	public abstract class ArgumentBuilder<TSource, TThis, TNode> : IArgumentBuilder<TSource, TNode> 
		where TThis : ArgumentBuilder<TSource, TThis, TNode> 
		where TNode : CommandNode<TSource>
	{
		private readonly RootCommandNode<TSource> _arguments = new RootCommandNode<TSource>();

		protected TThis This => (TThis)this;

		public TThis Then<TChildNode>(IArgumentBuilder<TSource, TChildNode> argument) where TChildNode : CommandNode<TSource>
		{
			if (RedirectTarget != null)
			{
				throw new InvalidOperationException("Cannot add children to a redirected node");
			}
			_arguments.AddChild(argument.Build());
			return This;
		}

		public TThis Then<TChildNode>(Func<IArgumentContext<TSource>, IArgumentBuilder<TSource, TChildNode>> argument) where TChildNode : CommandNode<TSource>
		{
			if (RedirectTarget != null)
			{
				throw new InvalidOperationException("Cannot add children to a redirected node");
			}
			_arguments.AddChild(argument(default(ArgumentContext<TSource>)).Build());
			return This;
		}

		public TThis Then(CommandNode<TSource> argument)
		{
			if (RedirectTarget != null)
			{
				throw new InvalidOperationException("Cannot add children to a redirected node");
			}
			_arguments.AddChild(argument);
			return This;
		}

		public IEnumerable<CommandNode<TSource>> Arguments => _arguments.Children;

		public TThis Executes(Command<TSource> command)
		{
			Command = command;
			return This;
		}

		public Command<TSource> Command { get; private set; }

		public TThis Requires(Predicate<TSource> requirement)
		{
			Requirement = requirement;
			return This;
		}

		public Predicate<TSource> Requirement { get; private set; } = s => true;

		public TThis Redirect(CommandNode<TSource> target)
		{
			return Forward(target, null, false);
		}

		public TThis Redirect(CommandNode<TSource> target, SingleRedirectModifier<TSource> modifier)
		{
			return Forward(target, modifier == null ? (RedirectModifier<TSource>)null: o => new[] { modifier(o) }, false);
		}

		public TThis Fork(CommandNode<TSource> target, RedirectModifier<TSource> modifier)
		{
			return Forward(target, modifier, true);
		}

		public TThis Forward(CommandNode<TSource> target, RedirectModifier<TSource> modifier, bool fork)
		{
			if (_arguments.Children.Count > 0)
			{
				throw new InvalidOperationException("Cannot forward a node with children");
			}

			RedirectTarget = target;
			RedirectModifier = modifier;
			IsFork = fork;
			return This;
		}

		public CommandNode<TSource> RedirectTarget { get; private set; }

		public RedirectModifier<TSource> RedirectModifier { get; private set; }

		public bool IsFork { get; private set; }

		public abstract TNode Build();
	}
}
