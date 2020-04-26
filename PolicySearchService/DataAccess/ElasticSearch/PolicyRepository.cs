﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nest;
using PolicySearchService.Domain;

namespace PolicySearchService.DataAccess.ElasticSearch
{
    public class PolicyRepository : IPolicyRepository
    {
        private readonly ElasticClient elasticClient;

        public PolicyRepository(ElasticClient elasticClient)
        {
            this.elasticClient = elasticClient;
        }

        public async Task Add(Domain.Policy policy)
        {
            await elasticClient.IndexDocumentAsync(policy);
        }

        public async Task<List<Domain.Policy>> Find(string queryText)
        {
            var result = await elasticClient
                .SearchAsync<Domain.Policy>(
                    s =>
                        s.From(0)
                        .Size(10)
                        .Query(q =>
                            q.MultiMatch(mm =>
                                mm.Query(queryText)
                                .Fields(f => f.Fields(p => p.PolicyNumber, p => p.PolicyHolder))
                                .Type(TextQueryType.BestFields)
                                .Fuzziness(Fuzziness.Auto)
                            )
                    ));

            return result.Documents.ToList();
        }
    }
}
