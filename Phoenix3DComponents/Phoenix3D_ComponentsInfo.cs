using System;
using System.Drawing;

using Grasshopper.Kernel;
using Grasshopper;

namespace Phoenix3D_Components
{
    public class Phoenix3D_ComponentsInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "Phoenix3D";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return Properties.Resources.Phoenix3D_logo_24x24;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "Plugin for stock-constrained design of structures";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("77456ab9-1008-43a9-a0a2-d579989b80a3");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "Jonas Warmuth & Jan Brütting";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "Structural Xploration Lab, EPFL Lausanne, 1700 Fribourg, Switzerland, sxl.epfl.ch";
            }
        }
    }

    public class PhoenixCategoryIcon : GH_AssemblyPriority
    {
        public override GH_LoadingInstruction PriorityLoad()
        {
            Instances.ComponentServer.AddCategoryIcon("Phoenix3D", Properties.Resources.Phoenix3D_logo_24x24);
            Instances.ComponentServer.AddCategorySymbolName("Phoenix3D", 'P');
            return GH_LoadingInstruction.Proceed;
        }
    }
}
