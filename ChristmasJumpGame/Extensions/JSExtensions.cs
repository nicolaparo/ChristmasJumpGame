using Microsoft.JSInterop;

namespace ChristmasJumpGame.Extensions
{
    public static class JSExtensions
    {
        public static ValueTask AlertAsync(this IJSRuntime js, string message)
        {
            return js.InvokeVoidAsync("alert", message);
        }
        public static ValueTask<string> PromptAsync(this IJSRuntime js, string message, string defaultValue = "")
        {
            return js.InvokeAsync<string>("prompt", message, defaultValue);
        }
        public static ValueTask<bool> ConfirmAsync(this IJSRuntime js, string message)
        {
            return js.InvokeAsync<bool>("confirm", message);
        }
    }
}