using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class Region
    {
        public int Region_ID;
        public int Type;
        public List<Cluster_New> C;
        public Dictionary<GridTile, Node> BoundaryNodes;
        public Dictionary<GridTile, Node> AllNodes;

        public int minX = 0, minY = 0, maxX = 0, maxY = 0;

        //public JPSPlusBakedMap bakedMap;
        //public JPSPlus jpsp;
        //JPSPlus jpsp = new JPSPlus();
        //jpsp.Init(r.bakedMap);
        
        public Region() {
            Region_ID = -1;
            Type = -1;
            C= new List<Cluster_New>();
            BoundaryNodes = new Dictionary<GridTile, Node>();
            //AllNodes = new Dictionary<GridTile, Node>();
            //jpsp = new JPSPlus();
        }
        public void AddCluster(Cluster_New c)
        {
            C.Add(c);
        }

        public bool GridInRegion(GridTile g)
        {
            //if (AllNodes.ContainsKey(g))
            if (minX <= g.x && g.x <= maxX && minY <= g.y && g.y <= maxY)
                return true;
            else
                return false;
        }

        public void printNodes()
        {
            foreach (Node node in this.BoundaryNodes.Values)
            {
                Console.WriteLine(node.getPos());
                Console.WriteLine(node.getEdges());
            }
        }
    }
}
