using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpriteGenerator.Utility
{
    static class Algorithm
    {
        /// <summary>
        /// Greedy algorithm.
        /// </summary>
        /// <param name="modules">List of modules that represent the images that need to be inserted into the sprite.</param>
        /// <returns>Near optimal placement.</returns>
        public static Placement Greedy(List<Module> modules, ILogger logger)
        {
            //Empty O-Tree code.
            OTree oTree = new OTree();
            OT finalOt = null;
            //Empty list of modules.
            List<Module> moduleList = new List<Module>();

            //For each module which needs to be inserted.
            var i = 0;
            var total = modules.Count;
            var step = Math.Ceiling(total * .1);
            foreach (Module module in modules)
            {
                OTree bestOTree = null;
                //Add module to the list of already packed modules.
                moduleList.Add(module);
                //Set the minimum perimeter of the placement to high.
                int minPerimeter = Int32.MaxValue;

                //Try all insertation point.
                foreach (int insertationPoint in oTree.InsertationPoints())
                {
                    OTree ot = oTree.Copy();
                    ot.Insert(module.Name, insertationPoint);
                    OT oT = new OT(ot, moduleList);
                    Placement pm = oT.Placement;

                    //Choose the one with the minimum perimeter.
                    if (pm.Perimeter < minPerimeter)
                    {
                        finalOt = oT;
                        bestOTree = ot;
                        minPerimeter = pm.Perimeter;
                    }
                }
                oTree = bestOTree;

                i++;
                if (i % step == 0)
                {
                    logger.Log("... {0:0.00}%", ((double)i / (double)total) * 100);
                }
            }

            return finalOt.Placement;
        }
    }
}
