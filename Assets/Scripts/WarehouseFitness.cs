using System.Collections.Generic;
using UnityEngine;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Fitnesses;
using System;
// Custom fitness function for the warehouse problem
public class WarehouseFitness : IFitness
{
    private List<Product> products;
    private int maxRows;
    private int maxColumns;
    private int maxLevels;
    private static double minFitness = double.MaxValue, maxFitness = double.MinValue;
    private const double Epsilon = 1e-10;

    public WarehouseFitness(List<Product> products, int maxRows, int maxColumns, int maxLevels)
    {
        this.products = products;
        this.maxRows = maxRows;
        this.maxColumns = maxColumns;
        this.maxLevels = maxLevels;

    }

    public double Evaluate(IChromosome chromosome)
    {
        var warehouseChromosome = chromosome as WarehouseChromosome;

        if (warehouseChromosome == null)
        {
            throw new ArgumentException("The chromosome is not of type WarehouseChromosome.");
        }

        // Calculate fitness based on three objectives:
        // 1. Accessibility of popular products
        // 2. Clustering of similar products
        // 3. Shelf stability (heavier products on lower shelves)

        double accessibilityFitness = 0.6 * EvaluateAccessibility(warehouseChromosome);
        double clusteringFitness = 0.2 * EvaluateClustering(warehouseChromosome);
        double stabilityFitness = 0.2 * EvaluateStability(warehouseChromosome);

        // Combine the three fitness components into a single fitness score
        // You might want to adjust the weights to prioritize certain objectives
        double totalFitness = accessibilityFitness + clusteringFitness + stabilityFitness;
        // Track the minimum and maximum fitness values for normalization
        minFitness = Math.Min(minFitness, totalFitness);
        maxFitness = Math.Max(maxFitness, totalFitness);

        // Ensure that maxFitness and minFitness are not equal to avoid division by zero
        double range = maxFitness - minFitness;
        if (range < Epsilon)
        {
            range = Epsilon;
        }

        // Normalize totalFitness within the range [minFitness, maxFitness]
        double normalizedFitness = (totalFitness - minFitness) / range;

        // Apply sigmoid function to normalized fitness value
        double beta = 1.0; // Adjust beta to control the steepness of the sigmoid curve
        double sigmoidFitness = Sigmoid(normalizedFitness, beta);

        return totalFitness;
    }
    private double Sigmoid(double x, double beta)
    {
        return 1.0 / (1.0 + Math.Exp(-beta * (x - 0.5) * 2)); // Center the sigmoid around 0.5 for better scaling
    }
    private double EvaluateAccessibility(WarehouseChromosome chromosome)
    {
        double accessibilityFitness = 0.0;
        var Entrance = new Vector3(0, 0, 0);
        foreach (var product in chromosome.products)
        {
            var productLoc = new Vector3(product.row, product.column, product.level / 0.65f);
            // Calculate distance from entrance
            var dist = Vector3.Distance(Entrance, productLoc);
            //double distance = Math.Sqrt(Math.Pow(product.row, 2) + Math.Pow(product.column, 2) + Math.Pow(product.level / 0.65, 2));

            // Ensure distance is non-zero to avoid division by zero
            if (dist != 0)
            {
                //accessibilityFitness += product.turnoverRate / dist;
                accessibilityFitness += product.turnoverRate * dist;
            }
            else
            {
                // Handle case when distance is zero (optional)
                accessibilityFitness += product.turnoverRate; // For example, just add the turnover rate
            }
        }
        //foreach (var product in chromosome.products)
        //{
        //    double p_v = (product.row / 1) + (product.column / 1) + (product.level / 0.65);
        //    //Calculate distance from entrance

        //   //double distance = Math.Sqrt(Math.Pow(product.row, 2) + Math.Pow(product.column, 2) + Math.Pow(product.level, 2));
        //    accessibilityFitness += p_v * product.turnoverRate;

        //}
        //accessibilityFitness = 1 / (accessibilityFitness + 1);
        return -accessibilityFitness;
    }

    private double EvaluateClustering(WarehouseChromosome chromosome)
    {
        //double clusteringFitness = 0.0;

        //foreach (var product in chromosome.products)
        //{
        //    double minDistance = double.MaxValue;

        //    foreach (var otherProduct in chromosome.products)
        //    {
        //        double distance = Math.Sqrt(Math.Pow(product.row - otherProduct.row, 2) +
        //                                    Math.Pow(product.column - otherProduct.column, 2) +
        //                                    Math.Pow(product.level - otherProduct.level, 2));

        //        // Ensure distance is non-zero to avoid division by zero
        //        if (distance != 0 && distance < minDistance)
        //        {
        //            minDistance = distance;
        //        }
        //    }

        //    clusteringFitness += minDistance;
        //}
        //clusteringFitness = 1 / (clusteringFitness + 1);
        //return clusteringFitness;
        double totalScore = 0.0;
        for (int i = 0; i < chromosome.products.Count; i++)
        {
            Product product = chromosome.GetProduct(i);
            var productCategory = product.weight;

            // Iterate through neighbors of the product (replace with your neighbor finding logic)
            foreach (Product neighbor in GetNeighbors(chromosome, i))
            {
                totalScore += (productCategory == neighbor.weight) ? 1.0 : 0.0; // Similarity score (1 for same category)
            }
        }
        //totalScore = 1 / (totalScore + 1);
        return totalScore;

    }
    private List<Product> GetNeighbors(WarehouseChromosome chromosome, int productIndex)
    {
        List<Product> neighbors = new List<Product>();
        Product product = chromosome.GetProduct(productIndex);
        int productRow = product.row;
        int productColumn = product.column;
        int productLevel = product.level;
        double distanceThreshold = 2; // Replace with your desired neighbor distance

        // Iterate through all other products
        for (int i = 0; i < chromosome.products.Count; i++)
        {
            if (i == productIndex) continue; // Skip the product itself

            Product neighbor = chromosome.GetProduct(i);
            int neighborRow = neighbor.row;
            int neighborColumn = neighbor.column;
            int neighborLevel = neighbor.level;

            // Calculate Manhattan distance (replace with your preferred distance metric)
            double distance = Math.Abs(productRow - neighborRow) +
                               Math.Abs(productColumn - neighborColumn) +
                               Math.Abs(productLevel - neighborLevel);

            if (distance <= distanceThreshold)
            {
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }
    private double EvaluateStability(WarehouseChromosome chromosome)
    {
        //double stabilityFitness = 0.0;

        //foreach (var product in chromosome.products)
        //{
        //    stabilityFitness += product.weight * product.level;
        //}
        //stabilityFitness = 1 / (stabilityFitness + 1);
        //return stabilityFitness;

        double totalScore = 0.0;
        for (int i = 0; i < chromosome.products.Count; i++)
        {
            Product product = chromosome.GetProduct(i);
            float productWeight = product.weight;
            int productLevel = product.level;
            int maxLevels = 2; // Assuming access to warehouse dimensions

            // Penalize based on weight and level (higher level, higher penalty)
            totalScore -= productWeight * (maxLevels - productLevel);

            // Additional stability checks (weight distribution on shelf, compatibility with neighboring items)
            // can be implemented here based on your specific warehouse setup
        }
        //totalScore = 1 / (totalScore + 1);
        return totalScore;
    }

}
