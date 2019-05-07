using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics;
using Grasshopper;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System.Linq;
using System.Drawing;

namespace SolidsVR.Components
{
    public class TopOptComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the TopOpt class.
        /// </summary>
        public TopOptComponent()
          : base("TopOpt", "TopOpt",
              "Topology Optimization",
              "Category3", "Subcategory3")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Mesh", "Mesh", "Mesh for Brep", GH_ParamAccess.item);
            pManager.AddTextParameter("Boundary conditions", "BC", "Nodes that are constrained", GH_ParamAccess.list);
            pManager.AddTextParameter("PointLoads", "PL", "Input loads", GH_ParamAccess.list);
            pManager.AddTextParameter("PreDeformations", "PD", "Input deformations", GH_ParamAccess.list);
            pManager.AddBrepParameter("Brep", "B", "Original brep for preview", GH_ParamAccess.item);
            pManager.AddGenericParameter("Material", "M", "Material", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Displacement", "Disp", "Displacement in each dof", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Strain", "Strain", "Strain vector", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Stress", "Stress", "Stress vector", GH_ParamAccess.tree);
            pManager.AddPointParameter("Nodes", "N", "Coordinates for corner nodes in brep", GH_ParamAccess.list); //For testing only
            pManager.AddTextParameter("Text", "Text", "Text for headline", GH_ParamAccess.item);
            pManager.AddNumberParameter("Size", "Size", "Text size", GH_ParamAccess.item);
            pManager.AddPlaneParameter("Plane", "Plane", "Placement for text", GH_ParamAccess.item);
            pManager.AddColourParameter("Colors", "Color T", "Colors for text", GH_ParamAccess.item);
            pManager.AddGenericParameter("Nodes", "Nodes", "All nodes", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // --- variables ---

            Mesh_class mesh = new Mesh_class();
            List<string> bctxt = new List<string>();
            List<string> loadtxt = new List<string>();
            List<string> deftxt = new List<string>();
            Brep origBrep = new Brep();
            Material material = new Material();


            // --- input ---

            if (!DA.GetData(0, ref mesh)) return;
            if (!DA.GetDataList(1, bctxt)) return;
            if (!DA.GetDataList(2, loadtxt)) return;
            if (!DA.GetDataList(3, deftxt)) return;
            if (!DA.GetData(4, ref origBrep)) return;
            if (!DA.GetData(5, ref material)) return;

            //double E = 210000;
            //double nu = 0.3;

            // --- solve ---
            Boolean first = true;
            List<List<int>> connectivity = mesh.GetConnectivity();
            List<List<Point3d>> elementPoints = mesh.GetElementPoints();
            int sizeOfMatrix = mesh.GetSizeOfMatrix();
            Point3d[] globalPoints = mesh.GetGlobalPoints();
            List<Node> nodes = mesh.GetNodeList();
            DataTree<double> defTree = new DataTree<double>();
            //topologitest
            int n = 0;
            double max = 0;
            int removeElem = -1;
            while (n < 5 && max < 355)
            {
                
                List<Element> elements = mesh.GetElements();
                if (first != true)
                {
                    elements.RemoveAt(removeElem);
                }
                first = false;
                //Create K_tot
                var tupleK_B = CreateGlobalStiffnessMatrix(connectivity, elementPoints, sizeOfMatrix, material, elements);
                Matrix<double> K_tot = tupleK_B.Item1;

                //B_all
                List<List<Matrix<double>>> B_all = tupleK_B.Item2;

                //Create boundary condition list AND predeformations
                var tupleBC = CreateBCList(bctxt, globalPoints);
                List<int> bcNodes = tupleBC.Item1;

                var tupleDef = CreateBCList(deftxt, globalPoints);
                List<int> predefNodes = tupleDef.Item1;
                List<double> predef = tupleDef.Item2;

                //Setter 0 i hver rad med bc og predef, og diagonal til 1.
                K_tot = ApplyBC_Row(K_tot, bcNodes);
                K_tot = ApplyBC_Row(K_tot, predefNodes);

                //Needs to take the predefs into account
                Vector<double> R_def = Vector<double>.Build.Dense(sizeOfMatrix);
                if (deftxt.Any()) R_def = ApplyPreDef(K_tot, predefNodes, predef, sizeOfMatrix);

                //double[] R_array = SetLoads(sizeOfM, loadtxt);
                double[] R_array = AssignLoadsDefAndBC(loadtxt, predefNodes, predef, bcNodes, globalPoints);

                //Adding R-matrix for pre-deformations.
                var V = Vector<double>.Build;
                Vector<double> R = (V.DenseOfArray(R_array)).Subtract(R_def);

                //Apply boundary condition and predeformations (Puts 0 in columns of K)
                K_tot = ApplyBC_Col(K_tot, bcNodes);
                K_tot = ApplyBC_Col(K_tot, predefNodes);

                //Inverting K matrix. Singular when all elements belonging to a node is removed
                Matrix<double> K_tot_inverse = K_tot.Inverse();
                
                //Caluculation of the displacement vector u
                Vector<double> u = K_tot_inverse.Multiply(R);

                //Creating tree for output of deformation. Structured in x,y,z for each node. As well as asigning deformation to each node class
                defTree = DefToTree(u, nodes);

                //Calculatin strains for each node in elements
                CalcStrain(elements);

                //Calculate global stresses from strain

                CalcStress(nodes, material);
                SetAverageStresses(elements);

                var tuple = mesh.RemoveOneElement();
                max = tuple.Item1;
                removeElem = tuple.Item2;
                n++;
            }
            DataTree<double> strainTree = new DataTree<double>();
            DataTree<double> stressTree = new DataTree<double>();

            for (int i = 0; i < nodes.Count; i++)
            {
                strainTree.AddRange(nodes[i].GetGlobalStrain(), new GH_Path(new int[] { 0, i }));
                stressTree.AddRange(nodes[i].GetStress(), new GH_Path(new int[] { 0, i }));
            }

            //FOR PREVIEW OF HEADLINE

            //Setting up reference values
            var tupleRef = GetRefValues(origBrep);
            double refLength = tupleRef.Item1;
            Point3d centroid = tupleRef.Item2;

            //Creating text-information for showing in VR
            var tupleHeadline = CreateHeadline(centroid, refLength);

            string headText = tupleHeadline.Item1;
            double headSize = tupleHeadline.Item2;
            Plane headPlane = tupleHeadline.Item3;
            Color headColor = tupleHeadline.Item4;

            //---output---

            DA.SetDataTree(0, defTree);
            DA.SetDataTree(1, strainTree);
            DA.SetDataTree(2, stressTree);
            DA.SetDataList(3, globalPoints);

            DA.SetData(4, headText);
            DA.SetData(5, headSize);
            DA.SetData(6, headPlane);
            DA.SetData(7, headColor);

            DA.SetDataList(8, nodes);

        }


        public Tuple<Matrix<double>, List<List<Matrix<double>>>> CreateGlobalStiffnessMatrix(List<List<int>> connectivity, List<List<Point3d>> elementPoints, int sizeOfMatrix, Material material, List<Element> elements)
        {
            Matrix<double> K_i = Matrix<double>.Build.Dense(sizeOfMatrix, sizeOfMatrix);
            Matrix<double> K_tot = Matrix<double>.Build.Dense(sizeOfMatrix, sizeOfMatrix);
            List<Matrix<Double>> B_e = new List<Matrix<Double>>();
            List<List<Matrix<double>>> B_all = new List<List<Matrix<double>>>();
            double E = material.GetE();
            double nu = material.GetNu();
            StiffnessMatrix sm = new StiffnessMatrix(E, nu);
            Assembly_StiffnessMatrix aSM = new Assembly_StiffnessMatrix();

            for (int i = 0; i < elements.Count; i++)
            {
                List<int> connectedNodes = elements[i].GetConnectivity();
                List<Node> nodes = elements[i].GetVertices();

                var tuple = sm.CreateMatrix(nodes);
                Matrix<double> K_e = tuple.Item1;
                B_e = tuple.Item2;
                elements[i].SetStiffnessMatrix(K_e);
                elements[i].SetBMatrices(B_e);

                B_all.Add(B_e);
                K_tot = aSM.AssemblyMatrix(K_tot, K_e, connectedNodes);
            }


            return Tuple.Create(K_tot, B_all);
        }


        public Tuple<List<int>, List<double>> CreateBCList(List<string> bctxt, Point3d[] points)
        {
            List<int> BC = new List<int>();
            List<double> BCPoints = new List<double>();
            List<double> restrains = new List<double>();

            foreach (string s in bctxt)
            {
                string coordinate = (s.Split(';'))[0];
                string iBC = (s.Split(';'))[1];

                string[] coord = (coordinate.Split(','));
                string[] iBCs = (iBC.Split(','));


                BCPoints.Add(Math.Round(double.Parse(coord[0]), 8));
                BCPoints.Add(Math.Round(double.Parse(coord[1]), 8));
                BCPoints.Add(Math.Round(double.Parse(coord[2]), 8));

                restrains.Add(Math.Round(double.Parse(iBCs[0]), 8));
                restrains.Add(Math.Round(double.Parse(iBCs[1]), 8));
                restrains.Add(Math.Round(double.Parse(iBCs[2]), 8));
            }

            int index = 0;


            foreach (Point3d p in points)
            {

                for (int j = 0; j < BCPoints.Count / 3; j++)
                {
                    if (BCPoints[3 * j] == Math.Round(p.X, 8) && BCPoints[3 * j + 1] == Math.Round(p.Y, 8) && BCPoints[3 * j + 2] == Math.Round(p.Z, 8))
                    {
                        BC.Add(index);
                        BC.Add(index + 1);
                        BC.Add(index + 2);

                    }
                }
                index += 3;
            }

            return Tuple.Create(BC, restrains);
        }

        public Matrix<double> ApplyBC_Col(Matrix<double> K, List<int> bcNodes)
        {
            for (int i = 0; i < bcNodes.Count; i++)
            {
                /*for (int j = 0; j < K.ColumnCount; j++)
                {
                    if (bcNodes[i] != j)
                    {
                        K[bcNodes[i], j] = 0;
                    }

                }*/

                for (int j = 0; j < K.RowCount; j++)
                {
                    if (bcNodes[i] != j)
                    {
                        K[j, bcNodes[i]] = 0;
                    }

                }
            }
            return K;
        }

        public Matrix<double> ApplyBC_Row(Matrix<double> K, List<int> bcNodes)
        {
            for (int i = 0; i < bcNodes.Count; i++)
            {


                for (int j = 0; j < K.RowCount; j++)
                {
                    if (bcNodes[i] != j)
                    {
                        K[bcNodes[i], j] = 0;
                    }
                    else
                    {
                        K[bcNodes[i], j] = 1;
                    }

                }
            }
            return K;
        }


        public Vector<double> ApplyPreDef(Matrix<double> K_tot, List<int> predefNodes, List<double> predef, int sizeOfM)
        {
            if (predefNodes.Count == 0) return Vector<double>.Build.Dense(sizeOfM);

            //Pick the parts of K that are prescribed a deformation
            Matrix<double> K_red = Matrix<double>.Build.Dense(sizeOfM, predefNodes.Count);
            int n = 0;
            foreach (int dof in predefNodes)
            {
                for (int j = 0; j < sizeOfM; j++)
                {
                    K_red[j, n] = K_tot[j, dof];
                }

                n++;
            }

            //Create a vector of the deformations
            Vector<double> d = Vector<double>.Build.Dense(predefNodes.Count);
            for (int i = 0; i < predefNodes.Count; i++)
            {
                d[i] = predef[i];
            }

            //Multiply this with K_red
            Vector<double> R_def = K_red.Multiply(d);
            for (int i = 0; i < predefNodes.Count; i++)
            {
                R_def[predefNodes[i]] = 0;
            }


            return R_def;
        }

        public double[] AssignLoadsDefAndBC(List<string> pointLoads, List<int> predefNodes, List<double> predef, List<int> bcNodes, Point3d[] points)
        {
            List<double> loadCoord = new List<double>();
            List<double> pointValues = new List<double>();

            double[] loads = new double[points.Length * 3];

            foreach (string s in pointLoads)
            {
                string coordinate = (s.Split(';'))[0];
                string iLoad = (s.Split(';'))[1];

                string[] coord = (coordinate.Split(','));
                string[] iLoads = (iLoad.Split(','));

                loadCoord.Add(Math.Round(double.Parse(coord[0]), 8));
                loadCoord.Add(Math.Round(double.Parse(coord[1]), 8));
                loadCoord.Add(Math.Round(double.Parse(coord[2]), 8));

                pointValues.Add(Math.Round(double.Parse(iLoads[0]), 8));
                pointValues.Add(Math.Round(double.Parse(iLoads[1]), 8));
                pointValues.Add(Math.Round(double.Parse(iLoads[2]), 8));
            }

            int index = 0;

            foreach (Point3d p in points)
            {

                for (int j = 0; j < loadCoord.Count / 3; j++)
                {
                    if (loadCoord[3 * j] == Math.Round(p.X, 8) && loadCoord[3 * j + 1] == Math.Round(p.Y, 8) && loadCoord[3 * j + 2] == Math.Round(p.Z, 8))
                    {
                        loads[index] = pointValues[3 * j];
                        loads[index + 1] = pointValues[3 * j + 1];
                        loads[index + 2] = pointValues[3 * j + 2];
                    }
                }
                index += 3;
            }
            //Corresponding value in R is set to 0 if it is a BC/predef here. Ref page 309 in FEM book.
            foreach (int bc in bcNodes)
            {
                loads[bc] = 0;
            }
            for (int i = 0; i < predefNodes.Count; i++)
            {
                loads[predefNodes[i]] = predef[i];
            }

            return loads;
        }

        public DataTree<double> DefToTree(Vector<double> u, List<Node> nodes)
        {
            DataTree<double> defTree = new DataTree<double>();
            int n = 0;
            for (int i = 0; i < u.Count; i += 3)
            {
                List<double> u_node = new List<double>(3);
                u_node.Add(u[i]);
                u_node.Add(u[i + 1]);
                u_node.Add(u[i + 2]);

                nodes[i / 3].SetDeformation(u_node);

                defTree.AddRange(u_node, new GH_Path(new int[] { 0, n }));
                n++;
            }

            return defTree;
        }

        public void CalcStrain(List<Element> elements)
        {
            List<Matrix<double>> B_e = new List<Matrix<double>>();
            List<int> c_e = new List<int>();
            List<Node> nodes_e = new List<Node>();

            StrainCalc sC = new StrainCalc();

            for (int i = 0; i < elements.Count; i++)
            {
                B_e = elements[i].GetBMatrixes();
                nodes_e = elements[i].GetVertices();

                sC.StrainCalculations(B_e, nodes_e);
            }

        }



        public void CalcStress(List<Node> nodes, Material material)
        {
            double E = material.GetE();
            double nu = material.GetNu();
            Cmatrix C = new Cmatrix(E, nu);
            Matrix<double> C_matrix = C.CreateMatrix();

            for (int i = 0; i < nodes.Count; i++)
            {
                Vector<double> globalStress = C_matrix.Multiply(nodes[i].GetGlobalStrain());

                //Adding Mises
                var tempStressVec = Vector<double>.Build.Dense(globalStress.Count + 1);
                globalStress.Storage.CopySubVectorTo(tempStressVec.Storage, 0, 0, 6);
                double Sxx = globalStress[0];
                double Syy = globalStress[1];
                double Szz = globalStress[2];
                double Sxy = globalStress[3];
                double Sxz = globalStress[4];
                double Syz = globalStress[5];
                double mises = Math.Sqrt(0.5 * (Math.Pow(Sxx - Syy, 2) + Math.Pow(Syy - Szz, 2) + Math.Pow(Szz - Sxx, 2)) + 3 * (Math.Pow(Sxy, 2) + Math.Pow(Sxz, 2) + Math.Pow(Syz, 2)));
                tempStressVec.At(6, mises);

                nodes[i].SetStress(tempStressVec);

            }

        }

        public void SetAverageStresses(List<Element> elements)
        {
            for (int i = 0; i < elements.Count; i++)
            {
                elements[i].SetAverageValuesStress();
            }
        }

        public Tuple<double, Point3d> GetRefValues(Brep origBrep)
        {
            VolumeMassProperties vmp = VolumeMassProperties.Compute(origBrep);
            Point3d centroid = vmp.Centroid;
            double volume = origBrep.GetVolume();
            double sqrt3 = (double)1 / 3;
            double refLength = Math.Pow(volume, sqrt3);

            return Tuple.Create(refLength, centroid);
        }

        public Tuple<string, double, Plane, Color> CreateHeadline(Point3d centroid, double refLength)
        {
            string headText = "MODEL";

            double headSize = (double)refLength / 2;

            Point3d p0 = centroid;
            Point3d p1 = Point3d.Add(p0, new Point3d(1, 0, 0));
            Point3d p2 = Point3d.Add(p0, new Point3d(0, 0, 1));

            Plane headPlane = new Plane(p0, p1, p2);
            headPlane.Translate(new Vector3d(0, 0, refLength));

            Color headColor = Color.Pink;

            return Tuple.Create(headText, headSize, headPlane, headColor);
        }


        private static bool IsSymmetric(Matrix<Double> K)
        {

            for (int i = 0; i < K.RowCount; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    if (K[i, j] != K[j, i])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public Matrix<double> RoundOf(Matrix<Double> K)
        {
            for (int i = 0; i < K.RowCount; i++)
            {
                for (int j = 0; j < K.ColumnCount; j++)
                {
                    K[i, j] = Math.Round(K[i, j], 1);
                }
            }

            return K;
        }







        public List<Vector<double>> CalcStress(List<Vector<double>> calcedStrain, Matrix<double> C_matrix)
        {

            DataTree<double> treeStress = new DataTree<double>();
            List<Vector<double>> calcedStress = new List<Vector<double>>();
            for (int i = 0; i < calcedStrain.Count; i++)
            {
                calcedStress.Add(C_matrix.Multiply(calcedStrain[i]));

            }

            return calcedStress;
        }
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("f0cb3c4f-cdc0-4e69-b554-84d9e3e112f4"); }
        }
    }
}