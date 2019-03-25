﻿using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace CentroidBrep
{
    public class CentroidBrepInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "CentroidBrep";
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
                return new Guid("f7f8b34b-d50e-483f-8953-ef9065d479a2");
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
