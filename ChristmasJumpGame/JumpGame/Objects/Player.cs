using Blazor.Extensions.Canvas.Canvas2D;
using ChristmasJumpGame.Engine;
using ChristmasJumpGame.JumpGame.Sprites;

namespace ChristmasJumpGame.JumpGame.Objects
{

    public class Player(ElfJumpGame game, PlayerSpriteAsset sprite) : GameObject(game)
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
            var running = KeyboardCheck(KeyboardKeys.ControlLeft) || ControllerInputCheckAny("RUN");
            var moveLeft = KeyboardCheck(KeyboardKeys.ArrowLeft) || ControllerInputCheckAny("LEFT");
            var moveRight = KeyboardCheck(KeyboardKeys.ArrowRight) || ControllerInputCheckAny("RIGHT");
            var jump = KeyboardCheckPressed(KeyboardKeys.Space) || ControllerInputCheckAnyPressed("JUMP");
            var holdJump = KeyboardCheck(KeyboardKeys.Space) || ControllerInputCheckAny("JUMP");

            // Gravity and vertical movement
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

            if (IsCollidingWith<Gift>(out var gifts))
            {
                foreach (var gift in gifts)
                {
                    Game.InstanceDestroy(gift);
                }
            }

            var speed = running ? 6 : 4;

            ImageSpeed = 0;

            if (moveLeft)
            {
                direction = -1;
                ImageSpeed = .25f;

                if (IsPointFree(X - speed, Y))
                    X -= speed;
            }

            if (moveRight)
            {
                direction = 1;
                ImageSpeed = .25f;

                if (IsPointFree(X + speed, Y))
                    X += speed;
            }

            if (ImageSpeed == 0)
                ImageIndex = 0;

            if (jump)
                if (!IsPointFree(X, Y + 2))
                    VSpeed = -12;

            if (IsPointFree(X, Y + 2))
                if (VSpeed < 0)
                    if (!holdJump)
                        VSpeed = 0;

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