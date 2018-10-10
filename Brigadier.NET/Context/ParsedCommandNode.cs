using System;
using Brigadier.NET.Tree;
using Brigadier.NET.Util;

namespace Brigadier.NET.Context
{
	public class ParsedCommandNode<TSource> : IEquatable<ParsedCommandNode<TSource>>
	{
		public ParsedCommandNode(CommandNode<TSource> node, StringRange range)
		{
			Node = node;
			Range = range;
		}

		public CommandNode<TSource> Node { get; }

		public StringRange Range { get; }

		public override string ToString()
		{
			return $"{Node}@{Range}";
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is ParsedCommandNode<TSource> other && Equals(other);
		}

		public bool Equals(ParsedCommandNode<TSource> other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(Node, other.Node) && Equals(Range, other.Range);
		}

		public override int GetHashCode()
		{
			return HashCode.Start
				.Hash(Node)
				.Hash(Range);
		}
	}
}
