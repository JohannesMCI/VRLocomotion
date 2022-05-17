using HP.Omnicept.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public partial class Logic : MonoBehaviour
{
    public PathDefinition.Path Path;
    public StateMachineStudy.Process StateMachine { get; set; }
    public GameObject xRRig;
    public GameObject environment;
    public GameObject MainCamera;

    public float riseLandMarkRate = 1; 
    public float riseMazeRate = 1;
    public float landMarkHeigth = -0.1f;
    public float mazeHeight = -0.1f;
    public float mazeHeightStore = -4f;

    public float WalkTimeMin = 10;
    public GameObject gliaObject;
    public GameObject feelGoodPrefab;
    public GameObject leeds;
    public GameObject confidencePrefab;
    public GameObject eyetrackerRayCaster;
    public GameObject walkplane;
    public string participantId;
    public Enums.LocomotionType LocomotionType;
    internal float[] GazeResults;

    internal CancellationTokenSource CTS { get; set; } = new CancellationTokenSource();

    // Start is called before the first frame update
    void Start()
    {
        try
        {
            StateMachine = new StateMachineStudy.Process();
            thisObject = this;
            var paths = new PathDefinition.Path[] { PathDefinition.Path.Path_1, PathDefinition.Path.Path_2, PathDefinition.Path.Path_2, PathDefinition.Path.Path_3 };
            // Path = paths[Random.Range(0,3)];
        }
        catch(Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    // Update is called once per frame
    void Update()
    {
        try
        {
            LogicUpdate();
            switch (StateMachine.CurrentState)
            {
                case StateMachineStudy.ProcessState.Empty:
                    if (Time.realtimeSinceStartup > 5 && !pointInProgress)
                    {
                        PathDefinition = new PathDefinition(Path);
                        MazeEnvironment = new MazeEnvironment(PathDefinition.Descriptor);
                        InitFunction();
                    }
                    break;

                case StateMachineStudy.ProcessState.Orientation:
                    if (!getFeeling)
                    {
                        GUITips.text = "";
                        OrientateEnviroment();
                        MazeEnvironment.CalculateStartingPositions();
                        watchFeels.Start();
                        Logging log = gameObject.GetComponent<Logging>();
                        Task.Run(() => log.LogData(CTS.Token));
                        getFeeling = true;
                        
                    }
                    if (getFeeling && watchFeels.Elapsed.TotalSeconds > 4 && !pointInProgress)
                    {
                        GetFeeling();
                        watchWalk.Start();
                    }
                    break;

                case StateMachineStudy.ProcessState.Walk:
                    
                    if (watchWalk.Elapsed.TotalSeconds > WalkTimeMin * 60 && !startingRaiseOnce)
                    {
                        GUITips.text = "";
                        ShowEnvironment(false);
                        startingRaiseOnce = RaiseStartingPosition();
                    }
                    break;

                case StateMachineStudy.ProcessState.ShowLM:
                    if (activeLandMark == null)
                    {
                        GUITips.text = "Stop moving!";
                        mazeIndex++;
                        activeLandMark = GameObject.FindGameObjectsWithTag("LandMark").Single(x => x.activeSelf == true && x.transform.parent == MazeEnvironment.Mazes[mazeIndex].transform);
                    }
                    if (RaiseLandMark() && !pointInProgress)
                    {
                        StartLookingForLandMark();
                        //Next State is triggered by StopLookingForLandMark()
                    }
                    break;

                case StateMachineStudy.ProcessState.Maze:
                    RaiseMaze();
                    break;

                case StateMachineStudy.ProcessState.PointLM:
                    if (!pointInProgress && unraise == false)
                    {
                        PointToMazeStart();
                    }
                    else if (unraise && !pointInProgress)
                    {
                        if (UnRaiseMaze())
                        {
                            unraise = false;
                            activeLandMark = null;

                            ShowConfidence();
                        }
                    }
                    break;

                case StateMachineStudy.ProcessState.Exit:
                    GazeResults = GetGazeRsults();
                    CTS.Cancel();
                    if (!pointInProgress)
                    {
                        EyeClosedRecognize eyeCLosedSkript = gameObject.GetComponent<EyeClosedRecognize>();
                        eyeCLosedSkript.EyeTriggered += EndApplication;
                        gliaBehaviour.OnEyeTracking.AddListener(eyeCLosedSkript.EyeTrackingHandler);
                        GUITips.text = "Thank you for participating. You can now remove the headset.";
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    private void OnApplicationQuit()
    {
        CTS?.Cancel();
        Thread.Sleep(3000);
    }
}
