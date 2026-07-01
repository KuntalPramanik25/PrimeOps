from __future__ import annotations
import json
from dataclasses import dataclass, field
from typing import Any


@dataclass
class PythonResult:
    success: bool
    message: str
    data: Any = field(default=None)

    def to_json(self) -> str:
        return json.dumps({
            "data":    self.data,
            "success": self.success,
            "message": self.message,
        })

    @staticmethod
    def ok(data: Any = None, message: str = "OK") -> "PythonResult":
        return PythonResult(success=True, message=message, data=data)

    @staticmethod
    def fail(message: str, data: Any = None) -> "PythonResult":
        return PythonResult(success=False, message=message, data=data)