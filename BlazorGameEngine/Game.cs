using Blazor.Extensions;
using Blazor.Extensions.Canvas.Canvas2D;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorGameEngine
{
    public abstract class Game(IServiceProvider serviceProvider) : GameEntity(null!)
    {
        public virtual ValueTask OnStartAsync() => ValueTask.CompletedTask;

        private readonly GameControllerHub? controllerHub = serviceProvider.GetService<GameControllerHub>();
        private readonly List<GameObject> instances = [];

        public IEnumerable<GameObject> Instances => instances;

        public sealed override T InstanceCreate<T>()
        {
            return InstanceCreate<T>(0, 0);
        }
        public sealed override T InstanceCreate<T>(float x, float y)
        {
            var instance = ActivatorUtilities.CreateInstance<T>(serviceProvider, this);
            typeof(T).GetProperty(nameof(GameObject.OriginalX))!.SetValue(instance, x);
            typeof(T).GetProperty(nameof(GameObject.OriginalY))!.SetValue(instance, y);
            instance.X = x;
            instance.Y = y;
            instances.Add(instance);
            instance.OnCreate();
            return instance;
        }
        public sealed override void InstanceDestroy<T>(T instance)
        {
            instances.Remove(instance);
        }

        internal async Task ExecuteFrameAsync(RenderingContext context)
        {
            (PreviousMouseX, PreviousMouseY) = (MouseX, MouseY);
            (MouseX, MouseY) = ((float)(lastMouseMoveEventArgs?.OffsetX ?? 0), (float)(lastMouseMoveEventArgs?.OffsetY ?? 0));

            var instances = this.instances.ToArray();

            if (context is Canvas2DContext canvas2DContext)
            {
                await OnDrawAsync(canvas2DContext);
                foreach (var obj in instances)
                    await obj.OnDrawAsync(canvas2DContext);
            }

            await OnStepAsync();

            foreach (var obj in instances)
                await obj.ExecuteStepAsync();

            foreach (var button in mousePressed.Keys)
                previousMousePessed[button] = mousePressed[button];
            foreach (var key in keyboardPressed.Keys)
                previousKeyboardPressed[key] = keyboardPressed[key];

            controllerHub?.Update();
        }

        public TimeSpan DeltaTime { get; private set; }

        public int InstanceCount => instances.Count;

        public float MouseX { get; private set; }
        public float MouseY { get; private set; }
        public float PreviousMouseX { get; private set; }
        public float PreviousMouseY { get; private set; }

        public int RoomWidth { get; init; } = 480;
        public int RoomHeight { get; init; } = 320;

        public int ViewWidth { get; set; } = 480;
        public int ViewHeight { get; set; } = 320;
        public int ViewX { get; set; }
        public int ViewY { get; set; }

        private MouseEventArgs? lastMouseMoveEventArgs;

        private readonly Dictionary<long, bool> mousePressed = new();
        private readonly Dictionary<long, bool> previousMousePessed = new();

        internal void OnMouseMove(MouseEventArgs mouseEventArgs) => lastMouseMoveEventArgs = mouseEventArgs;
        internal void OnMouseDown(MouseEventArgs mouseEventArgs) => mousePressed[mouseEventArgs.Button] = true;
        internal void OnMouseUp(MouseEventArgs mouseEventArgs) => mousePressed[mouseEventArgs.Button] = false;

        private readonly Dictionary<string, bool> keyboardPressed = new();
        private readonly Dictionary<string, bool> previousKeyboardPressed = new();
        internal void OnKeyDown(KeyboardEventArgs keyboardEventArgs)
        {
            keyboardPressed[keyboardEventArgs.Code] = true;
        }
        internal void OnKeyUp(KeyboardEventArgs keyboardEventArgs)
        {
            keyboardPressed[keyboardEventArgs.Code] = false;
        }

        public sealed override bool MouseCheckButton(MouseButton button)
        {
            if (button == MouseButton.Any)
                return Enum.GetValues<MouseButton>().Where(b => b != MouseButton.Any).Any(MouseCheckButton);

            return mousePressed.TryGetValue((long)button, out bool pressed) && pressed;
        }
        public sealed override bool MouseCheckButtonReleased(MouseButton button)
        {
            if (button == MouseButton.Any)
                return Enum.GetValues<MouseButton>().Where(b => b != MouseButton.Any).Any(MouseCheckButtonReleased);

            if (!mousePressed.TryGetValue((long)button, out bool pressed))
                return false;

            if (!previousMousePessed.TryGetValue((long)button, out bool previousPressed))
                return false;

            return !pressed && previousPressed;
        }
        public sealed override bool MouseCheckButtonPressed(MouseButton button)
        {
            if (button == MouseButton.Any)
                return Enum.GetValues<MouseButton>().Where(b => b != MouseButton.Any).Any(MouseCheckButtonPressed);

            if (!mousePressed.TryGetValue((long)button, out bool pressed))
                return false;

            if (!previousMousePessed.TryGetValue((long)button, out bool previousPressed))
                return false;

            return !previousPressed && pressed;
        }

        public sealed override bool KeyboardCheck(string key)
        {
            return keyboardPressed.TryGetValue(key, out bool pressed) && pressed;
        }
        public sealed override bool KeyboardCheckReleased(string key)
        {
            if (!keyboardPressed.TryGetValue(key, out bool pressed))
                return false;
            if (!previousKeyboardPressed.TryGetValue(key, out bool previousPressed))
                return false;
            return !pressed && previousPressed;
        }
        public sealed override bool KeyboardCheckPressed(string key)
        {
            if (!keyboardPressed.TryGetValue(key, out bool pressed))
                return false;
            if (!previousKeyboardPressed.TryGetValue(key, out bool previousPressed))
                return false;
            return !previousPressed && pressed;
        }

        public sealed override bool ControllerInputCheck(Guid controller, string inputId) => controllerHub is null ? false : controllerHub.ControllerInputCheck(controller, inputId);
        public sealed override bool ControllerInputCheckPressed(Guid controller, string inputId) => controllerHub is null ? false : controllerHub.ControllerInputCheckPressed(controller, inputId);
        public sealed override bool ControllerInputCheckReleased(Guid controller, string inputId) => controllerHub is null ? false : controllerHub.ControllerInputCheckReleased(controller, inputId);

        public sealed override bool ControllerInputCheckAny(string inputId)
        {
            if (controllerHub is null)
                return false;

            foreach (var controller in controllerHub.GetControllers())
            {
                if (controllerHub.ControllerInputCheck(controller, inputId))
                    return true;
            }
            return false;
        }
        public sealed override bool ControllerInputCheckAnyPressed(string inputId)
        {
            if (controllerHub is null)
                return false;

            foreach (var controller in controllerHub.GetControllers())
            {
                if (controllerHub.ControllerInputCheckPressed(controller, inputId))
                    return true;
            }
            return false;
        }
        public sealed override bool ControllerInputCheckAnyReleased(string inputId)
        {
            if (controllerHub is null)
                return false;

            foreach (var controller in controllerHub.GetControllers())
            {
                if (controllerHub.ControllerInputCheckReleased(controller, inputId))
                    return true;
            }
            return false;
        }

        public sealed override IEnumerable<Guid> GetControllerInputDevices() => controllerHub?.GetControllers() ?? [];

        public virtual bool IsSolidAt(float x, float y) => false;
    }
}
