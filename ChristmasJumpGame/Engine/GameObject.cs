using Blazor.Extensions.Canvas.Canvas2D;

namespace ChristmasJumpGame.Engine
{

    

    public class GameObject(Game game) : GameEntity
    {
        public SpriteAsset? Sprite { get; set; }

        private float imageIndex;
        public int ImageIndex { get => (int)imageIndex; set => imageIndex = value; }
        public float ImageSpeed { get; set; }

        public float X { get; set; }
        public float Y { get; set; }
        public float VSpeed { get; set; }
        public float HSpeed { get; set; }
        public float Speed
        {
            get => MathF.Sqrt(VSpeed * VSpeed + HSpeed * HSpeed);
            set
            {
                (VSpeed, HSpeed) = (-Direction.Sin() * value, Direction.Cos() * value);
            }
        }
        public Angle Direction
        {
            get => Angle.Atan2(VSpeed, HSpeed);
            set
            {
                var speed = Speed;
                (VSpeed, HSpeed) = (-value.Sin() * speed, value.Cos() * speed);
            }
        }
        public float VGravity { get; set; }
        public float HGravity { get; set; }
        public float Gravity
        {
            get => MathF.Sqrt(VGravity * VGravity + HGravity * HGravity);
            set
            {
                (VGravity, HGravity) = (-GravityDirection.Sin() * value, GravityDirection.Cos() * value);
            }
        }
        public Angle GravityDirection
        {
            get => Angle.Atan2(VGravity, HGravity);
            set
            {
                var gravity = Gravity;
                (VGravity, HGravity) = (-value.Sin() * gravity, value.Cos() * gravity);
            }
        }

        public Game Game { get; } = game;

        public async ValueTask ExecuteStepAsync()
        {
            await OnStepAsync();

            if (Sprite is not null)
            {
                imageIndex = (imageIndex + ImageSpeed) % Sprite.ImageCount;
            }

            X += HSpeed;
            Y += VSpeed;

            HSpeed += HGravity;
            VSpeed += VGravity;
        }

        public override async ValueTask OnDrawAsync(Canvas2DContext context)
        {
            if (Sprite is not null)
                await context.DrawSpriteAsync(Sprite, ImageIndex, X, Y);
        }
    }
}
