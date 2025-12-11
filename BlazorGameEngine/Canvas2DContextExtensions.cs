using Blazor.Extensions.Canvas.Canvas2D;
using BlazorGameEngine.Assets;

namespace BlazorGameEngine
{
    public static class Canvas2DContextExtensions
    {
        public static async ValueTask DrawSpriteAsync(this Canvas2DContext context, SpriteAsset sprite, int imageIndex, double x, double y, double width, double height)
        {
            var imageX = (imageIndex % sprite.ImagesPerRow) * (sprite.Width + sprite.XGap) + sprite.XOffset;
            var imageY = (imageIndex / sprite.ImagesPerRow) * (sprite.Height + sprite.YGap) + sprite.YOffset;

            await context.DrawImageAsync(sprite.ElementReference
                , imageX
                , imageY
                , sprite.Width
                , sprite.Height
                , x - sprite.OriginX, y - sprite.OriginY, width, height);
        }
        public static async ValueTask DrawSpriteAsync(this Canvas2DContext context, SpriteAsset sprite, int imageIndex, double x, double y)
        {
            await context.DrawSpriteAsync(sprite, imageIndex, x, y, sprite.Width, sprite.Height);
        }
        public static async ValueTask DrawSpriteAsync(this Canvas2DContext context, SpriteAsset sprite, int imageIndex, double x, double y, double width, double height, double scaleX = 1, double scaleY = 1, Angle rotation = default)
        {
            if (scaleX is 1 && scaleY is 1 && rotation == default)
            {
                await context.DrawSpriteAsync(sprite, imageIndex, x, y, width, height);
                return;
            }

            await context.SetTransformAsync(1, 0, 0, 1, 0, 0);
            await context.TranslateAsync(x, y);

            if (scaleX is not 1 || scaleY is not 1)
                await context.ScaleAsync(scaleX, scaleY);

            if (rotation != default)
                await context.RotateAsync((float)rotation.ToRadians());

            await context.DrawSpriteAsync(sprite, imageIndex, 0, 0, width, height);
            await context.SetTransformAsync(1, 0, 0, 1, 0, 0);
        }
        public static async ValueTask DrawSpriteAsync(this Canvas2DContext context, SpriteAsset sprite, int imageIndex, double x, double y, double scaleX = 1, double scaleY = 1, Angle rotation = default)
        {
            await context.DrawSpriteAsync(sprite, imageIndex, x, y, sprite.Width, sprite.Height, scaleX, scaleY, rotation);
        }

        public static async ValueTask DrawImageAsync(this Canvas2DContext context, ImageAsset image, double x, double y, double width, double height)
        {
            await context.DrawImageAsync(image.ElementReference
                , 0, 0, image.Width, image.Height
                , x, y, width, height);
        }
        public static async ValueTask DrawImageAsync(this Canvas2DContext context, ImageAsset image, double x, double y)
        {
            await context.DrawImageAsync(image, x, y, image.Width, image.Height);
        }
        public static async ValueTask DrawImageAsync(this Canvas2DContext context, ImageAsset image, double x, double y, double width, double height, double scaleX = 1, double scaleY = 1, Angle rotation = default)
        {
            if (scaleX is 1 && scaleY is 1 && rotation == default)
            {
                await context.DrawImageAsync(image, x, y, width, height);
                return;
            }

            await context.SetTransformAsync(1, 0, 0, 1, 0, 0);
            await context.TranslateAsync(x, y);

            if (scaleX is not 1 || scaleY is not 1)
                await context.ScaleAsync(scaleX, scaleY);

            if (rotation != default)
                await context.RotateAsync((float)rotation.ToRadians());

            await context.DrawImageAsync(image, 0, 0, width, height);
            await context.SetTransformAsync(1, 0, 0, 1, 0, 0);
        }
        public static async ValueTask DrawImageAsync(this Canvas2DContext context, ImageAsset image, double x, double y, double scaleX = 1, double scaleY = 1, Angle rotation = default)
        {
            await context.DrawImageAsync(image, x, y, image.Width, image.Height, scaleX, scaleY, rotation);
        }

    }
}
