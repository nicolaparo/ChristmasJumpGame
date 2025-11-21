using Blazor.Extensions.Canvas.Canvas2D;
using ChristmasJumpGame.Engine;

namespace ChristmasJumpGame.JumpGame
{
    public class Level(int tilesWidth, int tilesHeight)
    {
        public Tile[][] Tiles { get; } = Enumerable.Range(0, tilesHeight)
            .Select(_ => Enumerable.Range(0, tilesWidth).Select(_ => Tile.Block).ToArray())
            .ToArray();

        public int TilesWidth { get; } = tilesWidth;
        public int TilesHeight { get; } = tilesHeight;

        public Tile GetTileAt(int x, int y)
        {
            if (x < 0 || x >= TilesWidth || y < 0 || y >= TilesHeight)
                return Tile.Block;
            return Tiles[y][x];
        }

        public void SetTileAt(int x, int y, Tile tile)
        {
            if (x < 0 || x >= TilesWidth || y < 0 || y >= TilesHeight)
                return;
            Tiles[y][x] = tile;
        }
    }
    public enum Tile
    {
        None,
        Block,
    }

    public record BoxImage() : ImageAsset("/res/box.png", 32, 32);

    public class ElfJumpGame(IServiceProvider serviceProvider
        , BoxImage boxImage) : Game(serviceProvider)
    {
        private long hue = 0;

        private string level = """
            #####################
            #                   #
            #                   #
            #                   #
            #####################
            #####################
            #####################
            #####################
            #####################
            #####################
            #####################
            """;

        private GameObject? player;

        public override async ValueTask OnStartAsync()
        {

            player = InstanceCreate<Player>();
        }

        public override async ValueTask OnDrawAsync(Canvas2DContext context)
        {
            await context.SetFillStyleAsync($"hsl({hue++}, 100%, 50%)");
            await context.FillRectAsync(0, 0, 800, 600);

            var rows = level.Split('\n');

            for (int y = 0; y < rows.Length; y++)
            {
                var row = rows[y];
                for (int x = 0; x < row.Length; x++)
                {
                    var ch = row[x];
                    if (ch == '#')
                    {
                        await context.DrawImageAsync(boxImage, x * 32, y * 32);
                    }
                }
            }

            await context.SetFillStyleAsync("white");
            await context.FillRectAsync(MouseX - 20, MouseY - 20, 40, 40);

            hue %= 360;
        }
    }

    public static class KeyboardKey
    {
        public const string ArrowLeft = "ArrowLeft";
        public const string ArrowRight = "ArrowRight";
        public const string ArrowUp = "ArrowUp";
        public const string ArrowDown = "ArrowDown";
        public const string Space = "Space";
    }

    public record PlayerSprite() : SpriteAsset("/res/box.png", 32, 32);

    public class Player(ElfJumpGame game, PlayerSprite sprite) : GameObject(game)
    {
        public override void OnCreate()
        {
            X = 800 / 2;
            Y = 600 / 2;
            Sprite = sprite;
        }

        public override async ValueTask OnStepAsync()
        {

            if (game.KeyboardCheck(KeyboardKey.ArrowLeft))
                X--;

            if (game.KeyboardCheck(KeyboardKey.ArrowRight))
                X++;

            if (game.KeyboardCheck(KeyboardKey.ArrowUp))
                Y--;

            if (game.KeyboardCheck(KeyboardKey.ArrowDown))
                Y++;

            if (game.KeyboardCheckPressed("Space"))
            {
                VSpeed = -5;
            }
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