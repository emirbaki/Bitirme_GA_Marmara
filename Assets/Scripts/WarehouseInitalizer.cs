using System.Collections.Generic;
using UnityEngine;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Randomizations;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Domain.Terminations;
using GeneticSharp.Domain;
using GeneticSharp.Infrastructure.Framework.Threading;
using Unity.VisualScripting.FullSerializer;
using System;
using ClosedXML.Excel;

public class WarehouseInitializer : MonoBehaviour
{
    public int minPopulationSize = 3420; // Number of chromosomes in the population
    public int maxPopulationSize = 3420; // Number of chromosomes in the population
    public int maxRows = 12; // Maximum number of rows in the warehouse
    public int maxColumns = 63; // Maximum number of columns in the warehouse
    public int maxLevels = 5; // Maximum number of levels in the warehouse
    public int productCount = 3420; // Total number of products
    public List<Product> products; // List of products with predefined positions
    public Transform otekiObje;
    private GeneticAlgorithm m_ga;
    private double minFitness, maxFitness;
    void Start()
    {
        // Initialize GeneticSharp's random number generator
        GenerateProducts(true);
        //RandomizationProvider.Current = new UnityRandomizationProvider();
        var visualizer = GetComponent<WarehouseVisualizer>();
        visualizer.products = products;
        visualizer.Visualize(otekiObje);
        // Create a new population with the specified chromosome class and size
        var population = new Population(minPopulationSize, maxPopulationSize, new WarehouseChromosome(maxRows, maxColumns, maxLevels, products, productCount));
        population.GenerationStrategy = new PerformanceGenerationStrategy(); // Improve generation performance

        // Define crossover, mutation, and selection operators
        var crossover = new WarehouseCrossover();

        var mutation = new AdaptiveMutation(0.05f, 0.005f, 1000);

        var selection = new RouletteWheelSelection();

        // Create the fitness function for the warehouse problem
        var fitness = new WarehouseFitness(products, maxRows, maxColumns, maxLevels);

        //var termination2 = new GenerationNumberTermination(1000);
        var termination2 = new FitnessStagnationTermination(50);
        // Create the Genetic Algorithm instance
        m_ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation)
        {
            Termination = termination2,

            TaskExecutor = new ParallelTaskExecutor { MinThreads = 100, MaxThreads = 200 }
        };

        // Subscribe to the GenerationRan event to log the best solution after each generation
        m_ga.GenerationRan += delegate
        {

            //Debug.Log($"Generation: {m_ga.GenerationsNumber} - Fitness: {m_ga.BestChromosome.Fitness} - Time : {m_ga.TimeEvolving}");
   
        };
        var fitnessLogger = new WarehouseFitnessLogger();
        m_ga.GenerationRan += (sender, e) => fitnessLogger.LogGenerationFitness(m_ga);
        // Start the Genetic Algorithm
        m_ga.Start();

