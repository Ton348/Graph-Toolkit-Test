using System;
using Unity.GraphToolkit.Editor;

[Serializable]
[UseWithGraph(typeof(CommonGraphEditorGraph))]
public sealed class RequestAssignSupplierNodeModel : GameGraphSuccessFailNodeModel
{
	public const string LOT_ID_OPTION = "LotId";
	public const string SUPPLIER_ID_OPTION = "SupplierId";

	protected override string DefaultTitle => "Назначить поставщика";
	protected override string DefaultDescription => "Назначает поставщика бизнесу.";

	protected override void OnDefineOptions(IOptionDefinitionContext context)
	{
		base.OnDefineOptions(context);
		context.AddOption<string>(LOT_ID_OPTION).WithDisplayName("LotId");
		context.AddOption<string>(SUPPLIER_ID_OPTION).WithDisplayName("SupplierId");
	}
}
