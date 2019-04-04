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


namespace SolidsVR
{
    public class FEMeshTBrepComponent : GH_Component
    {


        public FEMeshTBrepComponent()
          : base("Finite Element of meshed TBrep", "FEMeshTBrep",
              "Description",
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
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // --- variables ---

            //GH_Structure<GH_Integer> treeConnectivity = new GH_Structure<GH_Integer>();
            //GH_Structure<GH_Point> treePoints = new GH_Structure<GH_Point>();
            Mesh_class mesh = new Mesh_class();
            List<string> bctxt = new List<string>();
            List<string> loadtxt = new List<string>();
            List<string> deftxt = new List<string>();
            Brep origBrep = new Brep();
            Material material = new Material();


            // --- inputs ---

            if (!DA.GetData(0, ref mesh)) return;
            if (!DA.GetDataList(1, bctxt)) return;
            if (!DA.GetDataList(2, loadtxt)) return;
            if (!DA.GetDataList(3, deftxt)) return;
            if (!DA.GetData(4, ref origBrep)) return;
            if (!DA.GetData(5, ref material)) return;

            //double E = 210000;
            //double nu = 0.3;

            // --- solve ---

            List<List<int>> connectivity = mesh.GetConnectivity();
            List<List<Point3d>> elementPoints = mesh.GetElementPoints();
            int sizeOfMatrix = mesh.GetSizeOfMatrix();
            Point3d[] globalPoints = mesh.GetGlobalPoints();

            //Create K_tot
            var tupleK_B = CreateGlobalStiffnessMatrix(connectivity, elementPoints, sizeOfMatrix, material);
            Matrix<double> K_tot = tupleK_B.Item1;

            //B_all
            List<List<Matrix<double>>> B_all = tupleK_B.Item2;

            //Create boundary condition list AND predeformations
            var tupleBC = CreateBCList(bctxt, globalPoints);
            List<int> bcNodes = tupleBC.Item1;

            var tupleDef = CreateBCList(deftxt, globalPoints);
            List<int> predefNodes = tupleDef.Item1;
            List<double> predef = tupleDef.Item2;

            //Apply boundary condition and predeformations
            K_tot = ApplyBC(K_tot, bcNodes);
            K_tot = ApplyBC(K_tot, predefNodes);

            //Needs to take the predefs into account
            Vector<double> R_def = Vector<double>.Build.Dense(sizeOfMatrix);
            if (deftxt.Any()) R_def = ApplyPreDef(K_tot, predefNodes, predef, sizeOfMatrix);

            //Inverting K matrix
            Matrix<double> K_tot_inverse = K_tot.Inverse();

            //double[] R_array = SetLoads(sizeOfM, loadtxt);
            double[] R_array = AssignLoadsDefAndBC(loadtxt, predefNodes, predef, bcNodes, globalPoints);

            //Adding R-matrix for pre-deformations.
            var V = Vector<double>.Build;
            Vector<double> R = (V.DenseOfArray(R_array)).Add(R_def);

            /*
             * For Cholesky calculation. Kept for now.
            double[] R_array_def = R.ToArray();

            double[] R_array_def = new double[sizeOfM];
            for (int j = 0; j < sizeOfM; j++)
            {
                R_array_def[j] = R[j];
            }

           //Cholesky calc. Kept for now,
           Deformations def = new Deformations(K_tot, R_array_def);
           List<double> u = def.Cholesky_Banachiewicz();
           */

            //Caluculation of the displacement vector u
            Vector<double> u = K_tot_inverse.Multiply(R);

            //Creating tree for output of deformation. Structured in x,y,z for each node.
            DataTree<double> defTree = DefToTree(u);
            
            //Calculatin strains for each node in elements
            List<List<Vector<double>>> strain = CalcStrain(u, B_all, connectivity);

            //Find the strains in each node from the strains in each element
            List<List<double>> globalStrain = FindGlobalStrain(strain, connectivity, sizeOfMatrix);

            //Calculate global stresses from strain
            List<Vector<double>> globalStress = CalcStress(globalStrain, material);

            DataTree<double> strainTree = new DataTree<double>();
            DataTree<double> stressTree = new DataTree<double>();
            for (int i = 0; i < globalStrain.Count; i++)
            {
                strainTree.AddRange(globalStrain[i], new GH_Path(new int[] { 0, i }));
                stressTree.AddRange(globalStress[i], new GH_Path(new int[] { 0, i }));
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

        }


        public Tuple<Matrix<double>, List<List<Matrix<Double>>>> CreateGlobalStiffnessMatrix(List<List<int>> connectivity, List<List<Point3d>> elementPoints, int sizeOfMatrix, Material material)
        {
            Matrix<double> K_i = Matrix<double>.Build.Dense(sizeOfMatrix, sizeOfMatrix);
            Matrix<double> K_tot = Matrix<double>.Build.Dense(sizeOfMatrix, sizeOfMatrix);
            List<Matrix<Double>> B_e = new List<Matrix<Double>>();
            List<List<Matrix<double>>> B_all = new List<List<Matrix<double>>>();
            double E = material.GetE();
            double nu = material.GetNu();
            StiffnessMatrix sm = new StiffnessMatrix(E, nu);
            Assembly_StiffnessMatrix aSM = new Assembly_StiffnessMatrix();

            for (int i = 0; i < connectivity.Count; i++)
            {
                List<int> connectedNodes = connectivity[i];
                List<Point3d> connectedPoints = elementPoints[i];

                var tuple = sm.CreateMatrix(connectedPoints);
                Matrix<double> K_e = tuple.Item1;
                B_e = tuple.Item2;
                B_all.Add(B_e);
                K_tot = aSM.AssemblyMatrix(K_tot, K_e, connectedNodes, sizeOfMatrix);
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

        public Matrix<double> ApplyBC(Matrix<double> K, List<int> bcNodes)
        {
            for (int i = 0; i < bcNodes.Count; i++)
            {
                for (int j = 0; j < K.ColumnCount; j++)
                {
                    if (bcNodes[i] != j)
                    {
                        K[bcNodes[i], j] = 0;
                    }

                }

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
            //Corresponding value in R is set to 0 if it is a BC here. Ref page 309 in FEM book.
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

        public DataTree<double> DefToTree(Vector<double> u)
        {
            DataTree<double> defTree = new DataTree<double>();
            int n = 0;
            for (int i = 0; i < u.Count; i += 3)
            {
                List<double> u_node = new List<double>(3);
                u_node.Add(u[i]);
                u_node.Add(u[i + 1]);
                u_node.Add(u[i + 2]);

                defTree.AddRange(u_node, new GH_Path(new int[] { 0, n }));
                n++;
            }

            return defTree;
        }

        public List<List<Vector<double>>> CalcStrain(Vector<double> u, List<List<Matrix<double>>> B_all, List<List<int>> connectivity)
        {
            List<Matrix<double>> B_e = new List<Matrix<double>>();
            List<int> c_e = new List<int>();
            DataTree<double> strain_node = new DataTree<double>();


            StrainCalc sC = new StrainCalc();

            List<List<Vector<double>>> strain = new List<List<Vector<double>>>();

            for (int i = 0; i < B_all.Count; i++)
            {
                B_e = B_all[i];
                c_e = connectivity[i];
                List<Vector<double>> calcedStrain = sC.StrainCalculations(B_e, u, c_e);

                strain.Add(calcedStrain);

            }

            return strain;
        }

        public List<List<double>> FindGlobalStrain(List<List<Vector<double>>> strain, List<List<int>> connectivity, int sizeOfM)
        {
            List<List<double>> globalStrain = new List<List<double>>();


            for (int i = 0; i < sizeOfM / 3; i++)
            {
                List<double> nodeStrain = Enumerable.Repeat(0d, 6).ToList();
                globalStrain.Add(nodeStrain);
            }

            for (int i = 0; i < connectivity.Count(); i++) //For each element
            {
                List<int> cNodes = connectivity[i];
                for (int j = 0; j < cNodes.Count; j++)
                {
                    List<double> TList = globalStrain[cNodes[j]];
                    for (int k = 0; k < 6; k++)
                    {


                        if (globalStrain[cNodes[j]][k] == 0)
                        {
                            globalStrain[cNodes[j]][k] = strain[i][j][k];
                        }
                        else
                        {
                            globalStrain[cNodes[j]][k] = (globalStrain[cNodes[j]][k] + strain[i][j][k]) / 2;
                        }

                    }
                }
            }

            return globalStrain;
        }

        public List<Vector<double>> CalcStress(List<List<double>> globalStrain, Material material)
        {
            List<Vector<double>> globalStress = new List<Vector<double>>();
            double E = material.GetE();
            double nu = material.GetNu();
            Cmatrix C = new Cmatrix(E, nu);
            Matrix<double> C_matrix = C.CreateMatrix();


            for (int i = 0; i < globalStrain.Count; i++)
            {
                Vector<double> strainVec = Vector<double>.Build.Dense(globalStrain[i].ToArray());
                Vector<double> stressVec = C_matrix.Multiply(strainVec);
                //Adding Mises
                var tempStressVec = Vector<double>.Build.Dense(stressVec.Count + 1);
                stressVec.Storage.CopySubVectorTo(tempStressVec.Storage, 0, 0, 6);
                double Sxx = stressVec[0];
                double Syy = stressVec[1];
                double Szz = stressVec[2];
                double Sxy = stressVec[3];
                double Sxz = stressVec[4];
                double Syz = stressVec[5];
                double mises = Math.Sqrt(0.5*(Math.Pow(Sxx-Syy, 2)+Math.Pow(Syy-Szz,2)+Math.Pow(Szz-Sxx, 2))+3*(Math.Pow(Sxy,2)+ Math.Pow(Sxz, 2)+ Math.Pow(Syz, 2)));
                tempStressVec.At(6, mises);
                globalStress.Add(tempStressVec);
            }


            return globalStress;
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
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("cec65985-58a5-4ff1-a238-e4761e0abbeb"); }
        }
        
        static void Main(string[] args)
        {
            //Control.UseManaged();
            Control.UseNativeMKL();
        }
        
    }
}

    