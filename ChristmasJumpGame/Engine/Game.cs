using Blazor.Extensions;
using Blazor.Extensions.Canvas.Canvas2D;
using Blazor.Extensions.Canvas.WebGL;
using ChristmasJumpGame.Engine.Abstractions;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using System.Diagnostics;

namespace ChristmasJumpGame.Engine
{
    public class Game(IServiceProvider serviceProvider) : GameEntity
    {
        public virtual ValueTask OnStartAsync() => ValueTask.CompletedTask;

        private readonly GameControllerHub controllerHub = serviceProvider.GetRequiredService<GameControllerHub>();
        private readonly List<GameObject> instances = [];

        public IEnumerable<GameObject> Instances => instances;

        public T InstanceCreate<T>() where T : GameObject
        {
            return InstanceCreate<T>(0, 0);
        }
        public T InstanceCreate<T>(float x, float y) where T : GameObject
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

        public void InstanceDestroy<T>(T instance) where T : GameObject
        {
            instances.Remove(instance);
        }

        internal async Task Execute2DAsync(Canvas2DContext context)
        {
            await OnDrawAsync(context);
            foreach (var obj in instances)
                await obj.OnDrawAsync(context);
        }
        internal async Task ExecuteFrameAsync(RenderingContext context)
        {
            (PreviousMouseX, PreviousMouseY) = (MouseX, MouseY);
            (MouseX, MouseY) = ((float)(lastMouseMoveEventArgs?.OffsetX ?? 0), (float)(lastMouseMoveEventArgs?.OffsetY ?? 0));

            var instances = this.instances.ToArray();

            if (context is Canvas2DContext canvas2DContext)
                await Execute2DAsync(canvas2DContext);

            await OnStepAsync();

            for (var i = 0; i < instances.Length; i++)
                await instances[i].ExecuteStepAsync();

            foreach (var button in mousePressed.Keys)
                previousMousePessed[button] = mousePressed[button];
            foreach (var key in keyboardPressed.Keys)
                previousKeyboardPressed[key] = keyboardPressed[key];
            controllerHub.Update();
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

        public bool MouseCheckButton(MouseButton button)
        {
            if (button == MouseButton.Any)
                return Enum.GetValues<MouseButton>().Where(b => b != MouseButton.Any).Any(MouseCheckButton);

            return mousePressed.TryGetValue((long)button, out bool pressed) && pressed;
        }
        public bool MouseCheckButtonReleased(MouseButton button)
        {
            if (button == MouseButton.Any)
                return Enum.GetValues<MouseButton>().Where(b => b != MouseButton.Any).Any(MouseCheckButtonReleased);

            if (!mousePressed.TryGetValue((long)button, out bool pressed))
                return false;

            if (!previousMousePessed.TryGetValue((long)button, out bool previousPressed))
                return false;

            return !pressed && previousPressed;
        }
        public bool MouseCheckButtonPressed(MouseButton button)
        {
            if (button == MouseButton.Any)
                return Enum.GetValues<MouseButton>().Where(b => b != MouseButton.Any).Any(MouseCheckButtonPressed);

            if (!mousePressed.TryGetValue((long)button, out bool pressed))
                return false;

            if (!previousMousePessed.TryGetValue((long)button, out bool previousPressed))
                return false;

            return !previousPressed && pressed;
        }

        public bool KeyboardCheck(string key)
        {
            return keyboardPressed.TryGetValue(key, out bool pressed) && pressed;
        }
        public bool KeyboardCheckReleased(string key)
        {
            if (!keyboardPressed.TryGetValue(key, out bool pressed))
                return false;
            if (!previousKeyboardPressed.TryGetValue(key, out bool previousPressed))
                return false;
            return !pressed && previousPressed;
        }
        public bool KeyboardCheckPressed(string key)
        {
            if (!keyboardPressed.TryGetValue(key, out bool pressed))
                return false;
            if (!previousKeyboardPressed.TryGetValue(key, out bool previousPressed))
                return false;
            return !previousPressed && pressed;
        }

        public bool ControllerInputCheck(Guid controller, string inputId) => controllerHub.InputCheck(controller, inputId);
        public bool ControllerInputCheckPressed(Guid controller, string inputId) => controllerHub.InputCheckPressed(controller, inputId);
        public bool ControllerInputCheckReleased(Guid controller, string inputId) => controllerHub.InputCheckReleased(controller, inputId);

        public bool ControllerInputCheckAny(string inputId)
        {
            foreach (var controller in controllerHub.GetControllers())
            {
                if (controllerHub.InputCheck(controller, inputId))
                    return true;
            }
            return false;
        }
        public bool ControllerInputCheckAnyPressed(string inputId)
        {
            foreach (var controller in controllerHub.GetControllers())
            {
                if (controllerHub.InputCheckPressed(controller, inputId))
                    return true;
            }
            return false;
        }
        public bool ControllerInputCheckAnyReleased(string inputId)
        {
            foreach (var controller in controllerHub.GetControllers())
            {
                if (controllerHub.InputCheckReleased(controller, inputId))
                    return true;
            }
            return false;
        }

        public IEnumerable<Guid> GetInputDevices() => controllerHub.GetControllers();



        public virtual bool IsSolidAt(float x, float y) => false;
    }
}
