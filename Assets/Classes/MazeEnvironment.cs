using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

internal class MazeEnvironment
{
    public GameObject[] Mazes;
    public Vector3 StartingPosition;
    internal Vector3[] StartingPositions = new Vector3[5];
    public GameObject[] LandMarks = new GameObject[5];

    public MazeEnvironment(PathDescriptor pathDescriptor)
    {
        AssignMazes(pathDescriptor.MazeSequence);
        MirrorMazes(pathDescriptor.MirrorMazesX, pathDescriptor.MirrorMazesY);
        DeactivateLandMarks(pathDescriptor.Landmarks);
    }


    private void AssignMazes(int[] path)
    {
        try
        {
            GameObject[] mazes = GameObject.FindGameObjectsWithTag("Mazes");
            Mazes = new GameObject[path.Length];
            for (int i = 0; i < Mazes.Length; i++)
            {
                Mazes[i] = mazes.Single(x => x.name.Contains(path[i].ToString()));
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    private void MirrorMazes(bool[] mirrorMazesX, bool[] mirrorMazesY)
    {
        try
        {
            for (int i = 0; i < Mazes.Length; i++)
            {
                //Mirror according to given argument (if both are true, the previous assignment is overwritten); default is (1,1,1)
                if (mirrorMazesX[i])
                {
                    Mazes[i].transform.localScale = new Vector3(-1, 1, 1);
                }
                if (mirrorMazesY[i])
                {
                    Mazes[i].transform.localScale = new Vector3(1, 1, -1);
                }
                if (mirrorMazesX[i] && mirrorMazesY[i])
                {
                    Mazes[i].transform.localScale = new Vector3(-1, 1, -1);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    private void DeactivateLandMarks(char[] landmarks)
    {
        try
        {
            GameObject[] landMarksObjects = GameObject.FindGameObjectsWithTag("LandMark");
            var startingLandmark = landMarksObjects.Single(x => x.transform.IsChildOf(Mazes[0].transform) && x.tag == "LandMark" && x.name != landmarks[0].ToString());
            startingLandmark.tag = "StartingPosition";
            startingLandmark.GetComponent<Touched>().WasTouched += StartingLandMark_WasTouched;
            StartingPosition = startingLandmark.transform.position;

            LandMarks[0] = landMarksObjects.Single(x => x.transform.IsChildOf(Mazes[0].transform) && x.tag == "LandMark" && x.name == landmarks[0].ToString());
            for (int i = 0; i < Mazes.Length; i++)
            {
                // i=0 is exclude due to it being the starting position
                if (i != 0)
                {
                    GameObject landmark = landMarksObjects.Single(x => x.transform.IsChildOf(Mazes[i].transform) && x.tag == "LandMark" && x.name != landmarks[i].ToString());
                    landmark.SetActive(false);
                    LandMarks[i] = landMarksObjects.Single(x => x.transform.IsChildOf(Mazes[i].transform) && x.tag == "LandMark" && x.name == landmarks[i].ToString());
                }

                Touched touchedSkript = landMarksObjects.Single(x => x.transform.IsChildOf(Mazes[i].transform) && x.name == landmarks[i].ToString()).GetComponent<Touched>();
                touchedSkript.WasTouched += TouchedSkript_WasTouched;
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    private void StartingLandMark_WasTouched(object sender, EventArgs e)
    {
        try
        {
            
            Logic.thisObject.StateMachine.MoveNext(StateMachineStudy.Command.Begin);
        }
        catch(Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    private void TouchedSkript_WasTouched(object sender, EventArgs e)
    {
        
        Logic.thisObject.StateMachine.MoveNext(StateMachineStudy.Command.TouchedLM);
    }

    internal void CalculateStartingPositions()
    {
        Vector3[] vectors = new Vector3[5];
        GameObject[] landMarksObjects = GameObject.FindGameObjectsWithTag("LandMark");
        vectors[0] = StartingPosition;
        for (int i = 1; i < Mazes.Length; i++)
        {
            vectors[i] = LandMarks[i - 1].transform.position;
        }
        StartingPositions = vectors;
    }
}

