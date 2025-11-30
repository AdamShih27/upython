using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 遊戲狀態資料結構
/// 對應 Python Server 回傳的 JSON 格式
/// </summary>
[Serializable]
public class GameState
{
    public List<List<int>> snake;
    public List<int> food;
    public int score;
    public bool game_over;
    public int width;
    public int height;

    /// <summary>
    /// 從 JSON 字串解析 GameState
    /// Unity 的 JsonUtility 不支援巢狀陣列，所以需要手動解析
    /// </summary>
    public static GameState FromJson(string json)
    {
        GameState state = new GameState();
        state.snake = new List<List<int>>();
        state.food = new List<int>();

        try
        {
            // 移除空白
            json = json.Trim();

            // 解析 snake 陣列
            int snakeStart = json.IndexOf("\"snake\"") + 9;
            int snakeEnd = FindMatchingBracket(json, snakeStart);
            string snakeStr = json.Substring(snakeStart, snakeEnd - snakeStart + 1);
            state.snake = ParseNestedArray(snakeStr);

            // 解析 food 陣列
            int foodStart = json.IndexOf("\"food\"") + 8;
            int foodEnd = FindMatchingBracket(json, foodStart);
            string foodStr = json.Substring(foodStart, foodEnd - foodStart + 1);
            state.food = ParseSimpleArray(foodStr);

            // 解析 score
            state.score = ParseIntValue(json, "score");

            // 解析 game_over
            state.game_over = ParseBoolValue(json, "game_over");

            // 解析 width
            state.width = ParseIntValue(json, "width");

            // 解析 height
            state.height = ParseIntValue(json, "height");
        }
        catch (Exception e)
        {
            Debug.LogError("JSON Parse Error: " + e.Message + "\nJSON: " + json);
        }

        return state;
    }

    private static int FindMatchingBracket(string str, int start)
    {
        int depth = 0;
        for (int i = start; i < str.Length; i++)
        {
            if (str[i] == '[') depth++;
            else if (str[i] == ']') depth--;
            if (depth == 0) return i;
        }
        return str.Length - 1;
    }

    private static List<List<int>> ParseNestedArray(string str)
    {
        List<List<int>> result = new List<List<int>>();
        str = str.Trim().Trim('[', ']');
        
        int depth = 0;
        int start = 0;
        
        for (int i = 0; i < str.Length; i++)
        {
            if (str[i] == '[') 
            {
                if (depth == 0) start = i;
                depth++;
            }
            else if (str[i] == ']')
            {
                depth--;
                if (depth == 0)
                {
                    string inner = str.Substring(start, i - start + 1);
                    result.Add(ParseSimpleArray(inner));
                }
            }
        }
        
        return result;
    }

    private static List<int> ParseSimpleArray(string str)
    {
        List<int> result = new List<int>();
        str = str.Trim().Trim('[', ']');
        
        if (string.IsNullOrEmpty(str)) return result;
        
        string[] parts = str.Split(',');
        foreach (string part in parts)
        {
            if (int.TryParse(part.Trim(), out int val))
            {
                result.Add(val);
            }
        }
        
        return result;
    }

    private static int ParseIntValue(string json, string key)
    {
        string search = "\"" + key + "\"";
        int idx = json.IndexOf(search);
        if (idx < 0) return 0;
        
        idx = json.IndexOf(":", idx) + 1;
        int end = json.IndexOfAny(new char[] { ',', '}' }, idx);
        string val = json.Substring(idx, end - idx).Trim();
        
        int.TryParse(val, out int result);
        return result;
    }

    private static bool ParseBoolValue(string json, string key)
    {
        string search = "\"" + key + "\"";
        int idx = json.IndexOf(search);
        if (idx < 0) return false;
        
        idx = json.IndexOf(":", idx) + 1;
        int end = json.IndexOfAny(new char[] { ',', '}' }, idx);
        string val = json.Substring(idx, end - idx).Trim().ToLower();
        
        return val == "true";
    }
}
