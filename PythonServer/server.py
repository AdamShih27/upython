"""
UPython Snake Game Server
This server handles communication between Unity and the game logic.
"""

from flask import Flask, request, jsonify
from flask_cors import CORS

from snake_game_lab import SnakeGame  

app = Flask(__name__)
CORS(app)  

game = None

@app.route('/start', methods=['POST'])
def start_game():
    global game
    game = SnakeGame(width=20, height=15)
    state = game.get_state()
    print(f"[Server] Game started! Initial state: {state}")
    return jsonify(state)


@app.route('/action', methods=['POST'])
def game_action():
    global game
    
    if game is None:
        return jsonify({"error": "Game not started"}), 400
    
    data = request.get_json()
    direction = data.get('direction', None)
    
    print(f"[Server] Received direction: {direction}")
    
    state = game.update(direction)
    
    print(f"[Server] Snake: {state['snake'][:3]}... Score: {state['score']}")
    
    return jsonify(state)


@app.route('/state', methods=['GET'])
def get_state():
    global game
    
    if game is None:
        return jsonify({"error": "Game not started"}), 400
    
    return jsonify(game.get_state())


@app.route('/health', methods=['GET'])
def health_check():
    return jsonify({"status": "ok", "message": "Snake Game Server is running!"})


if __name__ == '__main__':
    print("=" * 50)
    print("  UPython Snake Game Server")
    print("  http://localhost:5000")
    print("=" * 50)
    print("\nEndpoints:")
    print("  POST /start    - Start new game")
    print("  POST /action   - Send direction (UP/DOWN/LEFT/RIGHT)")
    print("  GET  /state    - Get current game state")
    print("  GET  /health   - Health check")
    print("\nWaiting for Unity client to connect...\n")
    
    app.run(host='0.0.0.0', port=5000, debug=True)
