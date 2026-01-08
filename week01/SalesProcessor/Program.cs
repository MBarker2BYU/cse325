using System.Globalization;
using System.Text;

string salesDirectory = GetSalesDirectoryPath();

Dictionary<string, decimal> salesByFile = new();
decimal totalSales = 0m;

var salesFiles = Directory.GetFiles(salesDirectory, "*.txt");

foreach (var file in salesFiles)
{
    string content = File.ReadAllText(file).Trim();
    if (decimal.TryParse(content, NumberStyles.Currency, CultureInfo.InvariantCulture, out decimal amount))
    {
        salesByFile[file] = amount;
        totalSales += amount;
    }
    else
    {
        Console.WriteLine($"Could not parse sales amount in {Path.GetFileName(file)}");
    }
}

Console.WriteLine($"Total Sales: {totalSales:C}");

// Your additional function
GenerateSalesSummary(salesDirectory, totalSales, salesByFile);

static void GenerateSalesSummary(string directoryPath, decimal totalSales, Dictionary<string, decimal> salesByFile)
{
    StringBuilder report = new();
    report.AppendLine("Sales Summary");
    report.AppendLine("----------------------------");
    report.AppendLine($"Total Sales: {totalSales:C}");
    report.AppendLine();
    report.AppendLine("Details:");

    foreach (var kvp in salesByFile.OrderBy(k => Path.GetFileName(k.Key)))
    {
        string filename = Path.GetFileName(kvp.Key);
        report.AppendLine($" {filename}: {kvp.Value:C}");
    }

    string reportPath = Path.Combine(directoryPath, "sales_summary.txt");
    File.WriteAllText(reportPath, report.ToString());
    Console.WriteLine($"Sales summary report generated at: {reportPath}");
}

static string GetSalesDirectoryPath()
{
    const string PathName = "Sales";

    // Start here, exactly as you want
    var currentDirectory = Directory.GetCurrentDirectory();

    string? salesDirectory = null;

    // Search downward: enumerate all subdirectories (including nested ones) for "Sales"
    var allSubdirectories = Directory.GetDirectories(
        currentDirectory,
        "Sales",
        SearchOption.AllDirectories);  // This searches the entire tree below

    if (allSubdirectories.Length > 0)
    {
        // Use the first "Sales" folder found (you can change to .Last() if you prefer deepest)
        salesDirectory = allSubdirectories[0];
        Console.WriteLine($"Found Sales directory at: {salesDirectory}");
    }
    else
    {
        // No Sales folder found anywhere below → create one right here
        salesDirectory = Path.Combine(currentDirectory, PathName);
        Directory.CreateDirectory(salesDirectory);
        Console.WriteLine($"No Sales folder found in subdirectories. Created new one at: {salesDirectory}");
    }

    // Now use salesDirectory for processing files
    Console.WriteLine($"Using Sales directory: {salesDirectory}");

    return salesDirectory;
}