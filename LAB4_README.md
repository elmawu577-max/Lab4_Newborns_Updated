# Lab 4 Newborns - implemented checkoff features

The project creates the Lab 4 manager and menu UI automatically at runtime.
No scene object or Inspector wiring is required for the new systems.

## Scene flow

1. `MainMenu` (build index 0)
2. `LoadingScene` (build index 1)
3. `World-1-1` (build index 2)
4. `World-1-2` (build index 3)

Main Menu includes Start Game and Reset High Score. Loading Scene includes
Enter World 1-1 and Back to Main Menu.

## Checkoff mapping

- Feedback: background music plus pause, power-up spawn/collect, Goomba stomp,
  and Game Over sounds.
- Multiple scenes: use Start Game, Enter World 1-1, and Next World.
- Persistence: `LabGameManager` is a singleton and uses `DontDestroyOnLoad`.
  Current score carries into World 1-2. High score uses PlayerPrefs, survives
  Editor restarts, and can be reset from Main Menu.
- Power-ups: question boxes alternate between Coin (+1 score) and Mushroom
  (temporary speed boost). Only a hit on the bottom face activates a box.
- Restart: the existing Restart button calls `RestartCurrentScene`, resets the
  current score, restarts music, and reloads the scene so Goombas and power-ups
  return to their initial state.
- ScriptableObject: `Assets/Resources/GameStateData.asset` stores shared score,
  power-up tuning, and audio references.

## Controls

- A/D: move
- Space: jump
- P or Escape: pause/resume
- Runtime buttons: pause, next world, and main menu

## Fast test order

1. Open `MainMenu` and press Play.
2. Start Game -> verify Loading Scene -> Back to Main Menu.
3. Start again -> Enter World 1-1.
4. Press P twice and listen for music stopping/starting.
5. Hit question boxes from below until both Coin and Mushroom appear. Jump on
   top of a fresh box once to show it does not activate.
6. Stomp a Goomba and confirm score/sound.
7. Press Next World and confirm the score is unchanged in World 1-2.
8. Lose to a Goomba, listen for Game Over, then Restart. Confirm score is zero,
   Goombas/power-ups have reset, and music plays again.
9. Return to Main Menu and test Reset High Score.

## Recording reminder

Keep the recording under five minutes and record audio. Briefly show
`GameStateData.asset` in the Inspector or `GameStateData.cs` in the code editor
to demonstrate ScriptableObject usage.
