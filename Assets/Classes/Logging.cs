using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HP.Omnicept.Messaging.Messages;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public class Logging : MonoBehaviour
{
    internal int heartRate = -1;
    internal float cognitiveLoad = 0f;
    internal float cognitiveLoadStd = 0f;
    internal string path;
    internal uint index =0;
    public GameObject XCamera;
    EventWaitHandle LogWh = new AutoResetEvent(false);
    DateTime timeSinceLastWait;
    public void HeartRateHandler(HeartRate heartRate)
    {
        if (heartRate.Rate != 0)
        {
            this.heartRate = (int) heartRate.Rate;
        }
        else
        {
            this.heartRate = -1;
        }
        //Debug.Log("Heartrate:" + heartRate);
    }

    public void CognitiveLoadHandler(CognitiveLoad cognitiveLoad)
    {
        this.cognitiveLoad = cognitiveLoad.CognitiveLoadValue;
        this.cognitiveLoadStd = cognitiveLoad.StandardDeviation;
        //Debug.Log("Cognitive load:" + cognitiveLoad);
        LogWh.Set();
    }

    public void LogData(CancellationToken token)
    {
        Task.Run(() => MinLog(token));
        Thread.CurrentThread.Priority = System.Threading.ThreadPriority.AboveNormal;
        Thread.CurrentThread.IsBackground = false;
        try
        {
            DateTime startTime = DateTime.Now;
            string folderPath = Path.Combine(Logic.thisObject.AppPath, "Results" + Logic.thisObject.participantId);
            Directory.CreateDirectory(folderPath);
            path = Path.Combine(folderPath,"SensorData_" + Logic.thisObject.participantId + "_" + Logic.thisObject.LocomotionType.ToString());
            var loggingStart = DateTime.Now.ToShortTimeString();
            using (StreamWriter sw = File.CreateText(path))
            {
                sw.WriteLine("ParticipantID:" + Logic.thisObject.participantId);
                sw.WriteLine("LocomotionMode:" + Logic.thisObject.LocomotionType.ToString());
                sw.WriteLine("Date:" + DateTime.Now.ToShortDateString() + "Time:" + loggingStart);
                sw.WriteLine("Path:" + Logic.thisObject.Path.ToString());
                sw.WriteLine("ParticipantID;TimeId;TimeStamp/ms;Heartbeat/bpm;CognitiveLoad/[];CognitiveLoadStd/[];X-Position/m;Y-Position/m;Z-Position/m;StateMachineState");
                
                while (!token.IsCancellationRequested)
                {
                    LogWh.WaitOne();
                    timeSinceLastWait = DateTime.Now;
                    var pos = Logic.thisObject.CameraPos;
                    string line = string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9}", Logic.thisObject.participantId, index, (DateTime.Now - startTime).TotalMilliseconds.ToString("F0"), heartRate, cognitiveLoad.ToString("N2"), cognitiveLoadStd.ToString("N2"), pos.x.ToString("N2"), pos.y.ToString("N2"), pos.z.ToString("N2"), Logic.thisObject.StateMachine.CurrentState.ToString());
                    sw.WriteLine(line);
                    index++;
                }
            }
            path = Path.Combine(folderPath, "SpatialUpdating_" + Logic.thisObject.participantId + "_" + Logic.thisObject.LocomotionType.ToString());
            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine("ParticipantID:" + Logic.thisObject.participantId);
                sw.WriteLine("LocomotionMode:" + Logic.thisObject.LocomotionType.ToString());
                sw.WriteLine("Date:" + DateTime.Now.ToShortDateString() + "Time:" + loggingStart);
                sw.WriteLine("Path:" + Logic.thisObject.Path.ToString());
                sw.WriteLine("Feel:" + Logic.thisObject.feelingGood.ToString("N2"));
                sw.WriteLine("ParticipantID;Path;LandmarkNo;Offset/degree;Confidence");
                for (int i = 0; i < Logic.thisObject.confidence.Length; i++)
                {
                    string line = string.Format("{0};{1};{2};{3};{4}", Logic.thisObject.participantId, Logic.thisObject.Path.ToString(), (i + 1).ToString(), Logic.thisObject.GazeResults[i].ToString("N2"), Logic.thisObject.confidence[i].ToString("N2"));
                    sw.WriteLine(line);
                }
                sw.WriteLine("Transformationmatrix:" + Logic.thisObject.transformationMatrix);
            }
        }
        catch(Exception ex)
        {
           Debug.Log(ex.Message);
        }
    }

    private void MinLog(CancellationToken token)
    {
        while (true)
        {
            Thread.Sleep(333);
            if(DateTime.Now - timeSinceLastWait > TimeSpan.FromSeconds(3))
            {
                LogWh.Set();
            }
        }
    }
}