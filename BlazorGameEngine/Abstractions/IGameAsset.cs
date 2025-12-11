using Microsoft.AspNetCore.Components;

namespace BlazorGameEngine.Abstractions
{
    public interface IGameAsset
    {
        public string Source { get; }
        public ElementReference ElementReference { get; set; }
    }
}
