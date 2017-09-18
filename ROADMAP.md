TODO/Ideas:
===========
[X] Initial implementation matches original in functionality
[ ] GetAllBlocks should support Range, or even enforce it
[ ] Encrypt Sqlite databases
[ ] Make sure this isn't a "brain wallet" implementation
[ ] Need a way to enforce coin settings are the same between peers (another hash?)
[ ] CLI client
[ ] Light client
[ ] Pluggable transaction types
[ ] Merge transaction input and output data types
[ ] Current implementation has layer/responsibility disease
[ ] Move pending transactions into the same data file as blocks (artificial performance impact crossing databases)
[ ] Remove exceptions thrown when they can be handled higher up
[ ] Audit access of _logger
[ ] Audit access of JsonConvert
[ ] Database performance improvements 
[ ] Convert everything to use asynchronous I/O
[ ] Remove code behaviour from Block, Transaction
[ ] Move chain code into its own library (ala NaiveChain)
[ ] Refactor repository methods for future partial replication implementation
[ ] 100% unit test coverage
[ ] Side chains
[ ] Offline wallet
[ ] Online wallet
[ ] Block Explorer
[ ] Coin Logos
[ ] Partial replication
[ ] Proof of Stake implementation
[ ] Pluggable peer protocols
[ ] Proper peer network (Lidgren, Neo, etc.)
[ ] Peer discovery over UPNP/UDP beacons
[ ] Efficient peer replication 
[ ] Consensus with Paxos and/or Raft
[ ] Pluggable storage
[ ] Pluggable consensus
[ ] Built-in stratum pool miner?
[ ] Export/Import private key with WIF