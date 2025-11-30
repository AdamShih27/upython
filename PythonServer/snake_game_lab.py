"""
Snake Game Logic - Lab Version (Fill in the TODO blanks)
"""

import random


class SnakeGame:
    def __init__(self, width=20, height=15):
        self.width = width
        self.height = height
        self.reset()
    
    def reset(self):
        center_x = self.width // 2
        center_y = self.height // 2
        self.snake = [
            [center_x, center_y],      
            [center_x - 1, center_y],  
            [center_x - 2, center_y]   
        ]
        
        self.direction = "RIGHT"
        
        self.score = 0
        
        self.game_over = False
        
        self.food = self._generate_food()
    
    def _generate_food(self):
        while True:
            food = [
                random.randint(0, self.width - 1),
                random.randint(0, self.height - 1)
            ]
            if food not in self.snake:
                return food
    
    def update(self, new_direction):
        if self.game_over:
            return self.get_state()
        
        # ============================================================
        # TODO 1: 處理方向改變
        
        # 蛇不能 180 度轉向（例如：正在向右走，不能直接向左轉）
        # 請完成以下程式碼：
        
        opposite = {
            "UP": "DOWN",
            "DOWN": "UP", 
            "LEFT": "RIGHT",
            "RIGHT": "LEFT"
        }
        
        if new_direction:
            # 請在這裡填入程式碼：檢查並更新 self.direction

        # ============================================================   
        

        # ============================================================
        # TODO 2: 計算蛇頭的新位置
        
        # 根據 self.direction，計算蛇頭移動後的新座標
        #
        # 座標系統（Unity 座標）：
        #   - x 軸：LEFT 減少，RIGHT 增加
        #   - y 軸：UP 增加，DOWN 減少
        #
        # 請完成以下程式碼：
        
        head = self.snake[0].copy()  
        
        if self.direction == "UP":
            
        elif self.direction == "DOWN":
            
        elif self.direction == "LEFT":
            
        elif self.direction == "RIGHT":
            
        # ============================================================
        
        if head[0] < 0 or head[0] >= self.width or \
           head[1] < 0 or head[1] >= self.height:
            self.game_over = True
            return self.get_state()
        
        if head in self.snake:
            self.game_over = True
            return self.get_state()
        
        self.snake.insert(0, head)
        
        # ============================================================
        # TODO 3: 檢查是否吃到食物
        # 判斷蛇頭是否與食物位置重疊
        #
        # 如果吃到食物：
        #   - 分數 +10
        #   - 生成新食物
        #   - 蛇會變長
        #
        # 請完成以下程式碼：
        
        if head ==     :
            self.score +=    
        else:
            self.snake.   
        
        # ============================================================
        
        return self.get_state()
    
    def get_state(self):
        """
        取得當前遊戲狀態 - 這個資料會透過 JSON 傳送給 Unity
        
        Returns:
            dict: 包含遊戲狀態的字典
        """
        # ============================================================
        # TODO 4: 回傳遊戲狀態
        #
        # 請完成以下 dict：
        
        return {
            "snake": _____,      
            "food": _____,       
            "score": _____,      
            "game_over": _____,  
            "width": self.width,
            "height": self.height
        }
    
        # ============================================================


# ============================================================
# 測試用程式碼 - 可以單獨執行這個檔案來測試
# ============================================================
if __name__ == "__main__":
    print("Testing SnakeGame...")
    
    game = SnakeGame(width=10, height=10)
    print(f"Initial state: {game.get_state()}")
    
    # 測試移動
    directions = ["RIGHT", "RIGHT", "DOWN", "DOWN", "LEFT"]
    for d in directions:
        state = game.update(d)
        print(f"After {d}: Snake head at {state['snake'][0]}, Score: {state['score']}")
    
    print("\nIf you see the snake moving correctly, your implementation is working!")
