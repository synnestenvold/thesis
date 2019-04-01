using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System.Drawing;
using System.Linq;

namespace ViewStresses
{
    public class ViewStressesComponent : GH_Component
    {
        Dictionary<Brep, Color> models = new Dictionary<Brep, Color>();

        public ViewStressesComponent()
          : base("ViewStresses", "ViewStress",
              "View stress",
              "Category3", "Preview")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            
            pManager.AddIntegerParameter("Connectivity", "C", "", GH_ParamAccess.tree);
            pManager.AddPointParameter("Points for Breps", "N", "Breps in coordinates", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Stresses", "Stress", "Stresses in each node", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Stress direction", "Stress dir", "S11, S22, S33, S12, S13, S23 as 0, 2, 3, 4, 5, 6", GH_ParamAccess.item);
            //pManager.AddNumberParameter("Yield limit", "Y", "The limit for coloring Green/Red", GH_ParamAccess.item);
            pManager.AddNumberParameter("Displacement", "Disp", "Displacement in each dof", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Scaling", "Scale", "Scale factor for the view", GH_ParamAccess.item, 1);
            pManager.AddBrepParameter("Brep", "B", "Original brep for preview", GH_ParamAccess.item);
            pManager[5].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "M", "3d Model of stresses", GH_ParamAccess.list);
            pManager.AddColourParameter("Colors", "C", "Colors for breps", GH_ParamAccess.list);
            pManager.AddGenericParameter("Stress range", "SR", "Breps for stress range", GH_ParamAccess.list);
            pManager.AddColourParameter("Colors", "C", "Colors for stress range", GH_ParamAccess.list);
            pManager.AddTextParameter("Text", "T", "Text", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Plane", "P", "Placement for text", GH_ParamAccess.list);
            pManager.AddColourParameter("Colors", "C", "Colors for text", GH_ParamAccess.list);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Brep> breps = new List<Brep>();
            List<Brep> coloredBreps = new List<Brep>();
            Dictionary<Brep, Color> tmpModels = new Dictionary<Brep, Color>();
            GH_Structure<GH_Point> treePoints = new GH_Structure<GH_Point>();
            GH_Structure<GH_Integer> treeConnect = new GH_Structure<GH_Integer>();
            GH_Structure<GH_Number> treeStress = new GH_Structure<GH_Number>();
            int dir = new int();
            GH_Structure<GH_Number> treeDef = new GH_Structure<GH_Number>();
            double scale = new double();
            List<Color> colors = new List<Color>();
            Brep origBrep = new Brep();

            if (!DA.GetDataTree(1, out treePoints)) return;
            if (!DA.GetDataTree(0, out treeConnect)) return;
            if (!DA.GetDataTree(2, out treeStress)) return;
            if (!DA.GetData(3, ref dir)) return;
            if (!DA.GetDataTree(4, out treeDef)) return;
            if (!DA.GetData(5, ref scale)) return;
            if (!DA.GetData(6, ref origBrep)) return;

            VolumeMassProperties vmp = VolumeMassProperties.Compute(origBrep);

            Point3d centroid = vmp.Centroid;
            double volume = origBrep.GetVolume();
            double angle = 90*Math.PI/180;

            double sqrt3 = (double)1 / 3;
            double refLength = Math.Pow(volume, sqrt3);

            Point3d center = Point3d.Add(centroid, new Point3d(0, -refLength * 2.5, 0));

            Vector3d[] defVectors = new Vector3d[treeDef.PathCount];
            defVectors = CreateVectors(treeDef, scale);
            breps = CreateDefBreps(treePoints, treeConnect, defVectors, angle, center);
            var tuple1 = ColorBreps(breps, treeConnect, treeStress, dir, colors);
            tmpModels = tuple1.Item1;
            List<string> rangeValues = tuple1.Item2;


            Dictionary<Brep, Color> tmpRanges = new Dictionary<Brep, Color>();
            List<Color> colorRange = CreateColorRange();
            
            var tuple2 = CreateBrepRanges(centroid, refLength, center, angle);

            List<Brep> brepRanges = tuple2.Item1;
            List<Plane> planeRanges = tuple2.Item2;

            tmpRanges = CreateRanges(brepRanges, colorRange);

            Color textColor = Color.Black;
            

            //Output
            foreach (var m in tmpModels)
            {
                models[m.Key] = m.Value;
            }

            foreach (var m in tmpRanges)
            {
                models[m.Key] = m.Value;
            }

            DA.SetDataList(0, tmpModels.Keys);
            DA.SetDataList(1, colors);
            DA.SetDataList(2, tmpRanges.Keys);
            DA.SetDataList(3, colorRange);
            DA.SetDataList(4, rangeValues);
            DA.SetDataList(5, planeRanges);
            DA.SetData(6, textColor);
        }

