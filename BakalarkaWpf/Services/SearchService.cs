using BakalarkaWpf.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BakalarkaWpf.Services;
public class SearchService
{

    public SearchService()
    {
    }


    public async Task<List<FileResults>> SearchAsync(string query)
    {
        var results = new List<FileResults>();

        return results;
    }

    public async Task<List<SearchResult>> NextResult(string query)
    {
        var results = new List<SearchResult>();

        return results;
    }
}