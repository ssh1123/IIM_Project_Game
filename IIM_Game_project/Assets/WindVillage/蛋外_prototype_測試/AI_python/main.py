import json
import os
import subprocess
import sys
import traceback
from pathlib import Path
from typing import List

from fastapi import FastAPI, HTTPException
from pydantic import BaseModel

app = FastAPI()

BASE_DIR = Path(__file__).resolve().parent
PROJECT_DIR = BASE_DIR
GENERATE_SCRIPT = PROJECT_DIR / "scripts" / "generate_answer.py"


class AskRequest(BaseModel):
    question: str


class CitationItem(BaseModel):
    label: str
    chunk_id: str
    document_id: str
    title: str
    section: str
    source_file: str


class AskResponse(BaseModel):
    question: str
    short_answer: str
    #detailed_answer: str
    #citation_labels: List[str]
    #matched_citations: List[CitationItem]
    #answer_text: str


@app.get("/")
def root():
    return {"message": "AI RAG server is running"}


@app.get("/health")
def health():
    return {"status": "ok"}


def run_generate_answer(question: str) -> dict:
    if not GENERATE_SCRIPT.exists():
        raise FileNotFoundError(f"找不到 generate_answer.py：{GENERATE_SCRIPT}")

    print("sys.executable =", sys.executable)
    print("GENERATE_SCRIPT =", GENERATE_SCRIPT)
    print("PROJECT_DIR =", PROJECT_DIR)

    cmd = [
        sys.executable,
        str(GENERATE_SCRIPT),
        question,
        "--pretty"
    ]

    env = os.environ.copy()
    env["PYTHONIOENCODING"] = "utf-8"
    env["PYTHONUTF8"] = "1"

    result = subprocess.run(
        cmd,
        cwd=str(PROJECT_DIR),
        capture_output=True,
        env=env
    )

    stdout = result.stdout.decode("utf-8", errors="replace").strip()
    stderr = result.stderr.decode("utf-8", errors="replace").strip()

    print("subprocess returncode =", result.returncode)
    print("subprocess stdout =\n", stdout)
    print("subprocess stderr =\n", stderr)

    if result.returncode != 0:
        raise RuntimeError(
            "generate_answer.py 執行失敗\n"
            f"returncode={result.returncode}\n"
            f"stdout=\n{stdout}\n"
            f"stderr=\n{stderr}"
        )

    if not stdout:
        raise RuntimeError("generate_answer.py 沒有輸出任何內容。")

    try:
        data = json.loads(stdout)
    except json.JSONDecodeError as e:
        raise RuntimeError(
            "無法解析 generate_answer.py 的 JSON 輸出\n"
            f"錯誤：{e}\n"
            f"stdout=\n{stdout}"
        )

    return data


@app.post("/ask", response_model=AskResponse)
def ask(req: AskRequest):
    try:
        print("===== /ask called =====")
        print("question:", req.question)

        data = run_generate_answer(req.question)

        return {
            "question": data.get("question", req.question),
            "short_answer": data.get("short_answer", ""),
            #"detailed_answer": data.get("detailed_answer", ""),
            #"citation_labels": data.get("citation_labels", []),
            #"matched_citations": data.get("matched_citations", []),
            #"answer_text": data.get("answer_text", "")
        }

    except Exception as e:
        traceback.print_exc()
        raise HTTPException(status_code=500, detail=str(e))