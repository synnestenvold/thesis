using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace PartitionSlider
{
    public class PartitionSliderInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "PartitionSlider";
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
                return new Guid("6efdd853-7f00-4f22-821c-6d1166a64616");
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
