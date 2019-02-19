using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;

namespace FEbrep
{
    class StiffnessMatrix
    {

        private double E = 0;
        private double nu = 0;
        private double lx = 0;
        private double ly = 0;
        private double lz = 0;

        public StiffnessMatrix(double _E, double _nu, double _lx, double _ly, double _lz)
        {
            E = _E;
            nu = _nu;
            lx = _lx;
            ly = _ly;
            lz = _lz;
        }


        static void Main(string[] args)
        {
            StiffnessMatrix s = new StiffnessMatrix(10, 10, 10, 10, 10);
                
        }


    }
}
