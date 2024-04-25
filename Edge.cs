
using System.Collections.Generic;
using System.Text;

public class Edge
{
    public Node start; // 起点
    public Node end; // 终点
    public EdgeType type; // 边类型：内部，交叉
    public float weight; // 权值

    public LinkedList<Edge> UnderlyingPath; //path

    public string getPresent()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("Start:" + this.start.getPos() + ",");
        sb.Append("End:" + this.end.getPos() + ",");
        sb.Append("Type:" + this.type + ",");
        sb.Append("Weight:" + this.weight + "\n");
        return sb.ToString();
    }
}


public enum EdgeType
{
    INTRA,
    INTER
}
