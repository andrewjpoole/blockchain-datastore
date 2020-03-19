# blockchain-datastore

So my idea (and I know I’m probably not the first to have had it) is that a private network Blockchain should be able to enable a distributed system to agree on the state of the truth without needing to elect a leader.

Each node in the distributed system would have its own copy of the Blockchain. Any new data added should be broadcasted to the other nodes, who will attempt to verify their Blockchain with the new block on the end. If they cant verify the chain, they will reject the new block, sending a negative response and the original node will have to try again.

So in a cluster with nodeA, nodeB and nodeC, if new data is added to nodeA and nodeB at the same time, one of them (lets say nodeB) will succeed in sending the new block to nodeC first, the other (nodeA) will (having received the new data from nodeB itself) recreate the new block and try again.

This gets more interesting as the number of nodes increases and my guess is that as long as I know the total number of nodes and therefore the size of a quorum (i.e. the number of nodes that represents more than half of the total) a given node should continue pushing a new block as long as it doesn’t receive more than the quorum amount of negative responses and it should definitely continue pushing a block if it receives more than the quorum amount of positive responses! The larger the cluster the more chance of competition, this is the bit that I am most looking forward to experimenting with.

I needed a concrete use case to test my theory and happened to have 50 odd Gb of family photos which need sorting so my plan was to build a WebApi which will allow me to store and retrieve files (mostly photos) and to define and search on metadata associated with the files I have stored. The files will be stored in an Azure blob store and the metadata will be stored in a Blockchain. The WebApi should run on multiple nodes as a distributed system, where all nodes are capable of reading and writing data. Whenever data is written to a node it should broadcast the new data to the other nodes and therefore all nodes should have an identical Blockchain.
How will I know when the question has been answered?

I guess I will have my system up and running, receiving uploaded photos as fast as possible for a sustained period of time, while being able to simultaneously make read successful requests and metadata searches. At any point in time, I should be able to check the latest block’s hash matches across the cluster.
Progress so far

## So far, I have:

* developed the basic dotNetCore WebApi service with Swagger, starting using LiteDb behind an abstraction
* developed my Blockchain implementation with unit tests
* developed a P2P module using the excellent GRPC
* developed a Blockchain database and switched it in place of LiteDB.
* written some NUnit integration tests that add 3000 random strings of data to a random node in a cluster of three, as fast as possible and check that the Blockchains are identical at the end
* persisted the chain to an Azure blob after each write
* added some ridiculously fast indexing using the humble Dictionary class.
* added logging to Elasticsearch
* have centralised configuration in Hashicorp Consul

## Next steps

### Next, I plan to:

* put the source onto GitHub
* write tests which prove that the cluster does not get into a split brain scenario after two coincident writes
* improve the Blockchain persistence
* have new nodes initialise their Blockchains from the blob store
* periodically check for and prune dead nodes from the list.
* develop an frontend probably Blazor or possible Angular
* add some kind of decent load testing

### Future ambitions

* I need an uploader application.
* I want to add indexing, including geospatial metadata, in ElasticSearch
* I want to deploy the thing to Azure Kubernetes Service.
* Long term I’m thinking of using Cognitive services Face Api and maybe some machine learning to fill in missing Tags etc.
