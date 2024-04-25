using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Boundaries
{
    //Top left corner (minimum corner)，最小值，左上角
    public GridTile Min { get; set; }

    //Bottom right corner (maximum corner)，最大值，右下角
    public GridTile Max { get; set; }

    public Boundaries(GridTile min, GridTile max)
    {
        Min = min;
        Max = max;
    }

    public Boundaries() { }
    public Boundaries(int min_x,int min_y,int max_x,int max_y)
    {
        Min = new GridTile(min_x, min_y);
        Max = new GridTile(max_x, max_y);
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        Boundaries other = obj as Boundaries;
        return Min.x == other.Min.x && Min.y == other.Min.y && Max.x == other.Max.x && Max.y == other.Max.y;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 29 + Min.x.GetHashCode();
            hash = hash * 29 + Min.y.GetHashCode();
            hash = hash * 29 + Max.x.GetHashCode();
            hash = hash * 29 + Max.y.GetHashCode();
            return hash;
        }
    }

    public static bool operator !=(Boundaries o1, Boundaries o2)
    {
        if (ReferenceEquals(o1, null)) return !ReferenceEquals(o2, null);
        else return !o1.Equals(o2);
    }

    public static bool operator ==(Boundaries o1, Boundaries o2)
    {
        if (ReferenceEquals(o1, null)) return ReferenceEquals(o2, null);
        else return o1.Equals(o2);
    }
}
