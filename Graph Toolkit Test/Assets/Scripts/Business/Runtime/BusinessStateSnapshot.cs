using System;
using System.Collections.Generic;

[Serializable]
public class BusinessInstanceSnapshot
{
    public string instanceId;
    public string lotId;
    public string businessTypeId;
    public bool isRented;
    public bool isOpen;
    public int rentPerDay;
    public List<string> installedModules = new List<string>();
    public int storageCapacity;
    public int shelfCapacity;
    public int storageStock;
    public int shelfStock;
    public string selectedSupplierId;
    public int autoDeliveryPerDay;
    public int markupPercent;
    public string hiredCashierContactId;
    public string hiredMerchContactId;
}
