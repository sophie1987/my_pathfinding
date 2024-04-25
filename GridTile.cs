using System.Collections;
using System.Collections.Generic;

using System;
using System.Diagnostics;
using System.Numerics;

[Serializable()]
[DebuggerDisplay("({x}, {y})")]
public class GridTile
{
    public int x;
    public int y;
    public int weight;

    //Empty constructor. Nothing to do really
    public GridTile() { }

    //Constructor with both x and y values given.
    public GridTile(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public GridTile(int x, int y, int weight) : this(x, y)
    {
        this.weight = weight;
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        GridTile other = obj as GridTile;
        return x == other.x && y == other.y;
    }

    // override object.GetHashCode
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 29 + x.GetHashCode();
            hash = hash * 29 + y.GetHashCode();
            return hash;
        }
    }

    public static bool operator !=(GridTile o1, GridTile o2)
    {
        if (ReferenceEquals(o1, null)) return !ReferenceEquals(o2, null);
        else return !o1.Equals(o2);
    }

    public static bool operator ==(GridTile o1, GridTile o2)
    {
        if (ReferenceEquals(o1, null)) return ReferenceEquals(o2, null);
        else return o1.Equals(o2);
    }

}