        public List<Color> CreateColorRange()
        {
            List<Color> colorRange = new List<Color>();

            colorRange.Add(Color.Blue);
            colorRange.Add(Color.RoyalBlue);
            colorRange.Add(Color.DeepSkyBlue);
            colorRange.Add(Color.Cyan);
            colorRange.Add(Color.PaleGreen);
            colorRange.Add(Color.LimeGreen);
            colorRange.Add(Color.Lime);
            colorRange.Add(Color.Lime);
            colorRange.Add(Color.GreenYellow);
            colorRange.Add(Color.Yellow);
            colorRange.Add(Color.Orange);
            colorRange.Add(Color.OrangeRed);
            colorRange.Add(Color.Red);

            return colorRange;
        }


        public Tuple<List<Brep>, List<Plane>> CreateBrepRanges(Point3d centroid, double refLength, Point3d center, double angle)
        {
            List<Brep> brepRanges = new List<Brep>();
            List<Plane> planeRanges = new List<Plane>();
            Brep brep_new = new Brep();

            int ranges = 13;
            double totLength = refLength * 1.5;
            double rangeHeight = totLength / ranges;

            Vector3d rangePos = new Vector3d(refLength,0, refLength );
            Point3d startRanges = centroid + rangePos;

            Vector3d vecBrep = new Vector3d(0, 0, 1);

            Plane plane = new Plane(new Point3d(0,0,0), vecBrep);

            for(int i = 0; i < ranges; i++)
            {
                Interval x = new Interval(startRanges.X, startRanges.X + refLength);
                Interval y = new Interval(startRanges.Y, startRanges.Y + refLength * 0.2);
                Interval z = new Interval(startRanges.Z + rangeHeight*i, startRanges.Z + rangeHeight * (i+1));
                Box box_new = new Box(Plane.WorldXY, x, y, z);
                brep_new = box_new.ToBrep();

                Vector3d vecAxis = new Vector3d(0, 0, 1);
                brep_new.Rotate(angle, vecAxis, center);

                brepRanges.Add(brep_new);

                Point3d[] points = brep_new.DuplicateVertices();

                vecBrep = points[1] - points[0];

                Point3d p0 = points[0] + vecBrep;
                Point3d p1 = points[1] + vecBrep;
                Point3d p2 = points[4] + vecBrep;

                plane = new Plane(p0, p1, p2);

                planeRanges.Add(plane);

            }


            plane.Translate(new Vector3d(0, 0, rangeHeight));
            

            planeRanges.Add(plane);

            plane.Translate(new Vector3d(0, 0, rangeHeight));
            plane.Translate(-vecBrep);

            planeRanges.Add(plane);

            return Tuple.Create(brepRanges, planeRanges);
        }
            
           

        public Dictionary<Brep, Color> CreateRanges(List<Brep> breps, List<Color> color)
        {
            Dictionary<Brep, Color> tmpRanges = new Dictionary<Brep, Color>();

            for(int i = 0; i < breps.Count; i++)
            {
                tmpRanges[breps[i]] = color[i];
            }

            return tmpRanges;
        }

        

        public Vector3d CreateTransVector(Point3d centroid, double refLength, double angle)
        {
            double factor = 2.5;

            double side1 = refLength*factor;
            double side2 = side1 * Math.Tan(45*Math.PI/180);

            Vector3d transVec = new Vector3d(centroid.X-side1, centroid.Y-side2, 0);

            return transVec;
        }

        public Vector3d[] CreateVectors(GH_Structure<GH_Number> treeDef, double scale)
        {
            int number = treeDef.PathCount;
            Vector3d[] vectors = new Vector3d[number];
            for (int i = 0; i < number; i++)
            {
                List<GH_Number> def = (List<GH_Number>)treeDef.get_Branch(i);
                Vector3d vector = new Vector3d((def[0].Value) * scale, (def[1].Value) * scale, (def[2].Value) * scale);
                vectors[i] = vector;
            }
            return vectors;
        }

        public List<Brep> CreateDefBreps(GH_Structure<GH_Point> treePoints, GH_Structure<GH_Integer> treeConnect, Vector3d[] defVectors, double angle, Point3d center)
        {
            List<Brep> breps = new List<Brep>();
            for (int j = 0; j < treePoints.PathCount; j++)
            {
                List<GH_Point> vertices = (List<GH_Point>)treePoints.get_Branch(j);
                var mesh = new Mesh();
                List<GH_Integer> connect = (List<GH_Integer>)treeConnect.get_Branch(j);

                for (int i = 0; i < vertices.Count; i++)
                {
                    GH_Point p = vertices[i];
                    //Vector3d totVec = Vector3d.Add(defVectors[connect[i].Value], transVec);
                   // Point3d new_p = Point3d.Add(p.Value, totVec);
                    mesh.Vertices.Add(p.Value);
                }
                mesh.Faces.AddFace(0, 1, 5, 4);
                mesh.Faces.AddFace(1, 2, 6, 5);
                mesh.Faces.AddFace(2, 3, 7, 6);
                mesh.Faces.AddFace(0, 3, 7, 4);
                mesh.Faces.AddFace(4, 5, 6, 7);
                mesh.Faces.AddFace(0, 1, 2, 3);

                Brep new_brep = Brep.CreateFromMesh(mesh, false);

                ///////THIS FOR ROTATION OF THE WHOLE CUBE. First we rotate one small brep for its own centroid, then for global.
                /*
                
                Point3d centroid_local = FindCentroidRectangle(points);
                Point3d centroid_global = Point3d.Add(centroid, transVec);
                
                new_brep.Rotate(angle * 2 * Math.PI / 180, vecAxis, centroid_local);
                new_brep.Rotate(angle * 2 * Math.PI / 180, vecAxis, centroid_global);

                ////// END ROTATION
                ///
                */

                Point3d[] points = new_brep.DuplicateVertices();
                Vector3d vecAxis = points[4] - points[0];
                new_brep.Rotate(angle, vecAxis, center);

                breps.Add(new_brep);

            }
            return breps;
        }

