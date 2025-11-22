using ChristmasJumpGame.JumpGame.Models;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace ChristmasJumpGame.JumpGame.Services
{
    public class LevelRepository
    {
        private readonly DirectoryInfo levelFolder;

        public LevelRepository(DirectoryInfo levelFolder)
        {
            this.levelFolder = levelFolder;
            levelFolder.Create();
        }

        public IEnumerable<string> GetLevels()
        {
            return levelFolder.GetFiles("*.lvl.json")
                .Select(f => f.Name[..^9]);
        }

        public async Task SaveLevelAsync(Level level)
        {
            var filePath = Path.Combine(levelFolder.FullName, $"{level.Name}.lvl.json");
            var json = JsonSerializer.Serialize(level);
            await File.WriteAllTextAsync(filePath, json);
        }
        public async Task<Level> LoadLevelAsync(string levelId)
        {
            var filePath = Path.Combine(levelFolder.FullName, $"{levelId}.lvl.json");
            var json = await File.ReadAllTextAsync(filePath);
            var result = JsonSerializer.Deserialize<Level>(json);

            return result;
        }
    }
}