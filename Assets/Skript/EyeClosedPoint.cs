using HP.Omnicept.Messaging.Messages;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeClosedPoint : MonoBehaviour
{
    DateTime leftClosed;
    bool isLeftClosed = false;
    int beeps = 0;
    public int maxBeep = 3;
    public AudioSource beeper;

    public event EventHandler EyeTriggered;

    public void EyeTrackingHandler(EyeTracking eyeTracking)
    {
        if (eyeTracking != null)
        {
            if (eyeTracking.LeftEye.Openness == 0)
            {
                if (isLeftClosed == false)
                {
                    leftClosed = DateTime.Now;
                    isLeftClosed = true;
                }
                else
                {
                    double millisecondsClosed = (DateTime.Now - leftClosed).TotalMilliseconds;
                    if (millisecondsClosed - beeps * 1000 > 1000)
                    {
                        beeper.Play();
                        beeps++;
                    }
                    if (beeps > maxBeep - 1)
                    {
                        EyeTriggered?.Invoke(this, EventArgs.Empty);
                        beeps = 0;
                        isLeftClosed=false;
                    }
                }
            }
            else
            {
                isLeftClosed = false;
                beeps = 0;
            }
        }
    }
}
