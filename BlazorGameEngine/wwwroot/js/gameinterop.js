export async function executeFrame(dotnetReference, callbackFunctionName) {
    await dotnetReference.invokeMethodAsync(callbackFunctionName);
    setTimeout(() => {
        window.requestAnimationFrame(() => executeFrame(dotnetReference, callbackFunctionName));
    }, 1000 / (window.fps));
};

export function setFPS(fps) {
    window.fps = fps;
}
