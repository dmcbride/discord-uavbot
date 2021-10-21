using System;
using System.Collections.Generic;
using System.Net.Http;
using uav.logic.Database;
using uav.logic.Database.Model;

namespace uav.logic.Service
{
    public class StoreVersions
    {
        private DatabaseService databaseService;
        private static HttpClient client;

        public async IAsyncEnumerable<string> UpdatedVersions(IEnumerable<LatestStoreVersion> versions)
        {
            client ??= new HttpClient();

            foreach (var version in versions)
            {
                var data = await client.GetStringAsync(version.Url);

                Console.WriteLine(data);
            }
            yield break;
        }
    }
}
