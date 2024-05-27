
// Custom class representing a product with predefined position
using System;

[Serializable]
public class Product
{
    public int row;
    public int column;
    public int level;
    public double turnoverRate;
    public float weight;

    public Product(int row, int column, int level, double turnoverRate, float weight)
    {
        this.row = row;
        this.column = column;
        this.level = level;
        this.turnoverRate = turnoverRate;
        this.weight = weight;
    }
}
