<table width="100%">
  <tr>
    <td width="50%">
      <img width="100%" alt="486676140-2071763e-3804-45ee-ba17-01b417498cbf" src="https://github.com/user-attachments/assets/2bf6ccfc-a5f2-4568-9cc1-fe00ff61694e" />
      <img width="100%" alt="486676140-2071763e-3804-45ee-ba17-01b417498cbf" src="https://github.com/user-attachments/assets/ad21ca94-69e0-4476-97a9-7c6e21696ecd" />
    </td>
    <td width="50%">
      <img width="100%" alt="486676140-2071763e-3804-45ee-ba17-01b417498cbf" src="https://github.com/user-attachments/assets/1b0dbd79-0b9b-4580-8640-670d612110fc" />
      <img width="100%" alt="486676140-2071763e-3804-45ee-ba17-01b417498cbf" src="https://github.com/user-attachments/assets/25ced21e-fafd-4691-aeae-e7de8d338e74" />
    </td>
</table>

## ℹ️ About
Structor is a 2D pixel art strategy builder game where the player needs to build a circuit with a set amount of cards. In the game, player are tasked to keep up with increasing quotas by buying different cards, getting upgrades, and strategizing the wire configuration that will yield the highest points.

## 🎮 Controls
Drag cards into the grid to place down tiles

Press the attack button (on the left) to fire beams from CPUs.

Drag cards into the discard (on the right) to trash the card, then press the discard to get new cards.

##  📜 Scripts

|  Scripts | Description |
| --- | --- |
| `GameManager.cs` | Keeps track of damage, quota, attacks, discards, money, and level indexes. This script is also responsible for setting up (calculating quota, setting up debuffs) and cleaning up the battle scene |
| `ShopManager.cs` | Generates the random item and updates all the UI components in the shop panel |
| `GridManager.cs`  | Manages the spawning and setup for each `GridObject.cs` that is played. Keeps track of each object in the grid to call properly. Is also responsible for firing beams from all tracked objects |
| `GridObject.cs`  | The script that each gameobject has that keeps track of what tile is occupying the space, and is also responsible for transferring the damage beams while applying their own damage boost |
| `DragableCard.cs`  | Handles almost all card logic, including animation, dragging, and the execution of cards |
| `CardManager.cs`| Manages the cards in hand (positioning and calling animations) and card draws |
| `CardAnimationManager.cs` | Where the card hand animations are executed like when drawing cards, the cards in hand fan out |
| `CardHoverEffect.cs` | Placed in card object, handles calls from `CardAnimationManager.cs` for individual cards|
| `TrashCanUI.cs`| Handles card discard logic |
| `SFXManager.cs`| Static instance that creates a audiosource with selected clip |


|Scriptable Objects | Desciption|
| --- | --- |
|`CardData.cs`| Contains card type, damage numbers, and booster number. This SO is also used in `GridObject.cs` as the base data, and for each object we copy the SO data and create a new instance so we can have seperate data per grid object|
|`DeckConfiguration.cs`| `DeckConfiguration.cs` is a list of `CardData.cs` which is the startinng deck. This SO contains the name and description of each deck, along with data for what type of object will be in the middle of the grid on the start of the game.|

## 👤 Contributors
Nhoel M. K. D. Goei - Game Designer

Steven Wijaya (Me) - Game Programmer

Leonardi - 2D Game Artist
