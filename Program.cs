namespace FileCopyHS;

internal class Program
{
    private static void Main()
    {
        Console.Write("Enter source file path: ");

        var sourcePath = Console.ReadLine();
        if (string.IsNullOrEmpty(sourcePath) || !Directory.Exists(sourcePath) || !File.Exists(sourcePath))
        {
            Console.WriteLine("Invalid source path.");
            return;
        }

        Console.Write("Enter destination path: ");

        var destinationPath = Console.ReadLine();
        if (string.IsNullOrEmpty(destinationPath) || !Directory.Exists(destinationPath))
        {
            Console.WriteLine("Invalid destination path or directory does not exist in the destination path.");
            return;
        }
        
        try
        {
            var fileName = Path.GetFileName(sourcePath);
            // TODO: validate if filename is valid

            destinationPath = Path.Combine(destinationPath, fileName);

            // TODO: split file in chunks initial logic

            Console.WriteLine("Copying...");
            File.Copy(sourcePath, destinationPath);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}