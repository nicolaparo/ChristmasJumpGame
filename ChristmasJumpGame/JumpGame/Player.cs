using Blazor.Extensions.Canvas.Canvas2D;
using ChristmasJumpGame.Engine;

namespace ChristmasJumpGame.JumpGame
{
    public class Player(ElfJumpGame game, PlayerSprite sprite) : GameObject(game)
    {
        public override void OnCreate()
        {
            X = 2 * 32;
            Y = 2 * 32;
            Sprite = sprite;
            VAcceleration = .1f;
            BoundingBox = new(-16, -8, 32, 32);
        }
        private int direction = 1;

        public override async ValueTask OnStepAsync()
        {
            if (IsPointFree(X, Y + 1))
            {
                VAcceleration = .8f;
                if (!IsPointFree(X, Y - 1))
                {
                    MoveOutsideSolid(0, 1, .1f);
                    if (VSpeed < 0)
                        VSpeed = 0;
                }
            }
            else
            {
                MoveOutsideSolid(0, -1, .1f);
                VSpeed = 0;
            }

            if (VSpeed >= 10)
                VSpeed = 10;

            var running = KeyboardCheck(KeyboardKeys.ControlLeft);
            var speed = running ? 6 : 4;

            ImageSpeed = 0;

            if (KeyboardCheck(KeyboardKeys.ArrowLeft))
            {
                direction = -1;
                ImageSpeed = .25f;

                if (IsPointFree(X - speed, Y))
                    X -= speed;
            }

            if (KeyboardCheck(KeyboardKeys.ArrowRight))
            {
                direction = 1;
                ImageSpeed = .25f;

                if (IsPointFree(X + speed, Y))
                    X += speed;
            }

            if (ImageSpeed == 0)
                ImageIndex = 0;

            if (KeyboardCheckPressed("Space"))
                if (!IsPointFree(X, Y + 2))
                    VSpeed = running ? -15 : -12;

            if (IsPointFree(X, Y + 2))
                if (VSpeed < -3)
                    if (!KeyboardCheck("Space"))
                        VSpeed = -3;

        }

        public override async ValueTask OnDrawAsync(Canvas2DContext context)
        {
            if (Sprite is null)
                return;

            if (IsPointFree(X, Y + 2))
                await context.DrawSpriteAsync(Sprite, 1, X, Y, scaleX: direction);
            else
                await context.DrawSpriteAsync(Sprite, ImageIndex, X, Y, scaleX: direction);
        }
    }

}