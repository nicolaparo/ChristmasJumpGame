using Blazor.Extensions.Canvas.Canvas2D;
using Blazor.Extensions.Canvas.WebGL;

namespace ChristmasJumpGame.Engine
{
    public abstract class GameEntity
    {
        public virtual void OnCreate() { }
        public virtual ValueTask OnStepAsync() => ValueTask.CompletedTask;
        public virtual ValueTask OnDrawAsync(Canvas2DContext context) => ValueTask.CompletedTask;
    }
}
