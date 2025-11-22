using ChristmasJumpGame.Components.Pages;

namespace ChristmasJumpGame.Engine
{
    public class GameControllerHub
    {
        private readonly Dictionary<Guid, Dictionary<string, bool>> previousInputPressed = new();
        private readonly Dictionary<Guid, Dictionary<string, bool>> inputPressed = new();

        public IEnumerable<Guid> GetControllers()
        {
            return inputPressed.Keys;
        }

        public void RegisterController(Guid controller)
        {
            inputPressed.Add(controller, new());
        }
        public void UnregisterController(Guid controller)
        {
            inputPressed.Remove(controller);
        }

        public async Task SendControllerInputUpAsync(Guid controller, string inputId)
        {
            inputPressed[controller][inputId] = false;
        }
        public async Task SendControllerInputDownAsync(Guid controller, string inputId)
        {
            inputPressed[controller][inputId] = true;
        }

        public void Update()
        {
            foreach (var controller in inputPressed.Keys)
            {
                if (!previousInputPressed.ContainsKey(controller))
                    previousInputPressed[controller] = new();
                var previousStates = previousInputPressed[controller];
                var currentStates = inputPressed[controller];

                foreach (var buttonId in currentStates.Keys)
                    previousStates[buttonId] = currentStates[buttonId];
            }
        }

        public bool ControllerInputCheck(Guid controller, string inputId)
        {
            return inputPressed.TryGetValue(controller, out var inputs)
                && inputs.TryGetValue(inputId, out bool pressed) && pressed;
        }
        public bool ControllerInputCheckPressed(Guid controller, string inputId)
        {
            if (!inputPressed.TryGetValue(controller, out var currentStates))
                return false;

            if (!previousInputPressed.TryGetValue(controller, out var previousStates))
                return false;

            if (currentStates.TryGetValue(inputId, out bool currentPressed) && currentPressed)
                if (previousStates.TryGetValue(inputId, out bool prevPressed) && !prevPressed)
                    return true;

            return false;
        }
        public bool ControllerInputCheckReleased(Guid controller, string inputId)
        {
            if (!inputPressed.TryGetValue(controller, out var currentStates))
                return true;

            if (!previousInputPressed.TryGetValue(controller, out var previousStates))
                return false;

            if (currentStates.TryGetValue(inputId, out bool currentPressed) && !currentPressed)
                if (previousStates.TryGetValue(inputId, out bool prevPressed) && prevPressed)
                    return true;

            return false;
        }

    }
}