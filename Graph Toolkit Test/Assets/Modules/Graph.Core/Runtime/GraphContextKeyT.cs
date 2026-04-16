namespace Graph.Core.Runtime
{
	public sealed class GraphContextKey<T> : GraphContextKey
	{
		public GraphContextKey(string id)
			: base(id, typeof(T))
		{
		}
	}
}