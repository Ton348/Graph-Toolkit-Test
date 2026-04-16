using System;
using Unity.GraphToolkit.Editor;
using Game1.Graph.Runtime;
using Game1.Graph.Editor;

using Game1.Graph.Editor.Templates;
using Graph.Core.Editor;

[Serializable]
[UseWithGraph(typeof(CommonGraphEditorGraph))]
public sealed class RequestAssignSupplierNodeModel : GameGraphSuccessFailNodeModel
{
	public const string LotIdOption = "LotId";
	public const string SupplierIdOption = "SupplierId";

	protected override string defaultTitle => "Назначить поставщика";
	protected override string defaultDescription => "Назначает поставщика бизнесу.";

	protected override void OnDefineOptions(IOptionDefinitionContext context)
	{
		base.OnDefineOptions(context);
		context.AddOption<string>(LotIdOption).WithDisplayName("LotId");
		context.AddOption<string>(SupplierIdOption).WithDisplayName("SupplierId");
	}
}