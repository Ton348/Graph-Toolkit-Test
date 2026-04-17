using System;

namespace Graph.Core.Runtime
{
	public abstract class GraphContextKey : IEquatable<GraphContextKey>
	{
		protected GraphContextKey(string id, Type valueType)
		{
			if (string.IsNullOrWhiteSpace(id))
			{
				throw new ArgumentException("GraphContextKey id cannot be null, empty or whitespace.", nameof(id));
			}

			Id = id;
			ValueType = valueType ?? typeof(object);
		}

		public string Id { get; }

		public Type ValueType { get; }

		public bool Equals(GraphContextKey other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return Id == other.Id && ValueType == other.ValueType;
		}

		public override string ToString()
		{
			return $"{Id}<{ValueType.Name}>";
		}

		public override bool Equals(object obj)
		{
			return obj is GraphContextKey other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((Id != null ? Id.GetHashCode() : 0) * 397) ^ (ValueType != null ? ValueType.GetHashCode() : 0);
			}
		}

		public static bool operator ==(GraphContextKey left, GraphContextKey right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(GraphContextKey left, GraphContextKey right)
		{
			return !Equals(left, right);
		}
	}
}