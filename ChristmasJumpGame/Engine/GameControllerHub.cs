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

        public async Task SendButtonUpAsync(Guid controller, string buttonId)
        {
            inputPressed[controller][buttonId] = false;
        }
        public async Task SendButtonDownAsync(Guid controller, string buttonId)
        {
            inputPressed[controller][buttonId] = true;
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

        public bool InputCheck(Guid controller, string buttonId)
        {
            return inputPressed.TryGetValue(controller, out var inputs)
                && inputs.TryGetValue(buttonId, out bool pressed) && pressed;
        }
        public bool InputCheckPressed(Guid controller, string buttonId)
        {
            if (!inputPressed.TryGetValue(controller, out var currentStates))
                return false;

            if (!previousInputPressed.TryGetValue(controller, out var previousStates))
                return false;

            if (currentStates.TryGetValue(buttonId, out bool currentPressed) && currentPressed)
                if (previousStates.TryGetValue(buttonId, out bool prevPressed) && !prevPressed)
                    return true;

            return false;
        }
        public bool InputCheckReleased(Guid controller, string buttonId)
        {
            if (!inputPressed.TryGetValue(controller, out var currentStates))
                return true;

            if (!previousInputPressed.TryGetValue(controller, out var previousStates))
                return false;

            if (currentStates.TryGetValue(buttonId, out bool currentPressed) && !currentPressed)
                if (previousStates.TryGetValue(buttonId, out bool prevPressed) && prevPressed)
                    return true;

            return false;
        }

    }
}