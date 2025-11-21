using Microsoft.AspNetCore.Components;

namespace ChristmasJumpGame.Engine.Abstractions
{
    public interface IGameAsset
    {
        public string Source { get; }
        public ElementReference ElementReference { get; set; }
    }
}
