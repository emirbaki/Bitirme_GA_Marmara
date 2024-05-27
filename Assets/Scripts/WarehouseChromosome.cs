using System.Collections.Generic;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Randomizations;
using System;
using System.Diagnostics;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Mutations;
using System.Linq;
// Custom chromosome representing a storage allocation plan
public class WarehouseChromosome : ChromosomeBase
{
    private int maxRows;
    private int maxColumns;
    private int maxLevels;
    private int productCount;
    public List<Product> products = new List<Product>(); // List of products with assigned slots

    public WarehouseChromosome(int maxRows, int maxColumns, int maxLevels, List<Product> products, int productCount) : base(productCount * 3)
    {
        this.maxRows = maxRows;
        this.maxColumns = maxColumns;
        this.maxLevels = maxLevels;
        this.productCount = productCount;
        if (products != null)
        {
            this.products = new List<Product>(products); // Copy the product list
        }
        else
        {
            this.products = new List<Product>(this.productCount);
        }

        // Randomly assign slot locations (row, column, level) to each product without repetition
        AssignUniqueSlotLocations();
    }

    private void AssignUniqueSlotLocations()
    {
        // Use a HashSet to track occupied slots (combination of row, column, and level)
        HashSet<int> occupiedSlots = new HashSet<int>();

        for (int i = 0; i < products.Count; i++)
        {
            int row, column, level;

            do
            {
                row = RandomizationProvider.Current.GetInt(0, maxRows);
                column = RandomizationProvider.Current.GetInt(0, maxColumns);
                level = RandomizationProvider.Current.GetInt(0, maxLevels);
            } while (occupiedSlots.Contains(GetSlotIndex(row, column, level)));

            products[i].row = row;
            products[i].column = column;
            products[i].level = level;

            occupiedSlots.Add(GetSlotIndex(row, column, level));
        }

        CreateGenes();
    }

    public override Gene GenerateGene(int geneIndex)
    {
       
        // Ensure gene index is within valid range
        int productIndex = (geneIndex) / 3;
        if (geneIndex == 0)
        {
            productIndex = 0;
        }
        //UnityEngine.Debug.Log(productIndex);
        // Check if productIndex is within the valid range
        if (productIndex < 0 || productIndex >= products.Count)
        {
            
            UnityEngine.Debug.Log(products.Count + " product list boş");
            UnityEngine.Debug.Log(productIndex);
            UnityEngine.Debug.Log(geneIndex);

            throw new ArgumentOutOfRangeException(nameof(geneIndex), "Gene index is out of range.");
        }
        // Get the product from the product list
        var product = products[productIndex];

        // Generate genes based on the gene type (row, column, level) for the product
        switch (geneIndex % 3)
        {
            case 0: // x (row)
                return new Gene(product.row);
            case 1: // y (column)
                return new Gene(product.column);
            case 2: // z (level)
                return new Gene(product.level);
            default:
                throw new InvalidOperationException("Invalid gene index.");
        }
    }

    public override IChromosome CreateNew()
    {
        return new WarehouseChromosome(maxRows, maxColumns, maxLevels, products, productCount); // Don't need product list here
    }

    // Access product information by index
    public Product GetProduct(int productIndex)
    {
        return products[productIndex];
    }

    // Update the slot location (row, column, level) for a product at a specific index
    public void UpdateSlotLocation(int productIndex, int geneIndex)
    {
        var genes = GetGenes();
        switch (geneIndex % 3)
        {
            case 0: // Update row
                products[productIndex].row = (int)genes[geneIndex].Value;
                break;
            case 1: // Update column
                products[productIndex].column = (int)genes[geneIndex].Value;
                break;
            case 2: // Update level
                products[productIndex].level = (int)genes[geneIndex].Value;
                break;
        }
    }

    // Utility method to calculate the slot index from row, column, and level
    private int GetSlotIndex(int row, int column, int level)
    {
        return level * (maxRows * maxColumns) + row * maxColumns + column;
    }
}

public class WarehouseCrossover : ICrossover
{
    public int ParentsNumber => 2;

    public int ChildrenNumber => 2;

