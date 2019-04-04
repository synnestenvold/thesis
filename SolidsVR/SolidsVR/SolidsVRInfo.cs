using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace SolidsVR
{
    public class SolidsVRInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "SolidsVR";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("33d9b9d2-22ea-49ed-af2e-1bbc18ee82ab");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "HP Inc.";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "";
            }
        }
    }
}
