# Dungeons & Developers

**Dungeons & Developers** is a Unity game project centered around managing an adventurer's guild and sending AI-controlled adventurers into dungeons.

The player prepares adventurers at the guild by recruiting, equipping, and organizing them into teams. Once a team enters a dungeon, the game shifts toward AI-driven behavior: adventurers explore rooms, respond to enemies and hazards, collect loot, and make decisions based on their class and current situation.

## Project Status

This project is currently in development for **CGDD 4242**.

Current work includes:

- Dungeon and room systems
- Adventurer movement
- Turn-based combat
- Enemy AI
- Adventurer class AI
- Loot and inventory systems
- Multi-agent/team behavior
- Reinforcement-learning experiments

## Engine

- **Unity 6**
- Project editor version: **6000.3.20f1**
- C#

## Running the Project

1. Clone or download the repository.
2. Open **Unity Hub**.
3. Choose **Add/Open Project** and select the repository folder.
4. Allow Unity to import the project.
5. Open a scene from `Assets/Scenes`.
6. Press the **Play** button at the top of the Unity Editor.

### Useful Test Scenes

The project currently contains several test scenes for individual systems. For combat and AI testing, use:

`Assets/Scenes/Combat Test.unity`

## AI Systems

The game is designed around several AI systems that control both adventurers and enemies inside the dungeon.

### Adventurer AI

Adventurers behave differently depending on their class. Their AI can use information such as health, nearby enemies, room layout, loot, allies, and available actions to decide what to do during a dungeon run.

### Enemy AI

Enemies use their own decision-making systems and behaviors. Different enemy types may prioritize attacking, stealing, protecting other enemies, retreating, or moving toward specific locations.

### Adaptive Fighter

The **Adaptive Fighter** is an adventurer-class AI that uses **Q-learning**, a reinforcement-learning algorithm.

The Fighter evaluates a simplified combat state using:

- Fighter health
- Enemy health
- Distance to the enemy

The current available actions are:

- **Attack**
- **Approach**
- **Hold**

After taking an action, the Fighter receives rewards or penalties based on what happened. These values are used to update its Q-values so that actions producing better results become more desirable over repeated encounters.

Examples include:

- Reward for damaging or defeating an enemy
- Reward for successfully moving closer
- Penalty for taking damage
- Penalty for attempting an invalid action
- Small penalty for wasting a turn

The Fighter also uses an exploration rate so that it occasionally tries actions other than its current best-known choice. This allows it to continue learning instead of always repeating the same behavior.

The implementation can be found at:

`Assets/Scripts/AdaptiveFighter.cs`

## Testing the Adaptive Fighter

1. Open `Combat Test`.
2. Select the **Adventurer** GameObject.
3. Confirm that the **Adaptive Fighter** component is attached.
4. Make sure the required movement, stats, and detector components are also attached.
5. Press **Play**.
6. Open the Unity **Console** to view the Fighter's decisions and Q-values.

During testing, the console will display information such as the Fighter's current state, selected action, exploration rate, and learned Q-values.

For easier isolated combat testing, the Adventurer's perception value can temporarily be increased so combat begins immediately.

## Team

**Dungeons & Developers**

- Bailey Peters
- Jaime McBride
- Megan Miller
- Kacy Watkins

## Repository Notes

Unity automatically creates a number of local/generated folders that should not be committed to source control, including folders such as `Library`, `Temp`, `Logs`, and `Obj`.

When committing Unity changes, include relevant files inside `Assets`, `Packages`, and `ProjectSettings`, along with their associated `.meta` files.
