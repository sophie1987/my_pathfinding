using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

public class Cluster_New // cluster包含Node，node包含grid
{
    public Boundaries Boundaries; //边界，由左上角和右下角gridtile组成
    public Dictionary<GridTile, Node> Nodes; //这个cluster中所有可通行的nodes

    public int Width; // 宽度
    public int Height; // 高度

    public int Type; // 当前cluster的属性，为-1表示混合cluster，>0表示组成Cluster的各grid权重相同，且为各grid的权重
    public int Region_ID; // 当前各cluster的Region_ID，初始为-1，表示未归类。
    public Int2 index;

    /*public Cluster_New left_cluster;
    public Cluster_New right_cluster;
    public Cluster_New top_cluster;
    public Cluster_New bottom_cluster;*/

    public Cluster_New()
    {
        Boundaries = new Boundaries();
        Nodes = new Dictionary<GridTile, Node>();
        Type = -1;
        Region_ID = -1;
        index = new Int2(-1,-1);
        /*left_cluster = null;
        right_cluster = null;
        top_cluster = null;
        bottom_cluster = null;*/
    }

    public string getBoundaries()
    {
        return "左上角（" + this.Boundaries.Min.x + "," + this.Boundaries.Min.y + "),右下角（" + this.Boundaries.Max.x + "," + this.Boundaries.Max.y + ")";
    }

    public void getPresent()
    {
        Console.WriteLine("Boundaries:" + this.getBoundaries());
        /*Console.WriteLine("Type:" + this.Type);
        if (left_cluster != null)
            Console.WriteLine("left cluster boundaries:" + left_cluster.getBoundaries());
        if (right_cluster != null)
            Console.WriteLine("right cluster boundaries:" + right_cluster.getBoundaries());
        if (top_cluster != null)
            Console.WriteLine("top cluster boundaries:" + top_cluster.getBoundaries());
        if (bottom_cluster != null)
            Console.WriteLine("bottom cluster boundaries:" + bottom_cluster.getBoundaries());*/

        /*foreach (Node node in this.Nodes.Values)
        {
            Console.WriteLine(node.getPos());
            Console.WriteLine(node.getEdges());
            Console.WriteLine(node.getChild());
        }*/
    }

    //Check if this cluster contains the other cluster (by looking at boundaries)
    /*public bool Contains(Cluster other)
    {
        return other.Boundaries.Min.x >= Boundaries.Min.x &&
                other.Boundaries.Min.y >= Boundaries.Min.y &&
                other.Boundaries.Max.x <= Boundaries.Max.x &&
                other.Boundaries.Max.y <= Boundaries.Max.y;
    }*/

    public bool Contains(GridTile pos) // 判断这个cluster是否包含这个节点
    {
        return pos.x >= Boundaries.Min.x &&
            pos.x <= Boundaries.Max.x &&
            pos.y >= Boundaries.Min.y &&
            pos.y <= Boundaries.Max.y;
    }

}

