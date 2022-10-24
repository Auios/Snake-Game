using Raylib_cs;

namespace SnakeGame; 

public enum Direction {
    Up = 0,
    Down,
    Left,
    Right,
}

public static class Program {
    // Most, if not all the variables in the game are "static". Why? Because I felt like it.
    public static int windowWidth;
    public static int windowHeight;
    public static bool runApp = true;
    public static bool gameOver = false;

    public static TimeSpan tickSpan = TimeSpan.FromMilliseconds(100);
    public static DateTime lastUpdateTime = DateTime.Now;
    public static int mapWidth;
    public static int mapHeight;
    public static int tileWidth;
    public static int tileHeight;
    public static int offsetX;
    public static int offsetY;

    public static Direction dir;
    public static List<TileObject> snake;
    public static TileObject food = new TileObject();

    // Main entry point
    public static void Main() {
        CreateWindow(800, 600, 32, 32, "Snake Game");

        dir = Direction.Right;
        CreateSnake();
        PlaceFood();

        while(runApp) {
            HandleInput();
            Update();
            Render();
            Thread.Sleep(1);
        }

        CloseWindow();
    }

    // Create the game's window and set the dimensions of the game.
    public static void CreateWindow(int windowWidth, int windowHeight, int mapWidth, int mapHeight, string title) {
        Program.windowWidth = windowWidth;
        Program.windowHeight = windowHeight;

        Program.mapWidth = mapWidth;
        Program.mapHeight = mapHeight;
        float scale = 0.9f;
        tileWidth = (int)(Math.Min(windowWidth * scale, windowHeight * scale) / mapWidth);
        tileHeight = (int)(Math.Min(windowWidth * scale, windowHeight * scale) / mapHeight);

        offsetX = (int)((windowWidth - (tileWidth * mapWidth)) * 0.5f);
        offsetY = (int)((windowHeight - (tileHeight * mapHeight)) * 0.5f);

        Raylib.InitWindow(Program.windowWidth, Program.windowHeight, title);
        Raylib.SetExitKey(0);
    }

    // Close window. Don't really need this for such a simple action - but never hurts to be consistent.
    public static void CloseWindow() {
        Raylib.CloseWindow();
    }

    // Create the initial snake. Can be used later for when resetting the game.
    public static void CreateSnake() {
        snake = new List<TileObject>();
        snake.Add(new TileObject((mapWidth / 2) - 1, mapWidth / 2));
        snake.Add(new TileObject( mapWidth / 2,      mapWidth / 2));
    }

    // Places food randomly in the world.
    // Note: Random().Next()'s upper value is exclusive. Therefore we don't need to do `mapWidth - 1`.
    public static void PlaceFood() {
        food.x = new Random().Next(0, mapWidth);
        food.y = new Random().Next(0, mapHeight);
    }

    public static void HandleInput() {
        // To nothing if the game is over.
        // TODO: Listen for input to start a new game.
        if(gameOver) return;

        // Quit the game if the user presses escape
        if(Raylib.IsKeyPressed(KeyboardKey.KEY_ESCAPE)) runApp = false;

        // Change direction of the snake
        // Bug: You can't change direction to the opposite direction youre currently going, but...
        //      due to the length of time a tick takes, you can change direction to something else, and then again change direction to completely opposite.
        //      Therefore basically acting like you just completely went in reverse.
        // Solution: Only allow one direction change per tick by introducing a "hasChangedDirection" flag which resets every tick.
        if(Raylib.IsKeyDown(KeyboardKey.KEY_W) && dir != Direction.Down) dir = Direction.Up;
        if(Raylib.IsKeyDown(KeyboardKey.KEY_S) && dir != Direction.Up) dir = Direction.Down;
        if(Raylib.IsKeyDown(KeyboardKey.KEY_A) && dir != Direction.Right) dir = Direction.Left;
        if(Raylib.IsKeyDown(KeyboardKey.KEY_D) && dir != Direction.Left) dir = Direction.Right;
    }

    // Update
    public static void Update() {
        if(gameOver) return;

        // Only update if the last update was more than the update time length `tickSpan`
        if(DateTime.Now - lastUpdateTime > tickSpan) {
            lastUpdateTime = DateTime.Now;

            TileObject head = snake[snake.Count - 1];

            // Naive method to move the snake.
            // Ideal method would be to have a pointer to the snake's head and tail and move the tail to the head's former position
            // Then move the head to the new position
            // Then increment the pointers to the next segment.
            for(int i = 0; i < snake.Count - 1; i++) {
                snake[i].x = snake[i + 1].x;
                snake[i].y = snake[i + 1].y;
            }

            // Move the snake's head based on the direction it's going.
            if(dir == Direction.Left) {
                head.x -= 1;
            }
            else if(dir == Direction.Right) {
                head.x += 1;
            }
            else if(dir == Direction.Up) {
                head.y -= 1;
            }
            else if(dir == Direction.Down) {
                head.y += 1;
            }

            // Gameover checks
            // Did the snake go out of bounds?
            // Did the snake hit itself?
            if(head.x >= mapWidth) gameOver = true;
            if(head.x < 0) gameOver = true;
            if(head.y >= mapHeight) gameOver = true;
            if(head.y < 0) gameOver = true;

            for(int i = 0; i < snake.Count - 1; i++) {
                TileObject bodyPart = snake[i];

                if(head.x == bodyPart.x && head.y == bodyPart.y) {
                    gameOver = true;
                    return;
                }
            }

            // Snake noms food?
            if(head.x == food.x && head.y == food.y) {
                PlaceFood();
                snake.Add(new TileObject(head.x, head.y));
                // TODO: Increment score?
            }
        }
    }

    public static void Render() {
        // TODO: Render score

        Raylib.BeginDrawing();
        {
            Raylib.ClearBackground(Color.BLACK);

            // Center the board via glTranslate rather than using + offset on all the renders.
            // It's easier to work under local coordinate space.
            Rlgl.rlTranslatef(offsetX, offsetY, 0);

            // Draw grid
            for(int y = 0; y < mapHeight; y++) {
                for(int x = 0; x < mapWidth; x++) {
                    Raylib.DrawRectangleLines(x * tileWidth - 1, y * tileHeight - 1, tileWidth + 1, tileHeight + 1, Color.DARKGRAY);
                }
            }

            // Draw food
            Raylib.DrawRectangleLines(food.x * tileWidth - 1, food.y * tileHeight - 1, tileWidth + 1, tileHeight + 1, TileObject.foodColor);

            // Draw snake head
            Raylib.DrawRectangleLines(snake[0].x * tileWidth - 1, snake[0].y * tileHeight - 1, tileWidth + 1, tileHeight + 1, TileObject.snakeColor);

            // Draw snake body
            for(int i = 1; i < snake.Count; i++) {
                Raylib.DrawRectangleLines(snake[i].x * tileWidth - 1, snake[i].y * tileHeight - 1, tileWidth + 1, tileHeight + 1, TileObject.snakeColor);
            }
        }
        Raylib.EndDrawing();
    }
}