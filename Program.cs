using ART_1;
using System.Text.RegularExpressions;

const string helpMessage = 
    "Provide 4 positional arguments:\n" +
    "1. Vigilance parameter, 0 < p <= 1.\n" +
    "2. Path to learning input file - each row is one input in format 0 1 1 0 1, lenghts must be the same.\n" +
    "3. Path to testing input file - each row is one input in format 0 1 1 0 1, lenghts must be the same.\n" +
    "4. Path to clustering output.\n";

/// <summary>
/// Parses input file and returns parsed inputs or throws an exception if file does not exists or format is invalid
/// </summary>
static List<List<int>> ReadInputFile(string path, out int numberOfInputs, out int numberOfAttributes)
{
    List<List<int>> inputs = 
        File.ReadAllLines(path)                             // Reads all lines
        .Select(str => str.Trim())             // Trims the lines
        .Select(str =>
            Regex.Split(str, @"\s+")            // Splits each line by whitespace
            .Where(s => s != string.Empty)    // Removes empty entries after splitting
            .Select(s => Int32.Parse(s))       // Parses each entry in the line to an int
            .ToList()                                       // Returns list (of ints) for each line in the file
        )
        .ToList();

    numberOfInputs = inputs.Count;

    // Just assume there is at leas one input, otherwise it will just throw an exception as written in summary
    numberOfAttributes = inputs[0].Count;

    // Check all the inputs
    foreach(var input in inputs)
    {
        if (input.Count != numberOfAttributes)
            throw new Exception("Numbers of attributes don't match.");
        
        foreach(var attribute in input)
        {
            if (attribute != 1 && attribute != 0)
                throw new Exception("Attributes must be either 0 or 1.");
        }
    }

    return inputs;
}

// Check if number of parameters is valid
if(args.Length != 4)
{
    Console.WriteLine(helpMessage);
    Console.WriteLine($"{args.Length} arguments were found.");
    return;
}

// Reading vigilance parameter
bool vigilanceParamertConverted = double.TryParse(args[0], out double vigilanceParametr);

if(!vigilanceParamertConverted || vigilanceParametr <= 0 || 1 < vigilanceParametr)
{
    Console.WriteLine(helpMessage);
    Console.WriteLine("Vigilance parameter could not be read or is not in (0, 1].");
    return;
}

// Reading learning input
List<List<int>> learningInputs;
int learningNumberOfInputs;
int learningnumberOfAttributes;

try
{
    learningInputs = ReadInputFile(args[1], out learningNumberOfInputs, out learningnumberOfAttributes);
}
catch(Exception ex)
{
    Console.WriteLine(helpMessage);
    Console.WriteLine($"Learning:\n{ex.Message}");
    return;
}

// Reading testing input
List<List<int>> testingInputs;
int testingNumberOfInputs;
int testingnumberOfAttributes;

try
{
    testingInputs = ReadInputFile(args[2], out testingNumberOfInputs, out testingnumberOfAttributes);
}
catch (Exception ex)
{
    Console.WriteLine(helpMessage);
    Console.WriteLine($"Testing:\n{ex.Message}");
    return;
}

if(learningnumberOfAttributes != testingnumberOfAttributes)
{
    Console.WriteLine(helpMessage);
    Console.WriteLine($"Number of attributes in learning and testing sets does not match.");
    return;
}

// Checking if output can be saved
FileStream outputFile;
try
{
    outputFile = File.OpenWrite(args[3]);
}
catch (Exception ex)
{
    Console.WriteLine(helpMessage);
    Console.WriteLine($"Could not open output file:\n{ex.Message}");
    return;
}

// Training with prediction
Console.WriteLine($"Vigilance parameter: {vigilanceParametr}.");
Console.WriteLine($"Found learning set. Size: {learningNumberOfInputs}. Number of attributes: {learningnumberOfAttributes}.");
Console.WriteLine($"Found testing set. Size: {testingNumberOfInputs}. Number of attributes: {testingnumberOfAttributes}.");
Console.WriteLine($"Output file successfully opened.");

Console.WriteLine("Training started...");

ART1Network.Train(learningInputs, vigilanceParametr, out List<List<double>> b, out List<List<double>> t);

Console.WriteLine($"Training finished. Found clusters: {b.Count}.");

Console.WriteLine("Predicting started...");

List<int> resultClusters = ART1Network.Predict(testingInputs, b);

Console.WriteLine($"Predicting finished.");

Console.WriteLine("Writing output...");

using(var sreamWriter = new StreamWriter(outputFile))
{
    for (int i = 0; i < testingNumberOfInputs; i++)
    {
        string outputLine = "";
        foreach (var n in testingInputs[i])
            outputLine += $"{n} ";
        outputLine += $"- {resultClusters[i]}";
        sreamWriter.WriteLine(outputLine);
    }
}

outputFile.Close();

Console.WriteLine("Finished!");