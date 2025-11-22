using Blazor.Extensions.Canvas.Canvas2D;

namespace ChristmasJumpGame.Engine.Abstractions
{
    public interface IGameEntity
    {
        void OnCreate();
        ValueTask OnStepAsync();
        ValueTask OnDrawAsync(Canvas2DContext context);
    }
}
