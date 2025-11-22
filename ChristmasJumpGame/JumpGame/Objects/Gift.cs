using Blazor.Extensions.Canvas.Canvas2D;
using ChristmasJumpGame.Engine;

namespace ChristmasJumpGame.JumpGame.Objects
{
    public class Gift(ElfJumpGame game, GiftSprite sprite) : GameObject(game)
    {
        private float phase = Random.Shared.NextSingle();

        public override void OnCreate()
        {
            Sprite = sprite;
            BoundingBox = new(0, 0, 32, 32);
        }

        public override async ValueTask OnStepAsync()
        {
            phase = (phase + 0.02f) % 1;
        }

        public override async ValueTask OnDrawAsync(Canvas2DContext context)
        {
            var y = MathF.Sin(phase * MathF.PI * 2) * 2;
            await context.DrawSpriteAsync(Sprite!, 0, X, Y + y);
        }
    }

}