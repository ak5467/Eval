using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eval
{
    class GraphConfig
    {
        public static IGraphClient GraphClient { get; private set; }

        public static IGraphClient ConfigGraph()
        {
            var client = new GraphClient(new Uri("http://localhost:7474/db/data"), "neo4j", "annu123");
            client.Connect();

            GraphClient = client;
            return GraphClient;
        }
    }
}
