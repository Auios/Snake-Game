using Raylib_cs;

namespace SnakeGame; 


// Rather than making a class for the food and a class for the snake itself,
// we can have one fundamental game object like class.
// What real differences are there between a food object and a snake's body part object?
// Just the colors they use when rendered for the most part.
public class TileObject {
    public static Color snakeColor = Color.WHITE;
    public static Color foodColor = Color.GREEN;

    public int x, y;

    public TileObject() {
        x = 0;
        y = 0;
    }

    public TileObject(int x, int y) {
        this.x = x;
        this.y = y;
    }

    public void SetPosition(int x, int y) {
        this.x = x;
        this.y = y;
    }
}
