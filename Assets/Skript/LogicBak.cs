using HP.Omnicept.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LogicBak : MonoBehaviour
{

    internal PathDefinition PathDefinition;
    internal MazeEnvironment MazeEnvironment;
    public PathDefinition.Path Path;
    public static StateMachineStudy.Process StateMachine { get; set; }
    public GameObject xRRig;
    public GameObject environment;

    internal Enums.EOrientation EOrientation = Enums.EOrientation.NotDone;
    internal Enums.Landmarkstate Landmarkstate = Enums.Landmarkstate.StartingPosition;
    internal Enums.MazeState MazeState = Enums.MazeState.Unraised;
    internal Enums.UserState UserState = Enums.UserState.Walking;

    public float riseRate = 1;
    private int mazeIndex = 0;
    public float landMarkHeigth = -0.1f;
    public float mazeHeight = -0.1f;
    public float mazeHeightStore = -4f;

    public static LogicBak thisObject;
    public float WalkTimeMin = 10;
    public GameObject gliaObject;
    internal GliaBehaviour gliaBehaviour;

    internal GameObject activeLandMark = null;
    private GameObject gazeCursor;

    private bool pointInProgress = false;

    // Start is called before the first frame update
    void Start()
    {
        StateMachine = new StateMachineStudy.Process();
        thisObject = this;
    }

    // Update is called once per frame
    void Update()
    {
        switch (StateMachine.CurrentState)
        {
            case StateMachineStudy.ProcessState.Empty:
                if (Time.realtimeSinceStartup > 1)
                {
                    PathDefinition = new PathDefinition(Path);
                    MazeEnvironment = new MazeEnvironment(PathDefinition.Descriptor);
                    gliaBehaviour = gliaObject.GetComponent<GliaBehaviour>();
                    StateMachine.MoveNext(StateMachineStudy.Command.Init);
                    gazeCursor = xRRig.transform.GetChild(0).transform.GetChild(2).gameObject;
                    gazeCursor.tag = "GazeCursor";
                    gazeCursor.gameObject.SetActive(false);
                }
                break;
            case StateMachineStudy.ProcessState.Orientation:
                if (EOrientation == Enums.EOrientation.NotDone)
                {
                    OrientateEnviroment();
                    StateMachine.MoveNext(StateMachineStudy.Command.Begin);
                }

                break;
            case StateMachineStudy.ProcessState.Walk:
                if (Time.realtimeSinceStartup > WalkTimeMin * 60)
                {
                    RaiseStartingPosition();
                   
                }
                break;

            case StateMachineStudy.ProcessState.ShowLM:
                UserState = Enums.UserState.Pointing;
                if (MazeState == Enums.MazeState.Raised)
                {
                    UnRaiseMaze();
                }              
                else if (Landmarkstate == Enums.Landmarkstate.LandMarks)
                {
                    if (activeLandMark == null)
                    {
                        activeLandMark = GameObject.FindGameObjectsWithTag("LandMark").Single(x => x.activeSelf == true && x.transform.parent == MazeEnvironment.Mazes[mazeIndex].transform);
                    }
                    RaiseLandMark();
                }
                break;

            case StateMachineStudy.ProcessState.Maze:

                switch (MazeState)
                {
                    case Enums.MazeState.Unraised:
                        RaiseMaze();
                        break;
                    case Enums.MazeState.Raised:

                        break;
                }
                break;
            case StateMachineStudy.ProcessState.PointLM:

                if (UserState == Enums.UserState.Pointing)
                {
                    if (!pointInProgress)
                    {
                        StartLookingForLandMark();
                        pointInProgress = true;
                    }
                }
                else
                {
                    UnRaiseMaze();
                }
                break;

            case StateMachineStudy.ProcessState.Exit:
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
                break;


        }
    }

    private void RaiseLandMark()
    {
        try
        {
            activeLandMark.transform.position += Vector3.up * riseRate * Time.deltaTime;
            if (activeLandMark.transform.position.y < landMarkHeigth)
            {
                activeLandMark.transform.position += Vector3.up * riseRate * Time.deltaTime;
                StartLookingForLandMark();
            }
        }
        catch (Exception ex)
        {
            Debug.Log("Starting position rise failed:" + ex.Message);
        }
    }

    private void RaiseMaze()
    {
        MazeEnvironment.Mazes[mazeIndex].transform.position += Vector3.up * riseRate * Time.deltaTime;
        activeLandMark.transform.position -= Vector3.up * riseRate * Time.deltaTime;

        if (MazeEnvironment.Mazes[mazeIndex].transform.position.y > mazeHeight)
        {
            MazeState = Enums.MazeState.Raised;
            MazeEnvironment.Mazes[mazeIndex].transform.GetComponentInChildren<Touched>(false).active = true;
            activeLandMark = null;
        }
    }

    private void UnRaiseMaze()
    {
        MazeEnvironment.Mazes[mazeIndex].transform.position += Vector3.down * riseRate * Time.deltaTime;

        if (MazeEnvironment.Mazes[mazeIndex].transform.position.y < mazeHeightStore && MazeState != Enums.MazeState.Unraised)
        {
            MazeState = Enums.MazeState.Unraised;
            MazeEnvironment.Mazes[mazeIndex].SetActive(false);

            mazeIndex++;
        }
    }

    private void RaiseStartingPosition()
    {
        try
        {
            GameObject startingLandmark = GameObject.FindGameObjectWithTag("StartingPosition");
            startingLandmark.transform.position += Vector3.up * riseRate * Time.deltaTime;

            if (startingLandmark.transform.position.y > landMarkHeigth)
            {
                StateMachine.MoveNext(StateMachineStudy.Command.Begin);
                startingLandmark.GetComponent<Touched>().active = true;
 
            }
        }
        catch (Exception ex)
        {
            Debug.Log("Starting position rise failed:" + ex.Message);
        }
    }

    void OrientateEnviroment()
    {
        try
        {
            var xRPosition = xRRig.transform.position;
            var position = new Vector3(xRPosition.x, 0, xRPosition.z);
            var xRFor = xRRig.transform.forward;
            var forward = (xRFor - Vector3.Dot(new Vector3(0, 1, 0), xRFor) * new Vector3(0, 1, 0)).normalized;

            environment.transform.SetPositionAndRotation(position, Quaternion.LookRotation(forward, new Vector3(0, 1, 0)));
            EOrientation = Enums.EOrientation.Successful;
        }
        catch (Exception ex)
        {
            EOrientation = Enums.EOrientation.Failed;
            Debug.Log(ex.Message);
        }
    }


    internal void StartLookingForLandMark()
    {
        try
        {
            gazeCursor.SetActive(true);
            EyeClosedRecognize eyeCLosedSkript = gameObject.GetComponent<EyeClosedRecognize>();
            eyeCLosedSkript.EyeTriggered += StopLookingForLandMark;
            gliaBehaviour.OnEyeTracking.AddListener(eyeCLosedSkript.EyeTrackingHandler);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    void StopLookingForLandMark(object sender, EventArgs e)
    {
        var eyeCursor = GameObject.FindGameObjectWithTag("GazeCursor");
        var cursorPosition = eyeCursor.transform.position;
        EyeClosedRecognize eyeCLosedSkript = gameObject.GetComponent<EyeClosedRecognize>();
        gliaBehaviour.OnEyeTracking.RemoveAllListeners();
        pointInProgress = false;

        UserState = Enums.UserState.Walking;
        Landmarkstate = Enums.Landmarkstate.LandMarks;
        if (StateMachine.CurrentState == StateMachineStudy.ProcessState.PointLM)
        {
            StateMachine.MoveNext(StateMachineStudy.Command.PointedTowardsLM);
        }
    }
}
