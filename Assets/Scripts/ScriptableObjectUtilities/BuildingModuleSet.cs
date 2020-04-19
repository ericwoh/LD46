using UnityEngine;

/// <summary>
/// Simple data container class for grouping sets of BuildingModules like "tilesets" for generating
/// WFC-generated buildings
/// </summary>
[CreateAssetMenu(menuName = "LD46/Building Module Set")]
public class BuildingModuleSet : RuntimeSet<BuildingModule>
{
    [Tooltip("Door module is special since all buildings should start with a door")]
    public BuildingModule mDoorModule;
}