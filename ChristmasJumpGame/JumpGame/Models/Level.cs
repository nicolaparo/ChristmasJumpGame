namespace ChristmasJumpGame.JumpGame.Models
{
    public class Level
    {
        public string? Name { get; set; }

        public Dictionary<int, List<LevelTile>> Tiles { get; init; } = new();

        public int TilesWidth { get; set; }
        public int TilesHeight { get; set; }

        public Level() { }
        public Level(int tilesWidth, int tilesHeight) : this()
        {
            TilesWidth = tilesWidth;
            TilesHeight = tilesHeight;
        }

        public IEnumerable<LevelTile> GetTiles(int layer)
        {
            if (Tiles.TryGetValue(layer, out var layerTiles))
                return layerTiles;
            return [];
        }

        public IEnumerable<LevelTile> GetTiles(LevelLayer layer) => GetTiles((int)layer);

        public int GetTileAt(LevelLayer layer, int x, int y) => GetTileAt((int)layer, x, y);
        public int GetTileAt(int layer, int x, int y)
        {
            if (!Contains(x, y))
                return TileIds.None;
            return GetTiles(layer).FirstOrDefault(t => t.X == x && t.Y == y)?.Id ?? TileIds.None;
        }

        public void SetTileAt(LevelLayer layer, int x, int y, int id) => SetTileAt((int)layer, x, y, id);
        public void SetTileAt(int layer, int x, int y, int id)
        {
            if (!Contains(x, y))
                return;

            if (!Tiles.TryGetValue(layer, out var layerTiles))
            {
                layerTiles = new();
                Tiles[layer] = layerTiles;
            }

            layerTiles.RemoveAll(t => t.X == x && t.Y == y);
            layerTiles.Add(new LevelTile { X = x, Y = y, Id = id });
        }

        public bool Contains(int x, int y)
        {
            if (x < 0 || x >= TilesWidth || y < 0 || y >= TilesHeight)
                return false;

            return true;
        }

        public void Normalize()
        {
            foreach (var (layerId, tiles) in Tiles)
                tiles.RemoveAll(t => !Contains(t.X, t.Y));
        }
    }
}