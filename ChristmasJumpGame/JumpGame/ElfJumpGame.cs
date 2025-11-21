using Blazor.Extensions.Canvas.Canvas2D;
using ChristmasJumpGame.Engine;

namespace ChristmasJumpGame.JumpGame
{
    public record BoxImage() : ImageAsset("/res/box.png", 32, 32);
    public record PlayerSprite() : SpriteAsset("/res/elf-green.png", 48, 48, 4, 4, 24, 24);
    public record TilesetImage() : ImageAsset("/res/tileset.png");

    public class ElfJumpGame : Game
    {
        private GameObject? player;
        private readonly LevelRepository levelRepository;
        private readonly TilesetImage tileset;

        public ElfJumpGame(IServiceProvider serviceProvider, LevelRepository levelRepository, TilesetImage tileset) : base(serviceProvider)
        {
            this.levelRepository = levelRepository;
            this.tileset = tileset;
            this.RoomWidth = 4800;
        }

        public Level Level { get; private set; }

        public override async ValueTask OnStartAsync()
        {
            Level = await levelRepository.LoadLevelAsync("level1");
            player = InstanceCreate<Player>();
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
            return Level.GetTileAt(LevelLayer.Level, (int)x / 32, (int)y / 32);
        }

        public override bool IsSolidAt(float x, float y)
        {
            if (base.IsSolidAt(x, y))
                return true;

            return GetTileAt(x, y) != TileIds.None;
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