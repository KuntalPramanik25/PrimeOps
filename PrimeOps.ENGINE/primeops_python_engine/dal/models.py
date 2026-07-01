from dataclasses import dataclass
from datetime import datetime
from decimal import Decimal

@dataclass
class TItemsMaster:
    Id: int
    ItemId: int
    ItemCode: str
    ItemName: str
    Description: str | None = None
    Price: Decimal = Decimal("0")
    Quantity: int = 0
    IsActive: bool = True
    CreatedOn: datetime | None = None
    CreatedBy: int = 0
    ModifiedOn: datetime | None = None
    ModifiedBy: int | None = None