using LeFauxMods.Common.Services;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewValley.Menus;
using StardewValley.Objects;

namespace LeFauxMods.CrystallineJunimoChests;

/// <inheritdoc />
internal sealed class ModEntry : Mod
{
    private static readonly string[] Names =
    [
        "Dark Blue", "Blue", "Teal", "Turquoise", "Green", "Light Green", "Yellow", "Gold", "Orange", "Red", "Maroon",
        "Pink", "Hot Pink", "Magenta", "Indigo", "Purple", "Black", "Gray", "Silver", "White"
    ];

    private ModConfig config = null!;
    private ConfigHelper<ModConfig> configHelper = null!;

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        // Init
        I18n.Init(this.Helper.Translation);
        this.configHelper = new ConfigHelper<ModConfig>(helper);
        this.config = this.configHelper.Load();

        // Events
        this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        this.Helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
    }

    private static void OnSaveLoaded(object? sender, SaveLoadedEventArgs e) =>
        Utility.ForEachItem(item =>
        {
            if (item is not Chest { QualifiedItemId: "(BC)256" } chest ||
                !chest.GlobalInventoryId.StartsWith(Constants.ModId, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var name = chest.GlobalInventoryId.Replace($"{Constants.ModId}-", string.Empty);
            var selection = Array.IndexOf(Names, name);
            if (selection < 0)
            {
                return true;
            }

            chest.playerChoiceColor.Value = DiscreteColorPicker.getColorFromSelection(selection + 1);
            return true;
        });

    private static void UpdateChest(Chest chest, int selection)
    {
        Game1.playSound("wand");
        chest.GlobalInventoryId = $"{Constants.ModId}-{Names[selection - 1]}";
        chest.playerChoiceColor.Value = DiscreteColorPicker.getColorFromSelection(selection);
        chest.Location.temporarySprites.Add(
            new TemporaryAnimatedSprite(5, (chest.TileLocation * Game1.tileSize) - new Vector2(0, 32),
                chest.playerChoiceColor.Value) { layerDepth = 1f });
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!e.Button.IsUseToolButton()
            || Game1.activeClickableMenu is not ItemGrabMenu
            {
                sourceItem: Chest { QualifiedItemId: "(BC)256" } chest,
                chestColorPicker: { visible: true } colorPicker
            })
        {
            return;
        }

        var (mouseX, mouseY) = Utility.ModifyCoordinatesForUIScale(e.Cursor.GetScaledScreenPixels()).ToPoint();
        var currentColor = chest.playerChoiceColor.Value;
        var currentSelection = colorPicker.colorSelection;

        colorPicker.receiveLeftClick(mouseX, mouseY);
        if (currentSelection == colorPicker.colorSelection)
        {
            return;
        }

        // Remove a color is "free"
        if (colorPicker.colorSelection == 0)
        {
            chest.GlobalInventoryId = "JunimoChests";
            return;
        }

        this.Helper.Input.Suppress(e.Button);
        chest.playerChoiceColor.Value = currentColor;

        // Change for free
        if (this.config.GemCost < 1)
        {
            Game1.activeClickableMenu.exitThisMenuNoSound();
            UpdateChest(chest, colorPicker.colorSelection);
            return;
        }

        // Change for pay
        var item = ItemRegistry.GetDataOrErrorItem(this.config.Items[colorPicker.colorSelection - 1]);
        if (Game1.player.Items.ContainsId(item.QualifiedItemId, this.config.GemCost))
        {
            var responses = Game1.currentLocation.createYesNoResponses();
            Game1.currentLocation.createQuestionDialogue(
                I18n.Message_Confirm(
                    this.config.GemCost,
                    item.DisplayName,
                    chest.DisplayName,
                    this.Helper.Translation.Get(
                        $"color.{Names[colorPicker.colorSelection - 1]}")),
                responses,
                (_, whichAnswer) =>
                {
                    if (whichAnswer != "Yes")
                    {
                        return;
                    }

                    Game1.player.Items.ReduceId(item.QualifiedItemId, this.config.GemCost);
                    UpdateChest(chest, colorPicker.colorSelection);
                });

            return;
        }

        // Player does not have item
        Game1.drawObjectDialogue(
            I18n.Message_Alert(
                this.config.GemCost,
                item.DisplayName,
                chest.DisplayName,
                this.Helper.Translation.Get(
                    $"color.{Names[colorPicker.colorSelection - 1]}")));
    }
}
