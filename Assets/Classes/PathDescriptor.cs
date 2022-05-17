using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class PathDescriptor
{
    /// <summary>
    /// The sequence of mazes (1 to 5)
    /// </summary>
    internal int[] MazeSequence;

    /// <summary>
    /// Should the respective maze be mirrored along the X axis
    /// </summary>
    internal bool[] MirrorMazesX;
    /// <summary>
    /// Should the respective maze be mirrored along the Z axis
    /// </summary>
    internal bool[] MirrorMazesY;

    /// <summary>
    /// The indicator, which land mark should be used as goal (A/B)
    /// </summary>
    internal char[] Landmarks;

    public PathDescriptor(int[] sequence, bool[] mirrorMazesX, bool[] mirrorMazesZ, char[] landmarks)
    {
        MazeSequence = sequence;
        MirrorMazesX = mirrorMazesX;
        MirrorMazesY = mirrorMazesZ;
        Landmarks = landmarks;
    }
}

