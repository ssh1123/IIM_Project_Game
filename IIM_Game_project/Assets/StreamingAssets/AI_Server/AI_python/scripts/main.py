import json
import os
import time
import traceback
from datetime import datetime
from pathlib import Path
from typing import List, Optional
from uuid import UUID

from dotenv import load_dotenv
from fastapi import FastAPI, HTTPException
from pydantic import BaseModel, Field
from supabase import Client, create_client

from simple_vector_store import search, load_index

from generate_answer import (
    DEFAULT_TOP_K,
    DEFAULT_GEMINI_BASE_URL,
    DEFAULT_GEMINI_MODEL,
    DEFAULT_API_KEY_ENV,
    build_context_blocks,
    format_context_for_prompt,
    build_system_prompt,
    build_user_prompt,
    build_citations,
    call_gemini_chat,
    try_parse_json_answer,
    fallback_parse_answer,
)


app = FastAPI()

load_dotenv()

SUPABASE_URL = os.getenv("SUPABASE_URL")
SUPABASE_SERVICE_ROLE_KEY = os.getenv("SUPABASE_SERVICE_ROLE_KEY")

if not SUPABASE_URL or not SUPABASE_SERVICE_ROLE_KEY:
    raise RuntimeError(
        "找不到 SUPABASE_URL 或 SUPABASE_SERVICE_ROLE_KEY。"
        "請確認 .env 與 main.py 位於同一個專案根目錄。"
    )

supabase: Client = create_client(
    SUPABASE_URL,
    SUPABASE_SERVICE_ROLE_KEY
)


# 全域快取：只在 FastAPI 啟動時載入一次
_embedding_function = None
_collection = None
_index_ready = False


@app.on_event("startup")
def load_model_and_index():
    global _index_ready

    print("正在載入 simple_index 和 embedding 模型...")
    load_index()
    _index_ready = True
    print("載入完成，server 已就緒。")


class AskRequest(BaseModel):
    question: str
    top_k: int = DEFAULT_TOP_K


class AskResponse(BaseModel):
    question: str
    short_answer: str
    # detailed_answer: str
    timing: dict


class QuestionAnswerInput(BaseModel):
    question_id: str = Field(min_length=1, max_length=200)
    question_order: int = Field(ge=1)
    attempt_number: int = Field(ge=1)
    selected_answer: Optional[str] = None
    is_correct: bool

    question_started_at: Optional[datetime] = None
    answered_at: Optional[datetime] = None

    answer_seconds: float = Field(ge=0)
    used_ai_hint: bool = False


class LearningGameResultInput(BaseModel):
    session_id: UUID
    player_id: str = Field(min_length=1, max_length=200)

    ai_assistant: bool = False
    feedback_quality: str = Field(pattern="^(high|low)$")

    is_cleared: bool = False
    play_seconds: float = Field(ge=0)

    correct_count: int = Field(ge=0)
    wrong_count: int = Field(ge=0)

    finished_at: Optional[datetime] = None
    answers: List[QuestionAnswerInput] = Field(default_factory=list)


class LearningTestAnswerInput(BaseModel):
    # 四題組識別：group_1、group_2、group_3、group_4
    question_group: str = Field(
        pattern="^(group_1|group_2|group_3|group_4)$"
    )

    # 題目固定 ID，例如 G1_Q01
    question_id: str = Field(min_length=1, max_length=200)

    # 此題在題組中的第幾題
    question_order: int = Field(ge=1)

    # 此題在整份測驗中的第幾題
    overall_question_order: int = Field(ge=1)

    # 如果成效測驗一題只能作答一次，Unity 一律傳 1
    # 若允許答錯重試，則在同一題每次嘗試加 1
    attempt_number: int = Field(default=1, ge=1)

    selected_answer: Optional[str] = None
    is_correct: bool
    
    question_started_at: Optional[datetime] = None
    answered_at: Optional[datetime] = None

    # 題目從顯示到送出答案的秒數
    answer_seconds: float = Field(ge=0)
    used_ai_hint: bool = False


class LearningTestResultInput(BaseModel):
    # 每進入一次第二部分測驗時，由 Unity Guid.NewGuid() 建立
    session_id: UUID

    # 對應第一部分 learning_game_results.id。
    # 若第二部分未接第一部分，可暫時不傳或傳 null。
    learning_game_result_id: Optional[int] = Field(default=None, ge=1)

    # 四個題組完成所花的總秒數
    test_seconds: float = Field(ge=0)

    # Unity 送來的統計；後端會用 answers 再驗證／重新計算
    total_question_count: int = Field(ge=0)
    correct_count: int = Field(ge=0)
    wrong_count: int = Field(ge=0)

    # 四題組是否全部完成
    is_completed: bool = True
    
    finished_at: Optional[datetime] = None

    # 此次測驗的所有逐題紀錄
    answers: List[LearningTestAnswerInput] = Field(default_factory=list)

