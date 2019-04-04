using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Drawing;


namespace SolidsVR
{
    public class SetUniLoadComponent : GH_Component
    {
      
        public SetUniLoadComponent()
          : base("Uniform load component for FEA", "UniLoads",
              "Description",
              "Category3", "Loads")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddSurfaceParameter("Surface", "Surface", "Surface for loading", GH_ParamAccess.item);
            pManager.AddVectorParameter("Load vector", "Load", "Direction and load amount in kN/m", GH_ParamAccess.item);
            pManager.AddIntegerParameter("U count", "U", "Number of divisions in U direction", GH_ParamAccess.item);
            pManager.AddIntegerParameter("V count", "V", "Number of divisions in V direction", GH_ParamAccess.item);
            pManager.AddIntegerParameter("W count", "W", "Number of divisions in W direction", GH_ParamAccess.item);
            pManager.AddBrepParameter("Brep", "B", "Brep as a reference size", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Point loads", "PL", "Lumped load to points, (x,y,z);(Fx,Fy,Fz)", GH_ParamAccess.list);
            pManager.AddLineParameter("Load-arrows", "Geometry", "Arrows showing the load", GH_ParamAccess.list);
            pManager.AddColourParameter("Arrow coloring", "Color", "Coloring of arrows", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {

            Surface surface = null;
            Vector3d forceVec = new Vector3d();
            int u = 1;
            int v = 1;
            int w = 1;
            Brep origBrep = new Brep();

            
            if (!DA.GetData(0, ref surface)) return;
            if (!DA.GetData(1, ref forceVec)) return;
            if (!DA.GetData(2, ref u)) return;
            if (!DA.GetData(3, ref v)) return;
            if (!DA.GetData(4, ref w)) return;
            if (!DA.GetData(5, ref origBrep)) return; //input as 8 points insted, and create origBrep from this

            List<string> pointLoads = FindPointLoads(surface, forceVec, u, v, w, origBrep);

            ///////FOR PREVIEWING OF LOADS///////

            //Setting up values for reflength and angle for rotation of area
            
            double volume = origBrep.GetVolume();
            double sqrt3 = (double)1 / 3;
            double refLength = Math.Pow(volume, sqrt3);

            List<Line> arrows = DrawLoads(pointLoads, refLength);

            Color color = Color.Blue;

            DA.SetDataList(0, pointLoads);
            DA.SetDataList(1, arrows);
            DA.SetData(2, color);

        }

        public List<Line> DrawLoads(List<string> pointLoads, double refLength)
        {
            List<Line> arrows = new List<Line>();

            List<double> loadCoord = new List<double>();
            List<double> pointValues = new List<double>();

            double loadRef = 0.5;
            double arrowRef = 0.8;

            foreach (string s in pointLoads)
            {
                string coordinate = (s.Split(';'))[0];
                string iLoad = (s.Split(';'))[1];

                string[] coord = (coordinate.Split(','));
                string[] iLoads = (iLoad.Split(','));

                double loadCoord1 = Math.Round(double.Parse(coord[0]), 8);
                double loadCoord2 = Math.Round(double.Parse(coord[1]), 8);
                double loadCoord3 = Math.Round(double.Parse(coord[2]), 8);

                double loadX = Math.Round(double.Parse(iLoads[0]), 8);
                double loadY = Math.Round(double.Parse(iLoads[1]), 8);
                double loadZ = Math.Round(double.Parse(iLoads[2]), 8);

                Point3d startPoint = new Point3d(loadCoord1, loadCoord2, loadCoord3);
                Point3d arrowPart1 = new Point3d(0, 0, 0);
                Point3d arrowPart2 = new Point3d(0, 0, 0);
                Point3d endPoint = new Point3d(0, 0, 0);

                if (loadX != 0)
                {
                    endPoint = new Point3d(loadCoord1 - loadX * loadRef, loadCoord2, loadCoord3);


                    if (loadX > 0)
                    {
                        arrowPart1 = new Point3d(loadCoord1 - arrowRef, loadCoord2 - arrowRef, loadCoord3);
                        arrowPart2 = new Point3d(loadCoord1 - arrowRef, loadCoord2 + arrowRef, loadCoord3);
                    }
                    else
                    {
                        arrowPart1 = new Point3d(loadCoord1 + arrowRef, loadCoord2 + arrowRef, loadCoord3);
                        arrowPart2 = new Point3d(loadCoord1 + arrowRef, loadCoord2 - arrowRef, loadCoord3);
                    }
                    arrows.Add(new Line(startPoint, endPoint));
                    arrows.Add(new Line(startPoint, arrowPart1));
                    arrows.Add(new Line(startPoint, arrowPart2));
                }

                if (loadY != 0)
                {
                    endPoint = new Point3d(loadCoord1, loadCoord2 - loadY * loadRef, loadCoord3);


                    if (loadY > 0)
                    {
                        arrowPart1 = new Point3d(loadCoord1 - arrowRef, loadCoord2 - arrowRef, loadCoord3);
                        arrowPart2 = new Point3d(loadCoord1 + arrowRef, loadCoord2 - arrowRef, loadCoord3);
                    }
                    else
                    {
                        arrowPart1 = new Point3d(loadCoord1 - arrowRef, loadCoord2 + arrowRef, loadCoord3);
                        arrowPart2 = new Point3d(loadCoord1 + arrowRef, loadCoord2 + arrowRef, loadCoord3);
                    }
                    arrows.Add(new Line(startPoint, endPoint));
                    arrows.Add(new Line(startPoint, arrowPart1));
                    arrows.Add(new Line(startPoint, arrowPart2));
                }



                if (loadZ != 0)
                {
                    endPoint = new Point3d(loadCoord1, loadCoord2, loadCoord3 - loadZ * loadRef);


                    if (loadZ > 0)
                    {
                        arrowPart1 = new Point3d(loadCoord1 + arrowRef, loadCoord2, loadCoord3 - arrowRef);
                        arrowPart2 = new Point3d(loadCoord1 - arrowRef, loadCoord2, loadCoord3 - arrowRef);
                    }
                    else
                    {
                        arrowPart1 = new Point3d(loadCoord1 + arrowRef, loadCoord2, loadCoord3 + arrowRef);
                        arrowPart2 = new Point3d(loadCoord1 - arrowRef, loadCoord2, loadCoord3 + arrowRef);
                    }
                    arrows.Add(new Line(startPoint, endPoint));
                    arrows.Add(new Line(startPoint, arrowPart1));
                    arrows.Add(new Line(startPoint, arrowPart2));
                }

            }

            return arrows;
        }

        public List<string> FindPointLoads(Surface surface, Vector3d forceVec, int u, int v, int w, Brep brep)
        {
            Brep surfaceBrep = surface.ToBrep();
            Point3d [] vertices = surfaceBrep.DuplicateVertices();
            Point3d[] nodesAll = brep.DuplicateVertices();
           
            //FINDING CORNER POINTS
            List<Point3d> cornerPoints = new List<Point3d>();
            cornerPoints.Add(vertices[0]);
            cornerPoints.Add(vertices[1]);
            cornerPoints.Add(vertices[2]);
            cornerPoints.Add(vertices[3]);
            //END CORNER

            int relativeU = u;
            int relativeV = v;

            int[] nodeIndex = new int[4];
            for (int i = 0; i < vertices.Length; i++)
            {
                for (int j = 0; j < nodesAll.Length; j++)
                {
                    if (vertices[i] == nodesAll[j])
                    {
                        nodeIndex[i] = j;
                    }
                }
            }
            Array.Sort(nodeIndex);

            //If surface is below
            Vector3d vec_u1 = (nodesAll[1] - nodesAll[0]) / nodesAll[0].DistanceTo(nodesAll[1]);
            Vector3d vec_u2 = (nodesAll[2] - nodesAll[3]) / nodesAll[3].DistanceTo(nodesAll[2]);
            Vector3d vec_v1 = (nodesAll[3] - nodesAll[0]) / nodesAll[0].DistanceTo(nodesAll[3]);
            Vector3d vec_v2 = (nodesAll[2] - nodesAll[1]) / nodesAll[1].DistanceTo(nodesAll[2]);
            
            double l_u1 = nodesAll[0].DistanceTo(nodesAll[1]) / relativeU;
            double l_u2 = nodesAll[3].DistanceTo(nodesAll[2]) / relativeU;
            double l_v1 = nodesAll[0].DistanceTo(nodesAll[3]) / relativeV;
            double l_v2 = nodesAll[1].DistanceTo(nodesAll[2]) / relativeV;

            //If not, update:
            vertices[0] = nodesAll[0];
            vertices[1] = nodesAll[3];
            vertices[2] = nodesAll[0];
            vertices[3] = nodesAll[1];

            if (nodeIndex[0] == 0)
            {
                if (nodeIndex[1] == 1)
                {
                    if (nodeIndex[2] == 4)
                    {
                        relativeV = w;
                        vec_u2 = (nodesAll[5] - nodesAll[4]) / nodesAll[4].DistanceTo(nodesAll[5]);
                        vec_v1 = (nodesAll[4] - nodesAll[0]) / nodesAll[0].DistanceTo(nodesAll[4]);
                        vec_v2 = (nodesAll[5] - nodesAll[1]) / nodesAll[1].DistanceTo(nodesAll[5]);

                        l_u2 = nodesAll[4].DistanceTo(nodesAll[5]) / relativeU;
                        l_v1 = nodesAll[0].DistanceTo(nodesAll[4]) / relativeV;
                        l_v2 = nodesAll[1].DistanceTo(nodesAll[5]) / relativeV;

                        vertices[1] = nodesAll[4];
                    }
                }
                else if (nodeIndex[1] == 3)
                {
                    {
                        relativeU = v;
                        relativeV = w;
                        vec_u1 = (nodesAll[3] - nodesAll[0]) / nodesAll[0].DistanceTo(nodesAll[3]);
                        vec_u2 = (nodesAll[7] - nodesAll[4]) / nodesAll[4].DistanceTo(nodesAll[7]);
                        vec_v1 = (nodesAll[4] - nodesAll[0]) / nodesAll[0].DistanceTo(nodesAll[4]);
                        vec_v2 = (nodesAll[7] - nodesAll[3]) / nodesAll[3].DistanceTo(nodesAll[7]);
                        l_u1 = nodesAll[0].DistanceTo(nodesAll[3]) / relativeU;
                        l_u2 = nodesAll[4].DistanceTo(nodesAll[7]) / relativeU;
                        l_v1 = nodesAll[0].DistanceTo(nodesAll[4]) / relativeV;
                        l_v2 = nodesAll[3].DistanceTo(nodesAll[7]) / relativeV;
                        vertices[1] = nodesAll[4];
                        vertices[3] = nodesAll[3];
                    }
                }
            }
            else if (nodeIndex[0] == 1)
            {
                if (nodeIndex[1] == 2)
                {
                    relativeU = v;
                    relativeV = w;
                    vec_u1 = (nodesAll[2] - nodesAll[1]) / nodesAll[1].DistanceTo(nodesAll[2]);
                    vec_u2 = (nodesAll[6] - nodesAll[5]) / nodesAll[5].DistanceTo(nodesAll[6]);
                    vec_v1 = (nodesAll[5] - nodesAll[1]) / nodesAll[1].DistanceTo(nodesAll[5]);
                    vec_v2 = (nodesAll[6] - nodesAll[2]) / nodesAll[2].DistanceTo(nodesAll[6]);
                    l_u1 = nodesAll[1].DistanceTo(nodesAll[2]) / relativeU;
                    l_u2 = nodesAll[5].DistanceTo(nodesAll[6]) / relativeU;
                    l_v1 = nodesAll[1].DistanceTo(nodesAll[5]) / relativeV;
                    l_v2 = nodesAll[2].DistanceTo(nodesAll[6]) / relativeV;
                    vertices[0] = nodesAll[1];
                    vertices[1] = nodesAll[5];
                    vertices[2] = nodesAll[1];
                    vertices[3] = nodesAll[2];
                }
            }
            else if (nodeIndex[0] == 2)
            {
                relativeV = w;
                vec_u1 = (nodesAll[2] - nodesAll[3]) / nodesAll[3].DistanceTo(nodesAll[2]);
                vec_u2 = (nodesAll[6] - nodesAll[7]) / nodesAll[7].DistanceTo(nodesAll[6]);
                vec_v1 = (nodesAll[7] - nodesAll[3]) / nodesAll[3].DistanceTo(nodesAll[7]);
                vec_v2 = (nodesAll[6] - nodesAll[2]) / nodesAll[2].DistanceTo(nodesAll[6]);
                l_u1 = nodesAll[3].DistanceTo(nodesAll[2]) / relativeU;
                l_u2 = nodesAll[7].DistanceTo(nodesAll[6]) / relativeU;
                l_v1 = nodesAll[3].DistanceTo(nodesAll[7]) / relativeV;
                l_v2 = nodesAll[2].DistanceTo(nodesAll[6]) / relativeV;
                vertices[0] = nodesAll[3];
                vertices[1] = nodesAll[7];
                vertices[2] = nodesAll[3];
                vertices[3] = nodesAll[2];
            }
            else
            {
                vec_u1 = (nodesAll[5] - nodesAll[4]) / nodesAll[4].DistanceTo(nodesAll[5]);
                vec_u2 = (nodesAll[6] - nodesAll[7]) / nodesAll[7].DistanceTo(nodesAll[6]);
                vec_v1 = (nodesAll[7] - nodesAll[4]) / nodesAll[4].DistanceTo(nodesAll[7]);
                vec_v2 = (nodesAll[6] - nodesAll[5]) / nodesAll[5].DistanceTo(nodesAll[6]);
                l_u1 = nodesAll[4].DistanceTo(nodesAll[5]) / relativeU;
                l_u2 = nodesAll[7].DistanceTo(nodesAll[6]) / relativeU;
                l_v1 = nodesAll[4].DistanceTo(nodesAll[7]) / relativeV;
                l_v2 = nodesAll[5].DistanceTo(nodesAll[6]) / relativeV;
                vertices[0] = nodesAll[4];
                vertices[1] = nodesAll[7];
                vertices[2] = nodesAll[4];
                vertices[3] = nodesAll[5];
            }


            //FINDING LINE POINTS
            List<Point3d> linePoints = new List<Point3d>();

            for (int i = 1; i < relativeU; i++)
            {
                Point3d p1 = new Point3d(vertices[0].X + l_u1 * i * vec_u1.X, vertices[0].Y + l_u1 * vec_u1.Y * i, vertices[0].Z + l_u1 * vec_u1.Z * i);
                linePoints.Add(p1);
                Point3d p2 = new Point3d(vertices[1].X + l_u2 * i * vec_u2.X, vertices[1].Y + l_u2 * vec_u2.Y * i, vertices[1].Z + l_u2 * vec_u2.Z * i);
                linePoints.Add(p2);
            }

            for (int i = 1; i < relativeV; i++)
            {
                Point3d p1 = new Point3d(vertices[2].X + l_v1 * i * vec_v1.X, vertices[2].Y + l_v1 * vec_v1.Y * i, vertices[2].Z + l_v1 * vec_v1.Z * i);
                linePoints.Add(p1);
                Point3d p2 = new Point3d(vertices[3].X + l_v2 * i * vec_v2.X, vertices[3].Y + l_v2 * vec_v2.Y * i, vertices[3].Z + l_v2 * vec_v2.Z * i);
                linePoints.Add(p2);
            }

            //FINDING CENTER POINTS
            List<Point3d> centerPoints = new List<Point3d>();

            for (int i = 1; i < relativeU; i++)
            {
                Point3d p1_u = new Point3d(vertices[0].X + l_u1 * i * vec_u1.X, vertices[0].Y + l_u1 * vec_u1.Y * i, vertices[0].Z + l_u1 * vec_u1.Z * i);
                Point3d p2_u = new Point3d(vertices[1].X + l_u2 * i * vec_u2.X, vertices[1].Y + l_u2 * vec_u2.Y * i, vertices[1].Z + l_u2 * vec_u2.Z * i);

                Vector3d vec_u = (p2_u - p1_u) / (p1_u.DistanceTo(p2_u));

                double length_v1 = p1_u.DistanceTo(p2_u) / relativeV;

                for (int j = 1; j < relativeV; j++)
                {
                    Point3d p1_v = new Point3d(p1_u.X + length_v1 * j * vec_u.X, p1_u.Y + length_v1 * j * vec_u.Y, p1_u.Z + length_v1 * j * vec_u.Z);
                    centerPoints.Add(p1_v);
                }
            }

            //DISTRIBUTING LOAD TO POINTS FOUND
            double pointsCount = 4 * centerPoints.Count + cornerPoints.Count + 2 * linePoints.Count;
            double area = surfaceBrep.GetArea();
            forceVec = forceVec * area;

            List<string> centerPointsString = new List<string>();
            List<string> linePointsString = new List<string>();
            List<string> cornerPointsString = new List<string>();

            List<string> pointLoads = new List<string>();


            //Center
            foreach (Point3d p in centerPoints)
            {
                string pointString = p.X.ToString() + "," + p.Y.ToString() + "," + p.Z.ToString();
                centerPointsString.Add(pointString);
            }
            string centerVector = 4 * (forceVec.X) / pointsCount + "," + 4 * (forceVec.Y) / pointsCount + "," + 4 * (forceVec.Z) / pointsCount;

            List<string> centerPointLoads = new List<string>();
            foreach (string s in centerPointsString)
            {
                centerPointLoads.Add(s + ";" + centerVector);
            }

            //Line
            foreach (Point3d p in linePoints)
            {
                string pointString = p.X.ToString() + "," + p.Y.ToString() + "," + p.Z.ToString();
                linePointsString.Add(pointString);
            }
            string lineVector = 2 * (forceVec.X) / pointsCount + "," + 2 * (forceVec.Y) / pointsCount + "," + 2 * (forceVec.Z) / pointsCount;

            List<string> linePointLoads = new List<string>();
            foreach (string s in linePointsString)
            {
                linePointLoads.Add(s + ";" + lineVector);
            }

            //Corner
            foreach (Point3d p in cornerPoints)
            {
                string pointString = p.X.ToString() + "," + p.Y.ToString() + "," + p.Z.ToString();
                cornerPointsString.Add(pointString);
            }
            string cornerVector = (forceVec.X) / pointsCount + "," + (forceVec.Y) / pointsCount + "," + (forceVec.Z) / pointsCount;

            List<string> cornerPointLoads = new List<string>();
            foreach (string s in cornerPointsString)
            {
                cornerPointLoads.Add(s + ";" + cornerVector);
            }

            pointLoads.AddRange(centerPointLoads);
            pointLoads.AddRange(linePointLoads);
            pointLoads.AddRange(cornerPointLoads);

            return pointLoads;
        }

        public Point3d[] SortNodes(Point3d[] nodes, Point3d centroid)
        {

            Point3d[] lowerNodes = new Point3d[4];
            Point3d[] upperNodes = new Point3d[4];

            double[] lowerAngles = new double[nodes.Length / 2];
            double[] upperAngles = new double[nodes.Length / 2];

            //Dividing in lower and upper nodes.
            for (int i = 0; i < nodes.Length / 2; i++)
            {
                lowerNodes[i] = nodes[i];
                upperNodes[i] = nodes[nodes.Length / 2 + i];
            }


            for (int i = 0; i < nodes.Length / 2; i++)
            {
                lowerAngles[i] = (180 / Math.PI) * Math.Atan2(lowerNodes[i].Y - centroid.Y, lowerNodes[i].X - centroid.X);
            }

            Array.Sort(lowerAngles, lowerNodes);



            for (int i = 0; i < nodes.Length / 2; i++)
            {
                upperAngles[i] = (180 / Math.PI) * Math.Atan2(upperNodes[i].Y - centroid.Y, upperNodes[i].X - centroid.X);
            }

            Array.Sort(upperAngles, upperNodes);

            Point3d[] nodesAll = new Point3d[8];

            nodesAll[0] = lowerNodes[0];
            nodesAll[1] = lowerNodes[1];
            nodesAll[2] = lowerNodes[2];
            nodesAll[3] = lowerNodes[3];
            nodesAll[4] = upperNodes[0];
            nodesAll[5] = upperNodes[1];
            nodesAll[6] = upperNodes[2];
            nodesAll[7] = upperNodes[3];


            return nodesAll;
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("9fa57f7d-4d01-4004-aa03-214e25f02b6a"); }
        }
    }
}
