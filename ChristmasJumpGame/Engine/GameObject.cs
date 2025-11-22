using Blazor.Extensions.Canvas.Canvas2D;

namespace ChristmasJumpGame.Engine
{
    public abstract class GameObject(Game game) : GameEntity
    {
        public SpriteAsset? Sprite { get; set; }
        public BoundingBox? BoundingBox { get; set; }

        private float imageIndex;
        public int ImageIndex { get => (int)imageIndex; set => imageIndex = value; }
        public float ImageSpeed { get; set; } = 1;

        public float OriginalX { get; init; }
        public float OriginalY { get; init; }
        public float X { get; set; }
        public float Y { get; set; }
        public float VSpeed { get; set; }
        public float HSpeed { get; set; }
        public float VAcceleration { get; set; }
        public float HAcceleration { get; set; }

        public Game Game { get; } = game;

        public async ValueTask ExecuteStepAsync()
        {
            await OnStepAsync();

            if (Sprite is not null)
            {
                imageIndex = (imageIndex + ImageSpeed) % Sprite.ImageCount;
            }

            MoveContactSolid(HSpeed, VSpeed);

            HSpeed += HAcceleration;
            VSpeed += VAcceleration;
        }

        public override async ValueTask OnDrawAsync(Canvas2DContext context)
        {
            if (Sprite is not null)
                await context.DrawSpriteAsync(Sprite, ImageIndex, X, Y);
        }

        public bool IsPointEmpty(float x, float y) => !game.IsSolidAt(x, y);
        public bool IsPointFree(float x, float y)
        {
            if (BoundingBox is null)
                return true;

            return IsPointEmpty(x + BoundingBox.X, y + BoundingBox.Y) &&
                   IsPointEmpty(x + BoundingBox.X + BoundingBox.Width - 1, y + BoundingBox.Y) &&
                   IsPointEmpty(x + BoundingBox.X, y + BoundingBox.Y + BoundingBox.Height - 1) &&
                   IsPointEmpty(x + BoundingBox.X + BoundingBox.Width - 1, y + BoundingBox.Y + BoundingBox.Height - 1);
        }
        
        public bool IsCollidingWith<T>(out T[]? others) where T : GameObject
        {
            others = default;
            var collidingObjects = game.Instances.OfType<T>().Where(IsCollidingWith).ToArray();
            if (collidingObjects.Length is 0)
                return false;
            others = collidingObjects;
            return true;
        }
        public bool IsCollidingWith(GameObject other)
        {
            if (BoundingBox is null || other.BoundingBox is null)
                return false;

            return !(X + BoundingBox.X + BoundingBox.Width <= other.X + other.BoundingBox.X ||
                     X + BoundingBox.X >= other.X + other.BoundingBox.X + other.BoundingBox.Width ||
                     Y + BoundingBox.Y + BoundingBox.Height <= other.Y + other.BoundingBox.Y ||
                     Y + BoundingBox.Y >= other.Y + other.BoundingBox.Y + other.BoundingBox.Height);
        }

        public bool MouseCheckButton(MouseButton button) => Game.MouseCheckButton(button);
        public bool MouseCheckButtonReleased(MouseButton button) => Game.MouseCheckButtonReleased(button);
        public bool MouseCheckButtonPressed(MouseButton button) => Game.MouseCheckButtonPressed(button);

        public bool KeyboardCheck(string key) => Game.KeyboardCheck(key);
        public bool KeyboardCheckReleased(string key) => Game.KeyboardCheckReleased(key);
        public bool KeyboardCheckPressed(string key) => Game.KeyboardCheckPressed(key);

        public IEnumerable<Guid> GetControllerInputDevices() => Game.GetInputDevices();

        public bool ControllerInputCheck(Guid controller, string inputId) => Game.ControllerInputCheck(controller, inputId);
        public bool ControllerInputCheckPressed(Guid controller, string inputId) => Game.ControllerInputCheckPressed(controller, inputId);
        public bool ControllerInputCheckReleased(Guid controller, string inputId) => Game.ControllerInputCheckReleased(controller, inputId);
        public bool ControllerInputCheckAny(string inputId) => Game.ControllerInputCheckAny(inputId);
        public bool ControllerInputCheckAnyPressed(string inputId) => Game.ControllerInputCheckAnyPressed(inputId);
        public bool ControllerInputCheckAnyReleased(string inputId) => Game.ControllerInputCheckAnyReleased(inputId);

        public bool MoveOutsideSolid(float deltaX, float deltaY, float resolution = 1f)
        {
            var direction = Angle.Atan2(-deltaY, deltaX);
            var distance = MathF.Sqrt(deltaX * deltaX + deltaY * deltaY);
            return MoveOutsideSolid(direction, distance, resolution);
        }
        public bool MoveOutsideSolid(Angle direction, float maxDistance, float resolution = 1f)
        {
            float incrementX = direction.Cos();
            float incrementY = -direction.Sin();
            var (newX, newY) = (X, Y);
            var distance = 0f;
            while (distance <= maxDistance)
            {
                newX = X + incrementX * distance;
                newY = Y + incrementY * distance;
                if (IsPointFree(newX, newY))
                    break;
                distance += resolution;
            }
            if (distance <= 0)
                return false;
            if (distance > maxDistance)
            {
                distance = maxDistance;
                newX = X + incrementX * distance;
                newY = Y + incrementY * distance;
            }
            X = newX;
            Y = newY;
            return true;
        }
        public bool MoveContactSolid(float deltaX, float deltaY, float resolution = 1f)
        {
            var direction = Angle.Atan2(-deltaY, deltaX);
            var distance = MathF.Sqrt(deltaX * deltaX + deltaY * deltaY);
            return MoveContactSolid(direction, distance, resolution);
        }
        public bool MoveContactSolid(Angle direction, float maxDistance, float resolution = 1f)
        {
            float incrementX = direction.Cos();
            float incrementY = -direction.Sin();

            var (newX, newY) = (X, Y);

            var distance = 0f;
            while (distance <= maxDistance)
            {
                newX = X + incrementX * distance;
                newY = Y + incrementY * distance;

                if (!IsPointFree(newX, newY))
                    break;

                distance += resolution;
            }
            if (distance <= 0)
                return false;

            if (distance > maxDistance)
            {
                distance = maxDistance;
                newX = X + incrementX * distance;
                newY = Y + incrementY * distance;
            }

            X = newX;
            Y = newY;

            return true;
        }
    }
}
