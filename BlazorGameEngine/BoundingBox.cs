namespace BlazorGameEngine
{
    public record BoundingBox(float X, float Y, float Width, float Height)
    {
        public bool Intersects(BoundingBox other)
        {
            return X < other.X + other.Width &&
                   X + Width > other.X &&
                   Y < other.Y + other.Height &&
                   Y + Height > other.Y;
        }
        public bool Contains(float x, float y)
        {
            return x >= X && x <= X + Width && y >= Y && y <= Y + Height;
        }
    }
}
