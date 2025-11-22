using Blazor.Extensions;
using Blazor.Extensions.Canvas;
using Blazor.Extensions.Canvas.Canvas2D;
using Blazor.Extensions.Canvas.WebGL;
using ChristmasJumpGame.Engine.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ChristmasJumpGame.Engine
{
    public abstract class GameEntity(Game game) : IGameEntity
    {
        public virtual void OnCreate() { }
        public virtual ValueTask OnStepAsync() => ValueTask.CompletedTask;
        public virtual ValueTask OnDrawAsync(Canvas2DContext context) => ValueTask.CompletedTask;

        public virtual T InstanceCreate<T>() where T : GameObject => game.InstanceCreate<T>();
        public virtual T InstanceCreate<T>(float x, float y) where T : GameObject => game.InstanceCreate<T>(x, y);
        public virtual void InstanceDestroy<T>(T instance) where T : GameObject => game.InstanceDestroy(instance);

        public virtual bool MouseCheckButton(MouseButton button) => game.MouseCheckButton(button);
        public virtual bool MouseCheckButtonReleased(MouseButton button) => game.MouseCheckButtonReleased(button);
        public virtual bool MouseCheckButtonPressed(MouseButton button) => game.MouseCheckButtonPressed(button);
        public virtual bool KeyboardCheck(string key) => game.KeyboardCheck(key);
        public virtual bool KeyboardCheckReleased(string key) => game.KeyboardCheckReleased(key);
        public virtual bool KeyboardCheckPressed(string key) => game.KeyboardCheckPressed(key);
        public virtual IEnumerable<Guid> GetControllerInputDevices() => game.GetControllerInputDevices();
        public virtual bool ControllerInputCheck(Guid controller, string inputId) => game.ControllerInputCheck(controller, inputId);
        public virtual bool ControllerInputCheckPressed(Guid controller, string inputId) => game.ControllerInputCheckPressed(controller, inputId);
        public virtual bool ControllerInputCheckReleased(Guid controller, string inputId) => game.ControllerInputCheckReleased(controller, inputId);
        public virtual bool ControllerInputCheckAny(string inputId) => game.ControllerInputCheckAny(inputId);
        public virtual bool ControllerInputCheckAnyPressed(string inputId) => game.ControllerInputCheckAnyPressed(inputId);
        public virtual bool ControllerInputCheckAnyReleased(string inputId) => game.ControllerInputCheckAnyReleased(inputId);

    }
}
