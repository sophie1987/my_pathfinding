using ConsoleApp1;
using System.Text;


public class Node // 合并后的节点？
{
    public GridTile pos; // 节点位置：GirdTile有x和y
    public List<Edge> edges; // 边列表
    public Node child; // 子node


    public Node(GridTile value)
    {
        this.pos = value;
        edges = new List<Edge>();
    }

    public String getPos()
    {
        return ("pos:" + this.pos.x + "," + this.pos.y);
    }

    public String getEdges()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("edges:\n");
        foreach (Edge e in edges)
        {
            sb.Append("Start:" + e.start.getPos() + ",");
            sb.Append("End:" + e.end.getPos() + ",");
            sb.Append("Type:" + e.type + ",");
            sb.Append("Weight:" + e.weight+"\n");
        }
        return sb.ToString();
    }

    public String getChild()
    {
        //return "child pos:"+this.child.getPos();
        StringBuilder sb = new StringBuilder();
        sb.Append("child:" + this.child.getPos() + "\n");
        sb.Append("child edges:\n");
        foreach (Edge e in this.child.edges)
        {
            sb.Append("Start:" + e.start.getPos() + ",");
            sb.Append("End:" + e.end.getPos() + ",");
            sb.Append("Type:" + e.type + ",");
            sb.Append("Weight:" + e.weight + "\n");
        }
        return sb.ToString();
    }
}


