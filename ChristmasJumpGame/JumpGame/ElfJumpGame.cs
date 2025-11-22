using Blazor.Extensions.Canvas.Canvas2D;
using ChristmasJumpGame.Engine;
using ChristmasJumpGame.JumpGame.Assets;
using ChristmasJumpGame.JumpGame.Models;
using ChristmasJumpGame.JumpGame.Objects;
using ChristmasJumpGame.JumpGame.Services;

namespace ChristmasJumpGame.JumpGame
{
    public class ElfJumpGame : Game
    {
        private GameObject? player;
        private readonly LevelRepository levelRepository;
        private readonly TilesetImageAsset tileset;

        public ElfJumpGame(IServiceProvider serviceProvider, LevelRepository levelRepository, TilesetImageAsset tileset) : base(serviceProvider)
        {
            this.levelRepository = levelRepository;
            this.tileset = tileset;
            this.RoomWidth = 320 * 32;
        }

        public Level? Level { get; private set; }

        public override async ValueTask OnStartAsync()
        {
            Level = await levelRepository.LoadLevelAsync("level1");
            player = InstanceCreate<Player>();

            foreach (var layer in Level.Tiles.Keys.OrderBy(k => k))
            {
                foreach (var tile in Level.GetTiles(layer).ToArray())
                {
                    if (tile.Id == 22) // gift placeholder
                    {
                        Level.SetTileAt(layer, tile.X, tile.Y, TileIds.None);
                        InstanceCreate<Gift>(tile.X * 32, tile.Y * 32);
                    }
                }
            }
        }

        public override async ValueTask OnStepAsync()
        {
            if (player is null)
                return;

            ViewX = (int)player.X - ViewWidth / 2;

            if (ViewX < 0)
                ViewX = 0;

            if (ViewX > RoomWidth - ViewWidth)
                ViewX = RoomWidth - ViewWidth;
        }

        public override async ValueTask OnDrawAsync(Canvas2DContext context)
        {
            // blue sky background
            await context.SetFillStyleAsync("skyblue");
            await context.FillRectAsync(0, 0, RoomWidth, RoomHeight);

            if (Level is null)
                return;

            foreach (var layer in Level.Tiles.Keys.OrderBy(k => k))
            {
                foreach (var tile in Level.GetTiles(layer))
                {
                    var sx = (tile.Id % 6) * 32;
                    var sy = (tile.Id / 6) * 32;
                    var dx = tile.X * 32;
                    var dy = tile.Y * 32;

                    if (dx + 32 < ViewX || dx > ViewX + ViewWidth || dy + 32 < ViewY || dy > ViewY + ViewHeight)
                        continue;

                    await context.DrawImageAsync(tileset.ElementReference, sx, sy, 32, 32, dx, dy, 32, 32);
                }
            }
        }


        private int GetTileAt(float x, float y)
        {
            if (Level is null)
                return TileIds.None;

            return Level.GetTileAt(LevelLayer.Level, (int)x / 32, (int)y / 32);
        }

        public override bool IsSolidAt(float x, float y)
        {
            if (base.IsSolidAt(x, y))
                return true;

            var tileId = GetTileAt(x, y);
            if (tileId is TileIds.None)
                return false;

            if (tileId is 16 or 24 or 30)
                return false;

            return true;
        }
    }


    public static class JumpGameAssets
    {
        public static void RegisterJumpGameAssets(this IServiceCollection services)
        {
            services.AddGame<ElfJumpGame>();
            services.AddGameResources();
        }
    }

}