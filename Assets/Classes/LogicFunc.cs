using HP.Omnicept.Unity;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities.Gltf.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public partial class Logic
{
    internal PathDefinition PathDefinition;
    internal MazeEnvironment MazeEnvironment;
    private int mazeIndex = -1;

    internal static Logic thisObject;
    internal GliaBehaviour gliaBehaviour;

    internal GameObject activeLandMark = null;
    private GameObject gazeCursor;
    internal Vector3 CameraPos { get; private set; } = new Vector3(float.NaN, float.NaN, float.NaN);
    private bool pointInProgress = false;
    private GameObject startingLandmark;

    Vector3[] gazeVectors = new Vector3[5];
    Vector3[] goalVectors = new Vector3[5];
    bool unraise = false;
    internal string AppPath;
    internal GameObject slider;
    internal float feelingGood;
    internal float[] confidence = new float[5];
    internal TextMeshProUGUI GUITips;
    internal string transformationMatrix;

    internal bool startingRaiseOnce = false;
    internal System.Diagnostics.Stopwatch watchWalk = new System.Diagnostics.Stopwatch();
    internal System.Diagnostics.Stopwatch watchFeels = new System.Diagnostics.Stopwatch();
    internal bool getFeeling = false;


    internal void InitFunction()
    {
        gliaBehaviour = gliaObject.GetComponent<GliaBehaviour>();
        gazeCursor = xRRig.transform.GetChild(0).transform.GetChild(1).gameObject;
        gazeCursor.tag = "GazeCursor";
        gazeCursor.gameObject.SetActive(false);
        startingLandmark = GameObject.FindGameObjectWithTag("StartingPosition");
        Logging log = gameObject.GetComponent<Logging>();
        gliaBehaviour.OnHeartRate.AddListener(log.HeartRateHandler);
        gliaBehaviour.OnCognitiveLoad.AddListener(log.CognitiveLoadHandler);
        AppPath = Application.persistentDataPath;
        //new NetMQ.Sockets.RequestSocket(null);// Build for omnicept (package ist included)
        GUITips = GameObject.FindGameObjectWithTag("Commands").GetComponent<TextMeshProUGUI>();
        GUITips.text = "Please stand in the middle of the physical room, look straight ahead and close your eyes for four seconds (beeps).";
        EyeClosedRecognize4 eyeCLosedSkript = gameObject.GetComponent<EyeClosedRecognize4>();
        gliaBehaviour.OnEyeTracking.AddListener(eyeCLosedSkript.EyeTrackingHandler);
        eyeCLosedSkript.EyeTriggered += StartEverything;
        pointInProgress = true;
    }

    private void GetFeeling()
    {
        gazeCursor?.SetActive(true);
        eyetrackerRayCaster.SetActive(true);
        GUITips.text = String.Empty;
        gliaBehaviour.OnEyeTracking.RemoveAllListeners();
        slider = Instantiate(feelGoodPrefab, MainCamera.transform.position + MainCamera.transform.forward * 2f, MainCamera.transform.rotation);
        EyeClosedRecognize eyeCLosedSkript = gameObject.GetComponent<EyeClosedRecognize>();
        gliaBehaviour.OnEyeTracking.AddListener(eyeCLosedSkript.EyeTrackingHandler);
        eyeCLosedSkript.EyeTriggered += GotFeeling;
        pointInProgress = true;
    }

    private void GotFeeling(object sender, EventArgs e)
    {
        try
        {
            gazeCursor?.SetActive(false);
            EyeClosedRecognize eyeCLosedSkript = gameObject.GetComponent<EyeClosedRecognize>();
            eyeCLosedSkript.EyeTriggered -= GotFeeling;
            GUITips.text = "Feel free to explore within the green line.";
            feelingGood = slider.GetComponentInChildren<Slider>().value;
            Debug.Log("Feels:" + feelingGood);
            slider.SetActive(false);
            gliaBehaviour.OnEyeTracking.RemoveAllListeners();
            pointInProgress = false;
            eyetrackerRayCaster.SetActive(false);
            ShowEnvironment(true);
            StateMachine.MoveNext(StateMachineStudy.Command.Begin);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    private void StartEverything(object sender, EventArgs e)
    {
        EyeClosedRecognize4 eyeCLosedSkript = gameObject.GetComponent<EyeClosedRecognize4>();
        eyeCLosedSkript.EyeTriggered -= StartEverything;
        gliaBehaviour.OnEyeTracking.RemoveAllListeners();
        pointInProgress = false;
        StateMachine.MoveNext(StateMachineStudy.Command.Init);
    }

    void OrientateEnviroment()
    {
        try
        {
            //var matrix = System.Numerics.Matrix4x4.CreateFromQuaternion(new System.Numerics.Quaternion(xRRig.transform.rotation.x, xRRig.transform.rotation.y, xRRig.transform.rotation.z, xRRig.transform.rotation.w));
            //var xRFor = new Vector3(matrix.M11, matrix.M21, matrix.M313);
            //var forward = (xRFor - Vector3.Dot(new Vector3(0, 1, 0), xRFor) * new Vector3(0, 1, 0)).normalized;

            var xRPosition = MainCamera.transform.position;
            var position = new Vector3(xRPosition.x, 0, xRPosition.z);
            var xRFor = MainCamera.transform.forward;
            var forward = (xRFor - Vector3.Dot(new Vector3(0, 1, 0), xRFor) * new Vector3(0, 1, 0)).normalized;
            environment.transform.SetPositionAndRotation(position, Quaternion.LookRotation(forward, new Vector3(0, 1, 0)));
            transformationMatrix = environment.transform.localToWorldMatrix.ToString();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    private void ShowEnvironment(bool show)
    {
        if (walkplane.activeSelf != show)
        {
            walkplane.SetActive(show);
        }
    }

    private bool RaiseStartingPosition()
    {
        try
        {
            if (startingLandmark.transform.position.y > landMarkHeigth)
            {
                startingLandmark.GetComponent<Touched>().active = true;
                walkplane.SetActive(false);
                GUITips.text = "Find the green pillar and touch it.";
                return true;
            }
            startingLandmark.transform.position += Vector3.up * riseLandMarkRate * Time.deltaTime;
            leeds.transform.position -= Vector3.up * riseLandMarkRate * Time.deltaTime;

        }
        catch (Exception ex)
        {
            Debug.Log("Starting position rise failed:" + ex.Message);
        }
        return false;
    }

    internal void StartLookingForLandMark()
    {
        try
        {
            GUITips.text = "Don't move! "+ Environment.NewLine +" Locate the next landmark. Rember it's position. After confirming the position by closing your eyes for 2 seconds, walk through the maze and find it.";
            gazeCursor?.SetActive(true);
            EyeClosedRecognize eyeCLosedSkript = gameObject.GetComponent<EyeClosedRecognize>();
            eyeCLosedSkript.EyeTriggered += StopLookingForLandMark;
            gliaBehaviour.OnEyeTracking.AddListener(eyeCLosedSkript.EyeTrackingHandler);
            pointInProgress = true;
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    void StopLookingForLandMark(object sender, EventArgs e)
    {
        try
        {
            EyeClosedRecognize eyeCLosedSkript = gameObject.GetComponent<EyeClosedRecognize>();
            eyeCLosedSkript.EyeTriggered -= StopLookingForLandMark;
            gliaBehaviour.OnEyeTracking.RemoveAllListeners();
            gazeCursor?.SetActive(false);
            pointInProgress = false;
            MazeEnvironment.Mazes[mazeIndex].transform.GetComponentInChildren<Touched>(false).active = true;
            StateMachine.MoveNext(StateMachineStudy.Command.SeenLM);
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }
    private bool RaiseLandMark()
    {
        try
        {
            if (activeLandMark.transform.position.y > landMarkHeigth)
            {
                return true;
            }
            activeLandMark.transform.position += Vector3.up * riseLandMarkRate * Time.deltaTime;
        }
        catch (Exception ex)
        {
            Debug.Log("Land mark rise failed:" + ex.Message);
        }
        return false;
    }
    private bool RaiseMaze()
    {
        try
        {
            if (MazeEnvironment.Mazes[mazeIndex].transform.position.y > mazeHeight)
            {
                return true;
            }
            MazeEnvironment.Mazes[mazeIndex].transform.position += Vector3.up * riseMazeRate * Time.deltaTime;
            activeLandMark.transform.position -= Vector3.up * riseMazeRate * Time.deltaTime;
            GUITips.text = String.Empty;
            return false;
        }
        catch (Exception ex)
        {
            Debug.Log("Failed to raise Maze" + ex.Message);
            return false;
        }
    }
    void PointToMazeStart()
    {
        try
        {
            pointInProgress = true;
            gazeCursor?.SetActive(true);
            GUITips.text = "Please look toward your starting position and confirm by closing your eyes for 3 seconds.";
            EyeClosedPoint eyeCLosedSkript = gameObject.GetComponent<EyeClosedPoint>();
            eyeCLosedSkript.EyeTriggered += FinishedPointing;
            gliaBehaviour.OnEyeTracking.AddListener(eyeCLosedSkript.EyeTrackingHandler);

        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }
    void FinishedPointing(object sender, EventArgs e)
    {
        try
        {
            EyeClosedPoint eyeCLosedSkript = gameObject.GetComponent<EyeClosedPoint>();
            eyeCLosedSkript.EyeTriggered -= FinishedPointing;
            GUITips.text = String.Empty;
            gazeVectors[mazeIndex] = MainCamera.transform.forward;
            goalVectors[mazeIndex] = MazeEnvironment.StartingPositions[mazeIndex] - MainCamera.transform.position;
            gliaBehaviour.OnEyeTracking.RemoveAllListeners();
            gazeCursor?.SetActive(false);
            pointInProgress = false;
            unraise = true;
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    private void ShowConfidence()
    {
        try
        {
            eyetrackerRayCaster.SetActive(true);
            pointInProgress = true;
            slider = Instantiate(confidencePrefab, MainCamera.transform.position + MainCamera.transform.forward * 2f, MainCamera.transform.rotation);
            EyeClosedRecognize eyeCLosedSkript = gameObject.GetComponent<EyeClosedRecognize>();
            gliaBehaviour.OnEyeTracking.AddListener(eyeCLosedSkript.EyeTrackingHandler);
            eyeCLosedSkript.EyeTriggered += SelectedConfidence;
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    private void SelectedConfidence(object sender, EventArgs e)
    {
        try
        {
            EyeClosedRecognize eyeCLosedSkript = gameObject.GetComponent<EyeClosedRecognize>();
            eyeCLosedSkript.EyeTriggered -= SelectedConfidence;
            confidence[mazeIndex] = slider.GetComponentInChildren<Slider>().value;
            slider.SetActive(false);
            gliaBehaviour.OnEyeTracking.RemoveAllListeners();
            pointInProgress = false;
            eyetrackerRayCaster.SetActive(false);
            if (mazeIndex < MazeEnvironment.Mazes.Length - 1)
            {
                StateMachine.MoveNext(StateMachineStudy.Command.PointedTowardsLM);
            }
            else
            {
                StateMachine.MoveNext(StateMachineStudy.Command.Exit);
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    private bool UnRaiseMaze()
    {
        try
        {
            if (MazeEnvironment.Mazes[mazeIndex].transform.position.y < mazeHeightStore)
            {
                MazeEnvironment.Mazes[mazeIndex].SetActive(false);
                return true;
            }
            MazeEnvironment.Mazes[mazeIndex].transform.position += Vector3.down * riseMazeRate * Time.deltaTime;
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
        return false;
    }
    private float[] GetGazeRsults()
    {
        float[] result = new float[MazeEnvironment.Mazes.Length];
        for (int i = 0; i < MazeEnvironment.Mazes.Length; i++)
        {
            Vector2 vectorGaze = new Vector2(gazeVectors[i].x, gazeVectors[i].z);
            Vector2 vectorGoal = new Vector2(goalVectors[i].x, goalVectors[i].z);
            float angle = Vector2.Angle(vectorGaze, vectorGoal);
            result[i] += angle;
        }

        return result;
    }

    private void LogicUpdate()
    {
        CameraPos = Camera.main.transform.position;
    }

    private void EndApplication(object sender, EventArgs e)
    {
        Thread.Sleep(3000);
#if UNITY_EDITOR
        // Application.Quit() does not work in the editor so
        // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
        UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }

}
