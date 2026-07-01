from pathlib import Path
from typing import Type, TypeVar
import polars as pl

T = TypeVar("T")


class actions:
    
    # @staticmethod
    # def get_data_from_dal (entity_name: str, entity_type: Type[T], folder_path: str) -> list[T]:
    #     file_path = Path(folder_path) / f"{entity_name}.parquet"
    #     if not file_path.exists():
    #         raise FileNotFoundError(f"Parquet file not found: {file_path}")

    #     df = pl.scan_parquet(file_path)
    #     return [
    #         entity_type(**row)
    #         for row in df.to_dicts()
    #     ]