var errors = new List<string>();
var numbers = new List<int>();

for (int i = 0; i < args.Length; i++)
{
    if (int.TryParse(args[i], out int number))
        numbers.Add(number);
    else
        errors.Add($"Failed to parse {i} argument as number.");
}

if (errors.Count > 0)
    throw new ArgumentException(string.Join(Environment.NewLine, errors));

Console.WriteLine(numbers.Sum());
