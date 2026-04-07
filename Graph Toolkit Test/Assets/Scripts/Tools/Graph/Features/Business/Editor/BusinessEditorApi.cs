using System;

namespace Graph.Features.Business.Editor
{
    [Serializable] public abstract class BusinessNodeModel : global::BusinessQuestBusinessNodeModel { }

    [Serializable] public abstract class CheckBusinessExistsNodeModel : global::CheckBusinessExistsNodeModel { }
    [Serializable] public abstract class CheckBusinessModuleInstalledNodeModel : global::CheckBusinessModuleInstalledNodeModel { }
    [Serializable] public abstract class CheckBusinessOpenNodeModel : global::CheckBusinessOpenNodeModel { }
    [Serializable] public abstract class CheckContactKnownNodeModel : global::CheckContactKnownNodeModel { }

    [Serializable] public abstract class RequestBuyBuildingNodeModel : global::RequestBuyBuildingNodeModel { }
    [Serializable] public abstract class RequestRentBusinessNodeModel : global::RequestRentBusinessNodeModel { }
    [Serializable] public abstract class RequestAssignBusinessTypeNodeModel : global::RequestAssignBusinessTypeNodeModel { }
    [Serializable] public abstract class RequestInstallBusinessModuleNodeModel : global::RequestInstallBusinessModuleNodeModel { }
    [Serializable] public abstract class RequestAssignSupplierNodeModel : global::RequestAssignSupplierNodeModel { }
    [Serializable] public abstract class RequestHireBusinessWorkerNodeModel : global::RequestHireBusinessWorkerNodeModel { }
    [Serializable] public abstract class RequestOpenBusinessNodeModel : global::RequestOpenBusinessNodeModel { }
    [Serializable] public abstract class RequestCloseBusinessNodeModel : global::RequestCloseBusinessNodeModel { }
    [Serializable] public abstract class RequestSetBusinessMarkupNodeModel : global::RequestSetBusinessMarkupNodeModel { }
    [Serializable] public abstract class RequestSetBusinessOpenNodeModel : global::RequestSetBusinessOpenNodeModel { }
    [Serializable] public abstract class RequestTradeOfferNodeModel : global::RequestTradeOfferNodeModel { }
    [Serializable] public abstract class RequestUnlockContactNodeModel : global::RequestUnlockContactNodeModel { }
}
