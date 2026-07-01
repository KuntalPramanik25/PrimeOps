from primeops_python_engine.common.result import PythonResult

def subtract(a: float, b: float) -> str:
    try:
        return PythonResult.ok(data=a - b, message="Subtraction successful").to_json()
    except Exception as exc:
        return PythonResult.fail(str(exc)).to_json()

def add(a: float, b: float) -> str:
    try:
        return PythonResult.ok(data=a + b, message="Addition successful").to_json()
    except Exception as exc:
        return PythonResult.fail(str(exc)).to_json()
