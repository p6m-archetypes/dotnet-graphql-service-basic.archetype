namespace {{ PrefixName }}{{ SuffixName }}.API.Dtos;

public class Delete{{ PrefixName }}Response
{
    public required bool Success { get; set; }
    public string? Message { get; set; }
}