    public int MinChromosomeLength => 150 * 3; // Adjust this value based on your actual chromosome length

    public bool IsOrdered => true;

    public IList<IChromosome> Cross(IList<IChromosome> parents)
    {
        var parent1 = parents[0];
        var parent2 = parents[1];

        var offspring1 = CreateOffspring(parent1, parent2);
        var offspring2 = CreateOffspring(parent2, parent1);

        return new List<IChromosome> { offspring1, offspring2 };
    }

    private IChromosome CreateOffspring(IChromosome parent1, IChromosome parent2)
    {
        var offspring = parent1.CreateNew();

        // Select crossover points
        var crossoverPoint1 = RandomizationProvider.Current.GetInt(0, parent1.Length);
        var crossoverPoint2 = RandomizationProvider.Current.GetInt(0, parent1.Length);

        if (crossoverPoint1 > crossoverPoint2)
        {
            var temp = crossoverPoint1;
            crossoverPoint1 = crossoverPoint2;
            crossoverPoint2 = temp;
        }

        // Copy genetic material between crossover points
        var genesFromParent1 = parent1.GetGenes().ToList();
        var genesFromParent2 = parent2.GetGenes().ToList();

        var usedGenes = new HashSet<Gene>();
        for (int i = crossoverPoint1; i <= crossoverPoint2; i++)
        {
            var geneFromParent2 = genesFromParent2[i];
            offspring.ReplaceGene(i, geneFromParent2);
            usedGenes.Add(geneFromParent2);
        }

        // Fill remaining slots while ensuring uniqueness
        for (int i = 0; i < offspring.Length; i++)
        {
            if (i < crossoverPoint1 || i > crossoverPoint2)
            {
                foreach (var gene in genesFromParent1)
                {
                    if (!usedGenes.Contains(gene))
                    {
                        offspring.ReplaceGene(i, gene);
                        usedGenes.Add(gene);
                        break;
                    }
                }
            }
        }

        return offspring;
    }
}

public class WarehouseMutation : IMutation
{
    public bool IsOrdered => true;

    public void Mutate(IChromosome chromosome, float probability)
    {
        if (probability <= 0)
        {
            return;
        }

        for (int i = 0; i < chromosome.Length; i++)
        {
            if (RandomizationProvider.Current.GetDouble() <= probability)
            {
                // Select two distinct genes
                int index1 = RandomizationProvider.Current.GetInt(0, chromosome.Length);
                int index2 = RandomizationProvider.Current.GetInt(0, chromosome.Length);
                while (index2 == index1)
                {
                    index2 = RandomizationProvider.Current.GetInt(0, chromosome.Length);
                }

                // Swap their positions
                var temp = chromosome.GetGene(index1);
                chromosome.ReplaceGene(index1, chromosome.GetGene(index2));
                chromosome.ReplaceGene(index2, temp);
            }
        }
    }
}

public class AdaptiveMutation : IMutation
{
    private float initialRate;
    private float finalRate;
    private int maxGenerations;
    public bool IsOrdered => true;
    private static int generationNumber = 1;
    public AdaptiveMutation(float initialRate, float finalRate, int maxGenerations)
    {
        this.initialRate = initialRate;
        this.finalRate = finalRate;
        this.maxGenerations = maxGenerations;
    }

    public void Mutate(IChromosome chromosome, float probability)
    {
        var currentGeneration = generationNumber++;
        var rate = initialRate + (finalRate - initialRate) * (currentGeneration / (float)maxGenerations);

        if (rate <= 0)
        {
            return;
        }

        for (int i = 0; i < chromosome.Length; i++)
        {
            if (RandomizationProvider.Current.GetDouble() <= rate)
            {
                int index1 = RandomizationProvider.Current.GetInt(0, chromosome.Length);
                int index2 = RandomizationProvider.Current.GetInt(0, chromosome.Length);
                while (index2 == index1)
                {
                    index2 = RandomizationProvider.Current.GetInt(0, chromosome.Length);
                }

                var temp = chromosome.GetGene(index1);
                chromosome.ReplaceGene(index1, chromosome.GetGene(index2));
                chromosome.ReplaceGene(index2, temp);
            }
        }
    }
}