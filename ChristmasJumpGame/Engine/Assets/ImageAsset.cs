using ChristmasJumpGame.Engine.Abstractions;
using Microsoft.AspNetCore.Components;
using SixLabors.ImageSharp;

namespace ChristmasJumpGame.Engine.Assets
{
    public abstract record ImageAsset : IGameAsset
    {
        public ImageAsset(string source)
        {
            var image = Image.Load($"wwwroot{source}");
            Width = image.Width;
            Height = image.Height;
            Source = source;
        }
        public ImageAsset(string source, int width, int height)
        {
            Source = source;
            Width = width;
            Height = height;
        }
        public ElementReference ElementReference { get; set; }
        public string Source { get; init; }
        public int Width { get; init; }
        public int Height { get; init; }
    }

}
