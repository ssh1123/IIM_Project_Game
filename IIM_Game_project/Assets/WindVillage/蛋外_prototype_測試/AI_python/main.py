from fastapi import FastAPI
from pydantic import BaseModel

app = FastAPI()

class AskRequest(BaseModel):
    question: str

class AskResponse(BaseModel):
    answer: str

@app.get("/")
def root():
    return {"message": "AI test server is running"}


@app.get("/health")
def health():
    return {"status": "ok"}

@app.post("/ask", response_model=AskResponse)
def ask_ai(data: AskRequest):
    user_question = data.question.strip()

    if user_question == "":
        return {"answer": "你還沒有輸入問題"}

    return {"answer": f"測試成功！你問的是{user_question}"}