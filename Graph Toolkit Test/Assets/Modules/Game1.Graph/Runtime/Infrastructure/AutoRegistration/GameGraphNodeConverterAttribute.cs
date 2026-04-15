using System;
using GraphCore.Runtime;
using Game1.Graph.Runtime;

namespace Game1.Graph.Runtime.Infrastructure.AutoRegistration
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public sealed class GameGraphNodeConverterAttribute : Attribute
	{
	}
}