@app.get("/")
def root():
    return {"message": "AI RAG server is running"}


@app.get("/health")
def health():
    return {"status": "ok" if _index_ready else "loading"}


@app.post("/learning-game-results")
def save_learning_game_result(payload: LearningGameResultInput):
    try:
        answer_count = len(payload.answers)

        calculated_correct_count = sum(
            1 for answer in payload.answers if answer.is_correct
        )
        calculated_wrong_count = sum(
            1 for answer in payload.answers if not answer.is_correct
        )

        if answer_count > 0:
            if payload.correct_count != calculated_correct_count:
                raise HTTPException(
                    status_code=400,
                    detail=(
                        "correct_count 與 answers 中 is_correct=true 的數量不一致。"
                    )
                )

            if payload.wrong_count != calculated_wrong_count:
                raise HTTPException(
                    status_code=400,
                    detail=(
                        "wrong_count 與 answers 中 is_correct=false 的數量不一致。"
                    )
                )

        game_row = {
            "session_id": str(payload.session_id),
            "player_id": payload.player_id,
            "ai_assistant": payload.ai_assistant,
            "feedback_quality": payload.feedback_quality,
            "is_cleared": payload.is_cleared,
            "play_seconds": payload.play_seconds,
            "correct_count": (
                calculated_correct_count
                if answer_count > 0
                else payload.correct_count
            ),
            "wrong_count": (
                calculated_wrong_count
                if answer_count > 0
                else payload.wrong_count
            ),
            "finished_at": (
                payload.finished_at.isoformat()
                if payload.finished_at
                else datetime.now().astimezone().isoformat()
            ),
        }

        game_response = (
            supabase
            .table("learning_game_results")
            .insert(game_row)
            .select("id, session_id")
            .execute()
        )

        if not game_response.data:
            raise RuntimeError("新增 learning_game_results 後沒有取得資料。")

        game_result_id = game_response.data[0]["id"]

        answer_rows = []

        for answer in payload.answers:
            answer_rows.append(
                {
                    "game_result_id": game_result_id,
                    "question_id": answer.question_id,
                    "attempt_number": answer.attempt_number,
                    "question_order": answer.question_order,
                    "selected_answer": answer.selected_answer,
                    "is_correct": answer.is_correct,
                    "question_started_at": (
                        answer.question_started_at.isoformat()
                        if answer.question_started_at
                        else None
                    ),
                    "answered_at": (
                        answer.answered_at.isoformat()
                        if answer.answered_at
                        else None
                    ),
                    "answer_seconds": answer.answer_seconds,
                    "used_ai_hint": answer.used_ai_hint,
                }
            )

        if answer_rows:
            (
                supabase
                .table("question_answer_records")
                .insert(answer_rows)
                .execute()
            )

        return {
            "success": True,
            "message": "遊戲總結果與逐題回答紀錄已成功儲存。",
            "game_result_id": game_result_id,
            "session_id": str(payload.session_id),
            "answer_record_count": len(answer_rows),
        }

    except HTTPException:
        raise

    except Exception as error:
        traceback.print_exc()

        raise HTTPException(
            status_code=500,
            detail=f"儲存遊戲結果失敗：{str(error)}"
        )
