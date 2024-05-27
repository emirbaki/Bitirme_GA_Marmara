using GeneticSharp.Domain.Randomizations;

public class UnityRandomizationProvider : IRandomization
{
    public int GetInt(int min, int max)
    {
        return UnityEngine.Random.Range(min, max);
    }

    public float GetFloat(float min, float max)
    {
        return UnityEngine.Random.Range(min, max);
    }

    public bool GetBoolean()
    {
        return UnityEngine.Random.value > 0.5f;
    }
    public bool GetBool(float val)
    {
        return UnityEngine.Random.value > val;
    }

    public double GetDouble()
    {
        return UnityEngine.Random.value;
    }

    public int[] GetInts(int length, int min, int max)
    {
        throw new System.NotImplementedException();
    }

    public int[] GetUniqueInts(int length, int min, int max)
    {
        throw new System.NotImplementedException();
    }

    public float GetFloat()
    {
        throw new System.NotImplementedException();
    }

    public double GetDouble(double min, double max)
    {
        throw new System.NotImplementedException();
    }
}