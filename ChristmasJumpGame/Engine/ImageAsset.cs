using ChristmasJumpGame.Engine.Abstractions;
using Microsoft.AspNetCore.Components;

namespace ChristmasJumpGame.Engine
{
    public abstract record ImageAsset : IImageGameAsset
    {
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
