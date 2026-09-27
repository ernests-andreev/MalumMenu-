namespace MalumMenu;

public static class OutfitPreset
{
    // Пресет внешнего вида: Мёртвый
    public static NetworkedPlayerInfo.PlayerOutfit Dead = new()
    {
        ColorId = 7,                        // Цвет: 7 (серый/призрак)
        VisorId = "visor_Scar"              // Визор: "visor_Scar" (Шрам)
    };

    // Пресет внешнего вида: Оборотень (Shapeshifter)
    public static NetworkedPlayerInfo.PlayerOutfit Shapeshifter = new()
    {
        ColorId = 0,                        // Цвет: 0 (красный)
        SkinId = "skin_screamghostface",    // Скин: "skin_screamghostface" (Крик / Призрачное лицо)
        VisorId = "visor_eliksni"           // Визор: "visor_eliksni"
    };

    // Пресет внешнего вида: Фантом (Phantom)
    public static NetworkedPlayerInfo.PlayerOutfit Phantom = new()
    {
        ColorId = 0,                        // Цвет: 0 (красный)
        HatId = "hat_screamghostface",      // Шляпа: "hat_screamghostface" (Крик)
        SkinId = "skin_screamghostface"     // Скин: "skin_screamghostface" (Крик)
    };

    // Пресет внешнего вида: Змея (Viper)
    public static NetworkedPlayerInfo.PlayerOutfit Viper = new()
    {
        ColorId = 0,                        // Цвет: 0 (красный)
        SkinId = "skin_Hazmat-Greenskin",   // Скин: "skin_Hazmat-Greenskin" (зелёный химкостюм)
        VisorId = "visor_animesunglassesVisor"  // Визор: "visor_animesunglassesVisor" (аниме-очки)
    };

    // Пресет внешнего вида: Предатель (Impostor)
    public static NetworkedPlayerInfo.PlayerOutfit Impostor = new()
    {
        ColorId = 0                         // Цвет: 0 (красный)
    };

    // Пресет внешнего вида: Трекер (Tracker)
    public static NetworkedPlayerInfo.PlayerOutfit Tracker = new()
    {
        ColorId = 10,                       // Цвет: 10 (зелёный)
        SkinId = "skin_rhm"                 // Скин: "skin_rhm"
    };

    // Пресет внешнего вида: Судья (Judge)
    public static NetworkedPlayerInfo.PlayerOutfit Judge = new()
    {
        ColorId = 10,                       // Цвет: 10 (зелёный)
        HatId = "hat_wigJudge",             // Шляпа: "hat_wigJudge" (парик судьи)
    };

    // Пресет внешнего вида: Шумовик (Noisemaker)
    public static NetworkedPlayerInfo.PlayerOutfit Noisemaker = new()
    {
        ColorId = 10,                       // Цвет: 10 (зелёный)
        HatId = "hat_pk03_Headphones"       // Шляпа: "hat_pk03_Headphones" (наушники)
    };

    // Пресет внешнего вида: Инженер (Engineer)
    public static NetworkedPlayerInfo.PlayerOutfit Engineer = new()
    {
        ColorId = 10,                       // Цвет: 10 (зелёный)
        SkinId = "skin_Mech",               // Скин: "skin_Mech" (мех)
        VisorId = "visor_D2CGoggles"        // Визор: "visor_D2CGoggles" (очки)
    };

    // Пресет внешнего вида: Учёный (Scientist)
    public static NetworkedPlayerInfo.PlayerOutfit Scientist = new()
    {
        ColorId = 10,                       // Цвет: 10 (зелёный)
        SkinId = "skin_Science",            // Скин: "skin_Science" (научный халат)
        VisorId = "visor_pk01_PaperMaskVisor"  // Визор: "visor_pk01_PaperMaskVisor" (бумажная маска)
    };

    // Пресет внешнего вида: Детектив (Detective)
    public static NetworkedPlayerInfo.PlayerOutfit Detective = new()
    {
        ColorId = 10,                       // Цвет: 10 (зелёный)
        HatId = "hat_pk05_Fedora",          // Шляпа: "hat_pk05_Fedora" (федора)
        SkinId = "skin_SuitW"               // Скин: "skin_SuitW" (костюм)
    };

    // Пресет внешнего вида: Член экипажа (Crewmate)
    public static NetworkedPlayerInfo.PlayerOutfit Crewmate = new()
    {
        ColorId = 10                        // Цвет: 10 (зелёный)
    };
}
