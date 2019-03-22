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
            pManager.AddPointParameter("Points for Breps", "P", "Breps in coordinates", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Connectivity", "C", "", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Stresses", "S", "Stresses in each node", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Stress direction", "Direction", "S11, S22, S33, S12, S13, S23 as 0, 2, 3, 4, 5, 6", GH_ParamAccess.item);
            //pManager.AddNumberParameter("Yield limit", "Y", "The limit for coloring Green/Red", GH_ParamAccess.item);
            pManager.AddNumberParameter("Displacement", "Disp", "Displacement in each dof", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Scaling", "Scale", "Scale factor for the view", GH_ParamAccess.item, 1);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "M", "3d Model", GH_ParamAccess.list);
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

            if (!DA.GetDataTree(0, out treePoints)) return;
            if (!DA.GetDataTree(1, out treeConnect)) return;
            if (!DA.GetDataTree(2, out treeStress)) return;
            if (!DA.GetData(3, ref dir)) return;
            if (!DA.GetDataTree(4, out treeDef)) return;
            if (!DA.GetData(5, ref scale)) return;

            Vector3d[] defVectors = new Vector3d[treeDef.PathCount];
            defVectors = CreateVectors(treeDef, scale);
            breps = CreateDefBreps(treePoints, treeConnect, defVectors);
            tmpModels = ColorBreps(breps, treeConnect, treeStress, dir);
            
            //Output
            foreach (var m in tmpModels)
            {
                models[m.Key] = m.Value;
            }

            DA.SetDataList(0, tmpModels.Keys);
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

        public List<Brep> CreateDefBreps(GH_Structure<GH_Point> treePoints, GH_Structure<GH_Integer> treeConnect, Vector3d[] defVectors)
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
                    Point3d new_p = Point3d.Add(p.Value, defVectors[connect[i].Value]);
                    mesh.Vertices.Add(new_p);
                }
                mesh.Faces.AddFace(0, 1, 5, 4);
                mesh.Faces.AddFace(1, 2, 6, 5);
                mesh.Faces.AddFace(2, 3, 7, 6);
                mesh.Faces.AddFace(0, 3, 7, 4);
                mesh.Faces.AddFace(4, 5, 6, 7);
                mesh.Faces.AddFace(0, 1, 2, 3);

                Brep new_brep = Brep.CreateFromMesh(mesh, false);
                breps.Add(new_brep);

            }
            return breps;
        }

        public Dictionary<Brep, Color> ColorBreps(List<Brep> breps, GH_Structure<GH_Integer> treeConnect, GH_Structure<GH_Number> treeStress, int dir)
        {
            Dictionary<Brep, Color> tmpModels = new Dictionary<Brep, Color>();
            List<Brep> coloredBreps = new List<Brep>();
            Color color = Color.White;
            double[] averageValues = AverageValuesStress(treeStress, treeConnect, dir);
            double max = averageValues.Max();
            double min = averageValues.Min();
            double range = (max - min) / 9;

            for (int i = 0; i < breps.Count; i++)
            {
                if (averageValues[i] < min + range) color = Color.Blue;
                else if (averageValues[i] < min + 2 * range) color = Color.Aqua;
                else if (averageValues[i] < min + 3 * range) color = Color.Teal;
                else if (averageValues[i] < min + 4 * range) color = Color.Olive;
                else if (averageValues[i] < min + 5 * range) color = Color.Green;
                else if (averageValues[i] < min + 6 * range) color = Color.Lime;
                else if (averageValues[i] < min + 7 * range) color = Color.Yellow;
                else if (averageValues[i] < min + 8 * range) color = Color.Orange;
                else color = Color.Red;
                tmpModels[breps[i]] = color;
            }
            return tmpModels;
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
                averageList[i] = average / connect.Count;
            }
            
            return averageList;
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
