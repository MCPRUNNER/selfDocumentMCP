using System.Text.Json;

var testObject = new { jsonrpc = "2.0", id = 1, result = new { test = "value" } };

var compactJson = JsonSerializer.Serialize(testObject, new JsonSerializerOptions
{
    WriteIndented = false
});

var indentedJson = JsonSerializer.Serialize(testObject, new JsonSerializerOptions
{
    WriteIndented = true
});

Console.WriteLine("Compact:");
Console.WriteLine(compactJson);
Console.WriteLine();
Console.WriteLine("Indented:");
Console.WriteLine(indentedJson);