        public Tuple<Dictionary<Brep, Color>,List<string>> ColorBreps(List<Brep> breps, GH_Structure<GH_Integer> treeConnect, GH_Structure<GH_Number> treeStress, int dir, List<Color> colors)
        {
            Dictionary<Brep, Color> tmpModels = new Dictionary<Brep, Color>();
            List<Brep> coloredBreps = new List<Brep>();
            Color color = Color.White;
            List<string> rangeValues = new List<String>();

            double[] averageValues = AverageValuesStress(treeStress, treeConnect, dir);
            double max = averageValues.Max();
            double min = averageValues.Min();
            double range = (max - min) / 13;


            //Adding values to stress range
            for(int i = 0; i <= 13; i++)
            {
                rangeValues.Add("_"+(Math.Round(min + i * range,4)).ToString());
            }

            //Creating header
            if (dir == 0) rangeValues.Add("S,xx [MPa]");
            else if (dir == 1) rangeValues.Add("S,yy [Mpa]");
            else if (dir == 2) rangeValues.Add("S,zz [Mpa]");
            else if (dir == 3) rangeValues.Add("S,yz [Mpa]");
            else if (dir == 4) rangeValues.Add("S,xz [Mpa]");
            else if (dir == 5) rangeValues.Add("S,zy [Mpa]");

           

            for (int i = 0; i < breps.Count; i++)
            {
                if (averageValues[i] < min + range) color = Color.Blue;
                else if (averageValues[i] < min + 2 * range) color = Color.RoyalBlue;
                else if (averageValues[i] < min + 3 * range) color = Color.DeepSkyBlue;
                else if (averageValues[i] < min + 4 * range) color = Color.Cyan;
                else if (averageValues[i] < min + 5 * range) color = Color.PaleGreen;
                else if (averageValues[i] < min + 6 * range) color = Color.LimeGreen;
                else if (averageValues[i] < min + 7 * range) color = Color.Lime;
                else if (averageValues[i] < min + 8 * range) color = Color.Lime;
                else if (averageValues[i] < min + 9 * range) color = Color.GreenYellow;
                else if (averageValues[i] < min + 10 * range) color = Color.Yellow;
                else if (averageValues[i] < min + 11 * range) color = Color.Orange;
                else if (averageValues[i] < min + 12 * range) color = Color.OrangeRed;
                else color = Color.Red;
                tmpModels[breps[i]] = color;
                colors.Add(color);
            }

            return Tuple.Create(tmpModels,rangeValues);

    
        }

        public double[] AverageValuesStress(GH_Structure<GH_Number> treeStress, GH_Structure<GH_Integer> treeConnect, int dir)
        {
            double[] averageList = new double[treeConnect.PathCount];

            for (int i = 0; i < treeConnect.PathCount; i++)
            {
                double average = 0;
                List<GH_Integer> connect = (List<GH_Integer>)treeConnect.get_Branch(i);
                for (int j = 0; j < connect.Count; j++)
                {
                    List<GH_Number> stresses = (List<GH_Number>)treeStress.get_Branch(connect[j].Value);
                    average += stresses[dir].Value;
                }
                averageList[i] = Math.Round(average / connect.Count, 4);
            }
            
            return averageList;
        }

        public Point3d FindCentroidRectangle(Point3d[] pList)
        {

            double c_x = 0;
            double c_y = 0;
            double c_z = 0;

            foreach (Point3d p in pList)
            {
                c_x += p.X;
                c_y += p.Y;
                c_z += p.Z;
            }

            Point3d centroid = new Point3d(c_x / 8, c_y / 8, c_z / 8);

            return centroid;
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return null;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("0f728642-ce7d-4508-95dd-4304ceb4d175"); }
        }
        public override void ExpireSolution(bool recompute)
        {
            models.Clear();
            base.ExpireSolution(recompute);
        }

        //public override BoundingBox ClippingBox => models.Keys.ToList()[0].GetBoundingBox(false);
        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            foreach (var m in models)
            {
                args.Display.DrawBrepShaded(m.Key, new Rhino.Display.DisplayMaterial(m.Value));
            }
            //base.DrawViewportMeshes(args);
        }

        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            //base.DrawViewportWires(args);
        }
    }
}
