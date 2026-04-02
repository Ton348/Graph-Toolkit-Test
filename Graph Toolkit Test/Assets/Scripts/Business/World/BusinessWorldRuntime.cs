using System.Collections.Generic;
using UnityEngine;

public class BusinessWorldRuntime : MonoBehaviour
{
    public string lotId;
    public Transform storagePoint;
    public Transform shelvesPoint;
    public Transform cashierPoint;
    public Transform deliveryZone;
    public List<BusinessModuleSlot> moduleSlots = new List<BusinessModuleSlot>();

    public GameBootstrap bootstrap;

    private void Awake()
    {
        if (bootstrap == null)
        {
            bootstrap = FindObjectOfType<GameBootstrap>();
        }
    }

    public BusinessInstanceSnapshot GetBusiness()
    {
        return bootstrap != null && bootstrap.BusinessStateSyncService != null
            ? bootstrap.BusinessStateSyncService.GetBusinessByLotId(lotId)
            : null;
    }

    public bool IsOwned()
    {
        var business = GetBusiness();
        return business != null && business.isRented;
    }

    public bool IsOpen()
    {
        var business = GetBusiness();
        return business != null && business.isOpen;
    }

    public BusinessActionFacade GetActionFacade()
    {
        return bootstrap != null ? bootstrap.BusinessActionFacade : null;
    }

    public BusinessSimulationService GetSimulationService()
    {
        return bootstrap != null ? bootstrap.BusinessSimulationService : null;
    }
}
