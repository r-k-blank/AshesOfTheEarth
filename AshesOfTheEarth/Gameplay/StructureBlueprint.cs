using System.Collections.Generic;

public class StructureBlueprint
{
    public string StructureId { get; private set; }
    public string DisplayName { get; private set; }
    public Dictionary<string, int> RequiredMaterials { get; private set; }
    public int PlacementWidthTiles { get; private set; } // Dimensiuni pentru plasare
    public int PlacementHeightTiles { get; private set; }
    // Alte proprietăți: textura pentru preview, entitatea rezultată etc.

    public StructureBlueprint(string id, string name, Dictionary<string, int> materials, int width = 1, int height = 1)
    {
        StructureId = id;
        DisplayName = name;
        RequiredMaterials = materials ?? new Dictionary<string, int>();
        PlacementWidthTiles = width;
        PlacementHeightTiles = height;
    }
}
// Clasă simplă pentru un plan de construcție


// Builder Pattern ar putea fi folosit pentru a construi entități complexe de structuri
/*
public interface IStructureBuilder
{
    void Reset();
    void SetFoundation(string material);
    void AddWall(string material);
    void AddRoof(string material);
    Entity GetResult();
}
public class WoodenHouseBuilder : IStructureBuilder { ... }
*/