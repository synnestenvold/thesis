using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace closestPoint
{
    public class closestPointInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "closestPoint";
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
                return new Guid("a9f4fe80-fde7-4eba-ac0a-e9b3517a1d69");
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
