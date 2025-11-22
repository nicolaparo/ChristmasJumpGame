using Blazor.Extensions;
using Blazor.Extensions.Canvas;
using Blazor.Extensions.Canvas.Canvas2D;
using Blazor.Extensions.Canvas.WebGL;
using ChristmasJumpGame.Engine.Abstractions;

namespace ChristmasJumpGame.Engine
{
    public abstract class GameEntity : IGameEntity
    {
        public virtual void OnCreate() { }
        public virtual ValueTask OnStepAsync() => ValueTask.CompletedTask;
        public virtual ValueTask OnDrawAsync(Canvas2DContext context) => ValueTask.CompletedTask;

        public abstract bool MouseCheckButton(MouseButton button);
        public abstract bool MouseCheckButtonReleased(MouseButton button);
        public abstract bool MouseCheckButtonPressed(MouseButton button);

        public abstract bool KeyboardCheck(string key);
        public abstract bool KeyboardCheckReleased(string key);
        public abstract bool KeyboardCheckPressed(string key);

        public abstract IEnumerable<Guid> GetControllerInputDevices();

        public abstract bool ControllerInputCheck(Guid controller, string inputId);
        public abstract bool ControllerInputCheckPressed(Guid controller, string inputId);
        public abstract bool ControllerInputCheckReleased(Guid controller, string inputId);
        public abstract bool ControllerInputCheckAny(string inputId);
        public abstract bool ControllerInputCheckAnyPressed(string inputId);
        public abstract bool ControllerInputCheckAnyReleased(string inputId);

    }
}
