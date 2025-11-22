using ChristmasJumpGame.Extensions;
using ChristmasJumpGame.JumpGame.Models;
using ChristmasJumpGame.JumpGame.Services;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace ChristmasJumpGame.Components.Pages
{
    public partial class LevelEditor(LevelRepository repository, IJSRuntime js)
    {
        private Level level = new(80, 10);

        private int selectedTileIndex = -1;
        private LevelLayer selectedLayer = LevelLayer.Level;

        private bool showGrid = true;

        private void OnLevelTileClicked(int x, int y)
        {
            level.SetTileAt(selectedLayer, x, y, selectedTileIndex);
        }
        private void OnTileSelected(int index)
        {
            selectedTileIndex = index;
        }

        private void OnShowGridClicked() => showGrid = !showGrid;
        private async Task OnNewLevelClickedAsync()
        {
            if (await js.ConfirmAsync("Are you sure you want to create a new level? Unsaved changes will be lost."))
            {
                level = new Level(80, 10);
            }
        }
        private string selectedLevelToLoad;
        private async Task OnLoadLevelClickedAsync()
        {
            if (!string.IsNullOrWhiteSpace(selectedLevelToLoad))
            {
                if (await js.ConfirmAsync($"Are you sure you want to load {selectedLevelToLoad}? Unsaved changes will be lost."))
                    level = await repository.LoadLevelAsync(selectedLevelToLoad);
            }
        }
        private async Task OnSaveLevelClickedAsync()
        {
            if (string.IsNullOrWhiteSpace(level.Name))
            {
                await js.AlertAsync("Please enter a level name before saving.");
                return;
            }

            await repository.SaveLevelAsync(level);

            await js.AlertAsync("Level saved successfully.");
        }
    }
}