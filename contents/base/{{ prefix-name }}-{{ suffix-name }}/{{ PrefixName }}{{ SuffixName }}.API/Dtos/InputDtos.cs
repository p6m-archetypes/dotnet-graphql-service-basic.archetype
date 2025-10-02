namespace {{ PrefixName }}{{ SuffixName }}.API.Dtos;

public class Create{{ PrefixName }}Input
{
    public required string Name { get; set; }
}

public class Update{{ PrefixName }}Input
{
    public required string Id { get; set; }
    public required string Name { get; set; }
}
