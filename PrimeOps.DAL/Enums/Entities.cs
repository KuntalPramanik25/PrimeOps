using System;
using System.Collections.Generic;
using System.Text;

namespace PrimeOps.DAL.Enums
{
    public enum TableName
    {
        TItemsMaster,
        TItemsDetails,
        TItemsCategory,
        TItemsSubCategory
    }

    public enum TItemsMasterColumn
    {
        Id,
        ItemId,
        ItemCode,
        ItemName,
        Description,
        Price,
        Quantity,
        IsActive,
        CreatedOn,
        CreatedBy,
        ModifiedOn,
        ModifiedBy
    }
}
