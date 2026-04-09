using System.Collections.Generic;
using UnityEngine;

public static class LotDefinitionsValidator
{
    public static bool Validate(LotDatabaseData lotDatabase, BusinessDefinitionsRepository businessDefinitions)
    {
        bool ok = true;
        if (lotDatabase?.lots == null)
        {
            Debug.LogError("[LotDefinitions] lots list is missing.");
            return false;
        }

        var ids = new HashSet<string>();
        foreach (var lot in lotDatabase.lots)
        {
            if (lot == null || string.IsNullOrWhiteSpace(lot.id))
            {
                Debug.LogError("[LotDefinitions] lot has empty id.");
                ok = false;
                continue;
            }

            if (!ids.Add(lot.id))
            {
                Debug.LogError($"[LotDefinitions] duplicate lot id: {lot.id}");
                ok = false;
            }

            if (lot.rentPerDay < 0)
            {
                Debug.LogError($"[LotDefinitions] lot {lot.id} has negative rentPerDay.");
                ok = false;
            }

            if (lot.allowedBusinessTypes == null || lot.allowedBusinessTypes.Count == 0)
            {
                Debug.LogError($"[LotDefinitions] lot {lot.id} has empty allowedBusinessTypes.");
                ok = false;
            }
            else if (businessDefinitions != null)
            {
                foreach (var typeId in lot.allowedBusinessTypes)
                {
                    if (string.IsNullOrWhiteSpace(typeId))
                    {
                        Debug.LogError($"[LotDefinitions] lot {lot.id} has empty business type reference.");
                        ok = false;
                    }
                    else if (!businessDefinitions.HasBusinessType(typeId))
                    {
                        Debug.LogError($"[LotDefinitions] lot {lot.id} references unknown business type: {typeId}");
                        ok = false;
                    }
                }
            }
        }

        return ok;
    }
}

