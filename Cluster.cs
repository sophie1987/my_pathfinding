using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;


/// <summary>
/// Domain-independent, rectangular clusters，合并后的cluster
/// </summary>
public class Cluster // cluster包含Node，node包含grid
{
    //Boundaries of the cluster (with respect to the original map)
    public Boundaries Boundaries; //边界，由左上角和右下角gridtile组成
    public Dictionary<GridTile, Node> Nodes; //这个cluster中所有可通行的nodes

    //Clusters from the lower level
    public List<Cluster> Clusters; // 下一层cluster列表

    public int Width; // 宽度
    public int Height; // 高度

    public Cluster()
    {
        Boundaries = new Boundaries();
        Nodes = new Dictionary<GridTile, Node>();
    }

    public string getBoundaries()
    {
        return "左上角（" + this.Boundaries.Min.x + "," + this.Boundaries.Min.y + "),右下角（" + this.Boundaries.Max.x + "," + this.Boundaries.Max.y + ")";
    }

    public void getPresent()
    {
        Console.WriteLine("Boundaries" + this.getBoundaries());
        foreach(Node node in this.Nodes.Values)
        {
            Console.WriteLine(node.getPos());
            Console.WriteLine(node.getEdges());
            Console.WriteLine(node.getChild());
        }
    }

    //Check if this cluster contains the other cluster (by looking at boundaries)
    public bool Contains(Cluster other)
    {
        return other.Boundaries.Min.x >= Boundaries.Min.x &&
                other.Boundaries.Min.y >= Boundaries.Min.y &&
                other.Boundaries.Max.x <= Boundaries.Max.x &&
                other.Boundaries.Max.y <= Boundaries.Max.y;
    }

    public bool Contains(GridTile pos) // 判断这个cluster是否包含这个节点
    {
        return pos.x >= Boundaries.Min.x &&
            pos.x <= Boundaries.Max.x &&
            pos.y >= Boundaries.Min.y &&
            pos.y <= Boundaries.Max.y;
    }

}

