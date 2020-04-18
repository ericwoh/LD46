using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    public JobManager _jobManager;

    //
    public Critters _critters;
    public CritterSettings _critterSettings;

    // Start is called before the first frame update
    void Start()
    {
        _jobManager = new JobManager();
        _critters = new Critters(_critterSettings, _jobManager);
    }

    // Update is called once per frame
    void Update()
    {
        _critters.tick(Time.deltaTime);
        _jobManager.tick(Time.deltaTime);
    }
}
