using {{ PrefixName}}{{ SuffixName }}.API.Dtos;
using {{ PrefixName }}{{ SuffixName }}.API.Schema;

namespace {{ PrefixName }}{{ SuffixName }}.API;

public interface I{{ PrefixName }}{{ SuffixName }}
{
    Task<Create{{ PrefixName }}Response> Create{{ PrefixName }}(Create{{ PrefixName }}Input input);
    Task<Get{{ PrefixName }}sResponse> Get{{ PrefixName }}s(Get{{ PrefixName }}sRequest request);
    Task<Get{{ PrefixName }}Response> Get{{ PrefixName }}(string id);
    Task<Update{{ PrefixName }}Response> Update{{ PrefixName }}(Update{{ PrefixName }}Input input);
    Task<Delete{{ PrefixName }}Response> Delete{{ PrefixName }}(string id);
}
