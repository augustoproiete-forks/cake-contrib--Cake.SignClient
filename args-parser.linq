<Query Kind="Statements" />

string file = @"D:\GitProjects\Righthand\Cake\Cake.SignClient\src\Cake.SignClient\Zip\sign-arguments.txt";
string[] lines = File.ReadAllLines(file);

Regex regex = new Regex(
	  "--(?<Argument>[a-z0-9,\\-]+)(?:\\s(?<Type>\\w+))?\\s+(?<Info>.+" +
	  ")",
	RegexOptions.IgnoreCase
	| RegexOptions.Multiline
	| RegexOptions.CultureInvariant
	| RegexOptions.Compiled
	);

Dictionary<string, List<string>> data = new Dictionary<string, List<string>>();
List<string> current = null;
foreach (string line in lines.Skip(2).Where(l =>!string.IsNullOrEmpty(l)).Select(l => l.TrimStart()))
{
	if (line.StartsWith("-"))
	{
		current = new List<string>();
		data.Add(line, current);
	}
	else
	{
		current.Add(line.Trim());
	}
}
foreach (var pair in data)
{
	string line = pair.Key;
	
	var match = regex.Match(line);
	if (!match.Success)
	{
		("FAILED to match " + line).Dump();
	}
	else
	{
		string rawName = match.Groups["Argument"].Value;
		string type = match.Groups["Type"].Value;
		string info = match.Groups["Info"].Value;
		string name = "";
		bool upperCase = true;
		foreach (char c in rawName)
		{
			if (upperCase)
			{
				name += char.ToUpper(c);
				upperCase = false;
			}
			else
			{
				if (c == '-')
				{
					upperCase = true;
				}
				else
				{
					name += c;
				}
			}
		}
		List<string> comment = new List<string>();
		comment.Add(info.Replace("<arg>", "").Trim());
		comment.AddRange(pair.Value.Select(l => $"\t{l}"));
		"/// <summary>".Dump();
		foreach (string commentLine in comment)
		{
			("/// " + commentLine
				.Replace("<", "&lt;")
				.Replace(">", "&gt;"))
				.Dump();
		}
		"/// </summary>".Dump();
		string netType;
		switch (type)
		{
			case "duration":
				netType = "TimeSpan?";
				break;
			case "int?":
			case "int":
			case "uint":
				netType = "int";
				break;
			case "value":
				if (info.EndsWith("[])"))
                {
					netType = "string[]";
				}
				else
				{
					netType = "string";
				}
				break;
			case "string":
				netType = "string";
				break;
			default:
				netType = "bool";
				break;
		}
		$"public {netType} {name} {{ get; set; }}".Dump();
	}
}