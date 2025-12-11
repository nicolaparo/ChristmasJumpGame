using Blazor.Extensions.Canvas.Canvas2D;
using BlazorGameEngine;
using BlazorGameEngine.Assets;

namespace DemoGame
{
    public class Demo(IServiceProvider services) : Game(services)
    {
        public override async ValueTask OnDrawAsync(Canvas2DContext context)
        {
            
        }
    }
}
