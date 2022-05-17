using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class PathDefinition
{
    public enum Path
    {
        Path_1 = 0,
        Path_2 = 1,
        Path_3 = 2,
        Path_4 = 3,
    }

    public Path UsedPath { get; set; }

    public PathDescriptor Descriptor { get; set; }

    public PathDefinition(Path path)
    {
        UsedPath = path;
        Descriptor = AssignPathDescriptor(path);
    }

    private PathDescriptor AssignPathDescriptor(Path path)
    {
        switch ((int)path)
        {
            case 0:
                int[] sequence = { 2, 3, 4, 1, 5 };
                bool[] mirrorX = { false, false, false, false, false };
                bool[] mirrorZ = { false, false, false, false, true };
                char[] landmark = { 'B', 'B', 'A', 'B', 'A' };
                return new PathDescriptor(sequence, mirrorX, mirrorZ, landmark);
            case 1:
                sequence = new int[] { 1, 4, 3, 2, 5 };
                mirrorX = new bool[] { false, true, true, true, true };
                mirrorZ = new bool[] { false, true, true, true, true };
                landmark = new char[] { 'B', 'B', 'A', 'A', 'B' };
                return new PathDescriptor(sequence, mirrorX, mirrorZ, landmark);
            case 2:
                sequence = new int[] { 3, 5, 1, 4, 2 };
                mirrorX = new bool[] { false, true, true, false, true };
                mirrorZ = new bool[] { false, true, true, false, false };
                landmark = new char[] { 'B', 'A', 'B', 'B', 'B' };
                return new PathDescriptor(sequence, mirrorX, mirrorZ, landmark);
            case 3:
                sequence = new int[] { 5, 2, 3, 4, 1 };
                mirrorX = new bool[] { false, false, false, false, false };
                mirrorZ = new bool[] { false, false, false, false, false };
                landmark = new char[] { 'A', 'B', 'B', 'A', 'B' };
                return new PathDescriptor(sequence, mirrorX, mirrorZ, landmark);
            default:
                return null;
        }
    }
}