namespace LeFauxMods.CrystallineJunimoChests.Models;

internal sealed class ModConfig
{
    /// <summary>Gets or sets the amount of gems required to change the color.</summary>
    public int GemCost { get; set; } = 1;

    /// <summary>Gets or sets the items required to change the color.</summary>
    public List<string> Items { get; set; } =

    [
        "(O)550", "(O)72", "(O)84", "(O)62", "(O)60", "(O)70", "(O)68", "(O)86", "(O)554", "(O)82", "(O)64", "(O)578",
        "(O)848", "(O)575", "(O)577", "(O)66", "(O)382", "(O)539", "(O)571", "(O)80"
    ];
}
