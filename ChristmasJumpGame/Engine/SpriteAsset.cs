using ChristmasJumpGame.Engine.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace ChristmasJumpGame.Engine
{

    public abstract record SpriteAsset : ImageAsset, IGameAsset
    {
        public SpriteAsset(string source, int width, int height, int imageCount, int imagesPerRow, int originX = 0, int originY = 0, int xGap = 0, int yGap = 0, int xOffset = 0, int yOffset = 0)
            : base(source, width, height)
        {
            ImageCount = imageCount;
            ImagesPerRow = imagesPerRow;
            OriginX = originX;
            OriginY = originY;
            XGap = xGap;
            YGap = yGap;
            XOffset = xOffset;
            YOffset = yOffset;
        }
        public SpriteAsset(string source, int width, int height, int imageCount, int imagesPerRow) : this(source, width, height, imageCount, imagesPerRow, 0, 0, 0, 0, 0, 0) { }
        public SpriteAsset(string source, int width, int height, int imageCount) : this(source, width, height, imageCount, 1) { }
        public SpriteAsset(string source, int width, int height) : this(source, width, height, 1) { }

        public int ImageCount { get; init; }
        public int ImagesPerRow { get; init; }
        public int OriginX { get; init; }
        public int OriginY { get; init; }
        public int XGap { get; init; }
        public int YGap { get; init; }
        public int XOffset { get; init; }
        public int YOffset { get; init; }
    }

}
