using System;

namespace Graph.Features.Business.Editor
{
    [Serializable] public abstract class BusinessNodeModel : global::BusinessQuestBusinessNodeModel { }

    [Serializable] public class CheckBusinessExistsNodeModel : global::CheckBusinessExistsNodeModel { }
    [Serializable] public class CheckBusinessModuleInstalledNodeModel : global::CheckBusinessModuleInstalledNodeModel { }
    [Serializable] public class CheckBusinessOpenNodeModel : global::CheckBusinessOpenNodeModel { }
    [Serializable] public class CheckContactKnownNodeModel : global::CheckContactKnownNodeModel { }

    [Serializable] public class RequestBuyBuildingNodeModel : global::RequestBuyBuildingNodeModel { }
    [Serializable] public class RequestRentBusinessNodeModel : global::RequestRentBusinessNodeModel { }
    [Serializable] public class RequestAssignBusinessTypeNodeModel : global::RequestAssignBusinessTypeNodeModel { }
    [Serializable] public class RequestInstallBusinessModuleNodeModel : global::RequestInstallBusinessModuleNodeModel { }
    [Serializable] public class RequestAssignSupplierNodeModel : global::RequestAssignSupplierNodeModel { }
    [Serializable] public class RequestHireBusinessWorkerNodeModel : global::RequestHireBusinessWorkerNodeModel { }
    [Serializable] public class RequestOpenBusinessNodeModel : global::RequestOpenBusinessNodeModel { }
    [Serializable] public class RequestCloseBusinessNodeModel : global::RequestCloseBusinessNodeModel { }
    [Serializable] public class RequestSetBusinessMarkupNodeModel : global::RequestSetBusinessMarkupNodeModel { }
    [Serializable] public class RequestSetBusinessOpenNodeModel : global::RequestSetBusinessOpenNodeModel { }
    [Serializable] public class RequestTradeOfferNodeModel : global::RequestTradeOfferNodeModel { }
    [Serializable] public class RequestUnlockContactNodeModel : global::RequestUnlockContactNodeModel { }
}
