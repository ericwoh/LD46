using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    public JobManager _jobManager;
    public GroceryManager _grocm;
    public GroceryManagerSettings _grocmsettings;

    //
    public Critters _critters;
    public CritterSettings _critterSettings;

    // Start is called before the first frame update
    void Start()
    {
        _jobManager = new JobManager();
        _grocm = new GroceryManager(_grocmsettings);
        _critters = new Critters(_critterSettings, _jobManager);
    }

    // Update is called once per frame
    void Update()
    {
        _critters.tick(Time.deltaTime);
        _grocm.tick();
        _jobManager.tick(Time.deltaTime);
    }
}
