using System;
using GraphCore.Runtime;

namespace GraphCore.Runtime
{
	public abstract class GraphContextKey : IEquatable<GraphContextKey>
	{
		private readonly string m_id;
		private readonly Type m_valueType;

		public string Id => m_id;
		public Type ValueType => m_valueType;

		protected GraphContextKey(string id, Type valueType)
		{
			if (string.IsNullOrWhiteSpace(id))
			{
				throw new ArgumentException("GraphContextKey id cannot be null, empty or whitespace.", nameof(id));
			}

			m_id = id;
			m_valueType = valueType ?? typeof(object);
		}

		public override string ToString()
		{
			return $"{Id}<{ValueType.Name}>";
		}

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

			return m_id == other.m_id && m_valueType == other.m_valueType;
		}

		public override bool Equals(object obj)
		{
			return obj is GraphContextKey other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((m_id != null ? m_id.GetHashCode() : 0) * 397) ^ (m_valueType != null ? m_valueType.GetHashCode() : 0);
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
