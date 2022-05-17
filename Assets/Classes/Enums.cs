using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Enums
{
    internal enum EOrientation
    {
        NotDone = 0,
        Successful = 1,
        Failed = 2,
    }

    internal enum Landmarkstate
    {
        StartingPosition = 0,
        LandMarks = 1,
    }

    internal enum MazeState
    {
        Unraised = 0,
        Raised = 1,
    }
    internal enum UserState
    {
        Walking = 0,
        Pointing = 1,
    }

    public enum LocomotionType
    {
        Walking =0,
        Wheelchair = 1,
        WheelchairInterface = 2,
        Joystick = 3,
    }
}

