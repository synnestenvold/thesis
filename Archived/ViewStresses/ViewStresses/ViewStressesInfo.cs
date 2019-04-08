using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace ViewStresses
{
    public class ViewStressesInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "ViewStresses";
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
                return new Guid("ae3a6d53-ed7e-4a7e-a2f1-56fc93a5af79");
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
