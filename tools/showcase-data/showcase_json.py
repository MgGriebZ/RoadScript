# Writes showcase JSON with 2-space indents, keeping each column and milestone on one line.
import json
from pathlib import Path

OUT_DIR = Path(__file__).resolve().parents[2] / "wwwroot" / "showcase"

INLINE_ARRAYS = {"columns", "milestones"}

def _dump(value, indent, key=None):
    pad = "  " * indent
    inner = "  " * (indent + 1)
    if isinstance(value, dict):
        if not value:
            return "{}"
        parts = [f'{inner}{json.dumps(k, ensure_ascii=False)}: {_dump(v, indent + 1, k)}' for k, v in value.items()]
        return "{\n" + ",\n".join(parts) + "\n" + pad + "}"
    if isinstance(value, list):
        if not value:
            return "[]"
        if key in INLINE_ARRAYS:
            parts = [inner + json.dumps(v, ensure_ascii=False, separators=(", ", ": ")) for v in value]
        else:
            parts = [inner + _dump(v, indent + 1) for v in value]
        return "[\n" + ",\n".join(parts) + "\n" + pad + "]"
    return json.dumps(value, ensure_ascii=False)

def write(data, path):
    text = _dump(data, 0) + "\n"
    assert json.loads(text) == data
    with open(path, "w") as f:
        f.write(text)
