using System;
using GraphCore.Editor;
using Unity.GraphToolkit.Editor;

[Serializable]
[UseWithGraph(typeof(CommonGraphEditorGraph))]
public sealed class RequestAssignBusinessTypeNodeModel : GameGraphSuccessFailNodeModel
{
	public const string LOT_ID_OPTION = "LotId";
	public const string BUSINESS_TYPE_ID_OPTION = "BusinessTypeId";

	protected override string DefaultTitle => "Назначить тип бизнеса";
	protected override string DefaultDescription => "Назначает тип бизнеса для участка.";

	protected override void OnDefineOptions(IOptionDefinitionContext context)
	{
		base.OnDefineOptions(context);
		context.AddOption<string>(LOT_ID_OPTION).WithDisplayName("LotId");
		context.AddOption<string>(BUSINESS_TYPE_ID_OPTION).WithDisplayName("BusinessTypeId");
	}
}