@app.post("/learning-test-results")
def save_learning_test_result(payload: LearningTestResultInput):
    try:
        answer_count = len(payload.answers)

        calculated_correct_count = sum(
            1 for answer in payload.answers if answer.is_correct
        )

        calculated_wrong_count = sum(
            1 for answer in payload.answers if not answer.is_correct
        )

        # 目前假設一筆 answer 就是一題作答紀錄。
        # 若測驗允許答錯重試，total_question_count 應改成「不重複題目數」，
        # 下方會用 unique question_id 計算。
        unique_question_ids = {
            answer.question_id
            for answer in payload.answers
        }

        calculated_total_question_count = len(unique_question_ids)

        # 若有傳逐題資料，避免 Unity 的總數字與實際紀錄不一致
        if answer_count > 0:
            if payload.total_question_count != calculated_total_question_count:
                raise HTTPException(
                    status_code=400,
                    detail=(
                        "total_question_count 與 answers 中不重複 "
                        "question_id 的數量不一致。"
                    )
                )

            if payload.correct_count != calculated_correct_count:
                raise HTTPException(
                    status_code=400,
                    detail=(
                        "correct_count 與 answers 中 is_correct=true "
                        "的數量不一致。"
                    )
                )

            if payload.wrong_count != calculated_wrong_count:
                raise HTTPException(
                    status_code=400,
                    detail=(
                        "wrong_count 與 answers 中 is_correct=false "
                        "的數量不一致。"
                    )
                )

        test_row = {
            "session_id": str(payload.session_id),
            "learning_game_result_id": payload.learning_game_result_id,
            "test_seconds": payload.test_seconds,
            "total_question_count": (
                calculated_total_question_count
                if answer_count > 0
                else payload.total_question_count
            ),
            "correct_count": (
                calculated_correct_count
                if answer_count > 0
                else payload.correct_count
            ),
            "wrong_count": (
                calculated_wrong_count
                if answer_count > 0
                else payload.wrong_count
            ),
            "is_completed": payload.is_completed,
            "finished_at": (
                payload.finished_at.isoformat()
                if payload.finished_at
                else datetime.now().astimezone().isoformat()
            ),
        }

        # 先新增一筆總結果，並取回資料庫產生的 id
        test_response = (
            supabase
            .table("learning_test_results")
            .insert(test_row)
            .select("id, session_id")
            .execute()
        )

        if not test_response.data:
            raise RuntimeError(
                "新增 learning_test_results 後沒有取得資料。"
            )

        test_result_id = test_response.data[0]["id"]

        # 將 Unity answers 陣列轉成逐題資料庫列
        answer_rows = []

        for answer in payload.answers:
            answer_rows.append(
                {
                    "test_result_id": test_result_id,
                    "question_group": answer.question_group,
                    "question_id": answer.question_id,
                    "question_order": answer.question_order,
                    "overall_question_order": (
                        answer.overall_question_order
                    ),
                    "attempt_number": answer.attempt_number,
                    "selected_answer": answer.selected_answer,
                    "is_correct": answer.is_correct,
                    "question_started_at": (
                        answer.question_started_at.isoformat()
                        if answer.question_started_at
                        else None
                    ),
                    "answered_at": (
                        answer.answered_at.isoformat()
                        if answer.answered_at
                        else None
                    ),
                    "answer_seconds": answer.answer_seconds,
                    "used_ai_hint": answer.used_ai_hint,
                }
            )

        # 有逐題資料才新增
        if answer_rows:
            (
                supabase
                .table("learning_test_answer_records")
                .insert(answer_rows)
                .execute()
            )

        return {
            "success": True,
            "message": "學習成效測驗總結果與逐題紀錄已成功儲存。",
            "test_result_id": test_result_id,
            "session_id": str(payload.session_id),
            "answer_record_count": len(answer_rows),
        }

    except HTTPException:
        raise

    except Exception as error:
        traceback.print_exc()

        raise HTTPException(
            status_code=500,
            detail=f"儲存學習成效測驗資料失敗：{str(error)}"
        )

@app.post("/ask", response_model=AskResponse)
def ask(req: AskRequest):
    try:
        if not _index_ready:
            raise RuntimeError("索引尚未載入完成，請稍後再試。")

        t0 = time.time()

        documents, metadatas, distances = search(
            query=req.question,
            top_k=req.top_k,
        )
        t1 = time.time()

        context_blocks = build_context_blocks(documents, metadatas, distances)
        context_text = format_context_for_prompt(context_blocks)

        system_prompt = build_system_prompt()
        user_prompt = build_user_prompt(req.question, context_text)

        t2 = time.time()

        raw_answer = call_gemini_chat(
            api_key=DEFAULT_API_KEY_ENV,
            base_url=DEFAULT_GEMINI_BASE_URL,
            model=DEFAULT_GEMINI_MODEL,
            system_prompt=system_prompt,
            user_prompt=user_prompt,
            temperature=0.3,
            max_tokens=800,
        )
        t3 = time.time()

        data = (
            try_parse_json_answer(raw_answer)
            or fallback_parse_answer(raw_answer)
        )

        return {
            "question": req.question,
            "short_answer": data.get("short_answer", ""),
            # "detailed_answer": data.get("detailed_answer", ""),
            "timing": {
                "retrieval": round(t1 - t0, 3),
                "prompt_build": round(t2 - t1, 3),
                "gemini": round(t3 - t2, 3),
                "total": round(t3 - t0, 3),
            },
        }

    except Exception as error:
        traceback.print_exc()
        raise HTTPException(status_code=500, detail=str(error))


if __name__ == "__main__":
    import multiprocessing
    import uvicorn

    multiprocessing.freeze_support()

    uvicorn.run(
        app,
        host="127.0.0.1",
        port=8001,
        log_level="info"
    )