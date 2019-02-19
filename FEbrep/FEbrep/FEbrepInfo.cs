using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace FEbrep
{
    public class FEbrepInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "FEbrep";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                return FEbrep.Properties.Resources.Icon1.ToBitmap();
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
                return new Guid("0d505f88-4a82-4988-b49b-e442eca9e674");
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
