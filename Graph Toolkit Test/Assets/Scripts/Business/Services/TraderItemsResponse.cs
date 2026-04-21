using System.Collections.Generic;
using Prototype.Business.Data;

namespace Prototype.Business.Services
{
	public sealed class TraderItemsResponse
	{
		public bool Success { get; set; }
		public string ErrorCode { get; set; }
		public string Message { get; set; }
		public string TraderId { get; set; }
		public string TraderName { get; set; }
		public List<TraderItemDefinitionData> Items { get; } = new();
	}
}
