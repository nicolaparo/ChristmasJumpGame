using Blazor.Extensions;
using Blazor.Extensions.Canvas.Canvas2D;
using Blazor.Extensions.Canvas.WebGL;
using ChristmasJumpGame.Engine;
using ChristmasJumpGame.Engine.Abstractions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace ChristmasJumpGame.Components
{

    public partial class GameView(IJSRuntime js, IEnumerable<IGameAsset> assets, ILogger<GameView> logger) : IGameView
    {
        private BECanvasComponent? canvas;
        private RenderingContext? context;
        private ElementReference canvasContainer;

        [Parameter]
        public long Width { get; set; } = 800;

        [Parameter]
        public long Height { get; set; } = 600;

        [Parameter]
        public Game? Game { get; set; }

        private readonly Lazy<Task<IJSObjectReference>> moduleTask = new(() => js.InvokeAsync<IJSObjectReference>("import", "/js/gameinterop.js").AsTask());

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                context = await canvas.CreateCanvas2DAsync();
                var reference = DotNetObjectReference.Create(this);

                if (Game is null)
                    throw new InvalidOperationException("Game property must be set.");

                await Game.OnStartAsync();

                await (await moduleTask.Value).InvokeVoidAsync("setFPS", Fps);
                await (await moduleTask.Value).InvokeVoidAsync("executeFrame", reference, nameof(OnAnimationFrameAsync));
            }
        }

        public double Fps { get; private set; } = 60;
        public async Task SetFpsAsync(double fps)
        {
            await (await moduleTask.Value).InvokeVoidAsync("setFPS", Fps);
        }

        private long frameCount = 0;

        [JSInvokable]
        public async Task OnAnimationFrameAsync()
        {
            await canvasContainer.FocusAsync();

            await context.BeginBatchAsync();

            if (context is Canvas2DContext ctx)
                await ctx.ClearRectAsync(0, 0, canvas.Width, canvas.Height);

            await Game.ExecuteFrameAsync(context);

            await context.EndBatchAsync();
            frameCount++;

        }

        private void OnMouseMove(MouseEventArgs mouseEventArgs) => Game.OnMouseMove(mouseEventArgs);
        private void OnMouseDown(MouseEventArgs mouseEventArgs) => Game.OnMouseDown(mouseEventArgs);
        private void OnMouseUp(MouseEventArgs mouseEventArgs) => Game.OnMouseUp(mouseEventArgs);
        private void OnKeyDown(KeyboardEventArgs keyboardEventArgs) => Game.OnKeyDown(keyboardEventArgs);
        private void OnKeyUp(KeyboardEventArgs keyboardEventArgs) => Game.OnKeyUp(keyboardEventArgs);
    }

}
