# 風待村 Unity 主要地圖套件

## 適用環境
- 建議 Unity 2021.3 LTS、2022.3 LTS 或更新版本
- 2D / Built-in / URP 均可
- 需要安裝 TextMeshPro（Unity 通常內建）

## 匯入方式
1. 解壓縮此套件。
2. 將 `Assets/WindVillage` 整個資料夾複製到你的 Unity 專案 `Assets` 內。
3. 回到 Unity，等待程式編譯完成。
4. 第一次使用 TextMeshPro 時，選擇：
   `Window > TextMeshPro > Import TMP Essential Resources`
5. 點擊上方選單：
   `Tools > Wind Village > Build Main Map Scene`
6. 系統會自動建立：
   `Assets/WindVillage/Scenes/WindVillage_MainMap.unity`
7. 開啟場景並按 Play。

## 已建立功能
- 左上：回收站
- 右上：竹子工坊
- 左下：廟宇
- 中下：活動中心
- 右下：空屋巷
- 五個可點擊地點按鈕
- 玩家標記會移動至所選地點
- 地點名稱、說明與「進入地點」按鈕
- 原始草圖作為半透明設計參考背景

## 後續延伸
在 `WindVillageMapController.EnterSelectedLocation()` 裡，可以加入：
- 切換 Unity Scene
- 開啟 NPC 對話
- 啟動任務
- 顯示選擇題
- 記錄玩家造訪與作答資料

## 重要提醒
本套件使用 Editor Builder 自動生成場景，因此不需要手動處理 Unity Scene YAML。若你的專案使用新版 Input System，場景仍能建立；若按鈕無法操作，可把 EventSystem 的輸入模組改成 `Input System UI Input Module`。
