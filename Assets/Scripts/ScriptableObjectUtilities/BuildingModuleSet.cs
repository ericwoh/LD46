using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Simple data container class for grouping sets of BuildingModules like "tilesets" for generating
/// WFC-generated buildings
/// </summary>
[CreateAssetMenu(menuName = "LD46/Building Module Set")]
public class BuildingModuleSet : RuntimeSet<BuildingModule>
{

}