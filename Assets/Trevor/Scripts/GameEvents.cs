using System;

public static class GameEvents
{
    // Define all your game's broadcastable events here
    public static Action OnPetJump;
    public static Action OnBonusScored;
    public static Action OnGameOver;
    public static Action OnGameCountdown;

    // You can add more later for UI, like:
    // public static Action OnUIButtonClicked;
}