using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace PointsToBrep
{
    public class PointsToBrepInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "PointsToBrep";
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
                return new Guid("8661d7b9-0a90-4de5-88b6-2f821e982c9c");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "";
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
