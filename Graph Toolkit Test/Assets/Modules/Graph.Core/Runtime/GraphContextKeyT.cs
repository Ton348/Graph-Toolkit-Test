using GraphCore.Runtime;
namespace GraphCore.Runtime
{
	public sealed class GraphContextKey<T> : GraphContextKey
	{
		public GraphContextKey(string id)
			: base(id, typeof(T))
		{
		}
	}
}
