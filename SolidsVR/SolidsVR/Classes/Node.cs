using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using System.Linq;

namespace SolidsVR
{
    public class Node
    {
        Point3d coordinate;
        List<int> surfaceNum = new List<int>();
        Boolean isCorner = false;
        Boolean isMiddle = false;
        Boolean isEdge = false;

        public Node (Point3d _coordinate)
        {
           coordinate = _coordinate;
        }

        public void SetSurfaceNum(int i)
        {
            surfaceNum.Add(i);
        }

        public List<int> GetSurfaceNum()
        {
            return surfaceNum;
        }

        public void SetIsCorner()
        {
            isCorner = true;
        }

        public Boolean GetIsCorner()
        {
            return isCorner;
        }

        public void SetIsMiddle()
        {
            isMiddle = true;
        }

        public Boolean GetIsMiddle()
        {
            return isMiddle;
        }

        public void SetIsEdge()
        {
            isEdge = true;
        }

        public Boolean GetIsEdge()
        {
            return isEdge;
        }

        public void SetNewCoord(Point3d _coord)
        {
            coordinate = _coord;

        }

        public Point3d GetCoord()
        {
            return coordinate;
        }

    }
}
