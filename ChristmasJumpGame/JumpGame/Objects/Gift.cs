using Blazor.Extensions.Canvas.Canvas2D;
using ChristmasJumpGame.Engine;
using ChristmasJumpGame.JumpGame.Sprites;

namespace ChristmasJumpGame.JumpGame.Objects
{
    public class Gift(ElfJumpGame game, GiftSpriteAsset sprite) : GameObject(game)
    {
        private float floatAnimationStep = Random.Shared.NextSingle();

        public override void OnCreate()
        {
            Sprite = sprite;
            BoundingBox = new(0, 0, 32, 32);
        }

        public override async ValueTask OnStepAsync()
        {
            floatAnimationStep = (floatAnimationStep + 0.02f) % 1;
        }

        public override async ValueTask OnDrawAsync(Canvas2DContext context)
        {
            var y = MathF.Sin(floatAnimationStep * MathF.PI * 2) * 2;
            await context.DrawSpriteAsync(Sprite!, 0, X, Y + y);
        }
    }

}