        var bestChromosome = m_ga.BestChromosome as WarehouseChromosome;
        visualizer.products = bestChromosome.products;
        visualizer.Visualize(transform);
        fitnessLogger.SaveToExcel("FitnessLog.xlsx");
    }
    private void GenerateProducts(bool initializeInOrder)
    {
        products = new List<Product>();
        double turnA = 0.215;
        double turnB = 0.023;
        double turnC = 0.002;
        double weightA = 2.433;
        double weightB = 3.997;
        double weightC = 5.555;
        // Generate products randomly for each category based on the specified constraints
        for (int i = 0; i < productCount; i++)
        {
            var rand = new UnityRandomizationProvider();
            // Randomly select a category (A, B, or C)
            char category = rand.GetBool(0.33f) ? 'A' : rand.GetBool(0.5f) ? 'B' : 'C';
       
            // Generate product attributes based on the selected category
            double turnover, weight, amount;
            switch (category)
            {
                case 'A':
                    //turnover = RandomizationProvider.Current.GetDouble(6.91 - 1, 6.91 + 1); // Generate turnover within a range around the mean
                    turnover = turnA; // Generate turnover within a range around the mean
                    //weight = RandomizationProvider.Current.GetDouble(21.25 - 5, 21.25 + 5); // Generate weight within a range around the mean
                    weight = weightA;
                    amount = RandomizationProvider.Current.GetDouble(12 - 3, 12 + 3); // Generate amount within a range around the mean
                    break;
                case 'B':
                    //turnover = RandomizationProvider.Current.GetDouble(4.41 - 1, 4.41 + 1); // Generate turnover within a range around the mean
                    turnover = turnB; // Generate turnover within a range around the mean
                    //weight = RandomizationProvider.Current.GetDouble(31.5 - 5, 31.5 + 5); // Generate weight within a range around the mean
                    weight = weightB; // Generate weight within a range around the mean
                    amount = RandomizationProvider.Current.GetDouble(10 - 3, 10 + 3); // Generate amount within a range around the mean
                    break;
                case 'C':
                    //turnover = RandomizationProvider.Current.GetDouble(2.37 - 1, 2.37 + 1); // Generate turnover within a range around the mean
                    turnover = turnC; // Generate turnover within a range around the mean
                    //weight = RandomizationProvider.Current.GetDouble(12.5 - 5, 12.5 + 5); // Generate weight within a range around the mean
                    weight = weightC; // Generate weight within a range around the mean
                    amount = RandomizationProvider.Current.GetDouble(7 - 3, 7 + 3); // Generate amount within a range around the mean
                    break;
                default:
                    throw new System.Exception("Invalid category.");
            }
            // Initialize row, column, and level values
            int row = 0, column = 0, level = 0;

            // If initializing in order, calculate row, column, and level based on product index
            if (initializeInOrder)
            {
                row = i / (maxColumns * maxLevels);
                column = (i / maxLevels) % maxColumns;
                level = i % maxLevels;
            }

            // Add the generated product to the list
            products.Add(new Product(row, column, level, turnover, (float)weight));
        }
    }
    private void SetPredefinedPositions(WarehouseChromosome chromosome)
    {
        // Set predefined positions for products
        for (int i = 0; i < productCount; i++)
        {
            chromosome.ReplaceGene(i * 3, new Gene(products[i].row)); // x (row)
            chromosome.ReplaceGene(i * 3 + 1, new Gene(products[i].column)); // y (column)
            chromosome.ReplaceGene(i * 3 + 2, new Gene(products[i].level)); // z (level)
        }
    }
    private IChromosome GenerateChromosomeWithPredefinedPositions()
    {
        var chromosome = new WarehouseChromosome(maxRows, maxColumns, maxLevels, products, 3420);

        // Set predefined positions for products
        for (int i = 0; i < productCount; i++)
        {
            chromosome.ReplaceGene(i * 3, new Gene(products[i].row)); // x (row)
            chromosome.ReplaceGene(i * 3 + 1, new Gene(products[i].column)); // y (column)
            chromosome.ReplaceGene(i * 3 + 2, new Gene(products[i].level)); // z (level)
        }

        return chromosome;
    }
}
public class WarehouseFitnessLogger
{
    private List<double> fitnessValues = new List<double>();
    private List<TimeSpan> timeEvolvings = new ();

    public void LogGenerationFitness(GeneticAlgorithm ga)
    {
        double maxFitness = ga.BestChromosome.Fitness.Value;
        fitnessValues.Add(maxFitness);
        timeEvolvings.Add(ga.TimeEvolving);
        //Console.WriteLine($"Generation {ga.GenerationsNumber}: {maxFitness}");
    }

    public void SaveToExcel(string filePath)
    {
        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Fitness Log");
            worksheet.Cell(1, 1).Value = "Generation";
            worksheet.Cell(1, 2).Value = "Max Fitness";
            worksheet.Cell(1, 3).Value = "Evolving Time";

            for (int i = 0; i < fitnessValues.Count; i++)
            {
                worksheet.Cell(i + 2, 1).Value = i + 1;
                worksheet.Cell(i + 2, 2).Value = fitnessValues[i];
                worksheet.Cell(i + 2, 3).Value = timeEvolvings[i];
            }

            workbook.SaveAs(filePath);
        }
    }
}
