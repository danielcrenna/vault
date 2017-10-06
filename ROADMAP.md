TODO/Ideas:
===========
Blockchain:
[ ] Hoist proof system in NaiveChain
[ ] Smart contracts
[ ] Side chains
[ ] Verification of block objects (sources, order, etc.)

UI:
[ ] Block Explorer
[ ] Coin Logos
[ ] CLI client
[ ] Light client

I/O:
[ ] Pluggable storage
[ ] Bootstrap chain (ala log shipping)
[ ] Database performance improvements
[ ] Encrypt Sqlite databases
[ ] GetAllBlocks should support Range, or even enforce it
[ ] Performance optimization of block objects using snapshots

Security:
[ ] Need to wipe all working arrays (ala Chaos.NaCl)
[ ] Block object encryption (different than sqlite encryption, as its block layer level and address specific)

Mining:
[ ] Proof of Stake implementation
[ ] Built-in stratum pool miner?

Wallets:
[ ] Passphrase wallet (non-human)
[ ] Vanity addresses (ala vanitygen)
[ ] Offline wallet
[ ] Online wallet
[ ] Brainwallets (improvements ala warpwallet)

Peers:
[ ] Full replication (default)
[ ] Partial replication
[ ] Pluggable peer protocols
[ ] Proper peer network (Lidgren, Neo, etc.)
[ ] Peer discovery over UPNP/UDP beacons
[ ] Efficient peer replication 
[ ] Consensus with Paxos and/or Raft
[ ] Pluggable consensus
[ ] Need a way to enforce coin settings are the same between peers (another hash / version checksum)

Misc:
[ ] 100% unit test coverage
[ ] Remove code behaviour from Block, Transaction
[ ] Audit access of _logger
[ ] Audit access of JsonConvert
[ ] Remove exceptions thrown when they can be handled higher up
[ ] Investigate performance of removing first-class transaction objects
[ ] Remove Index from KeyPair?
[ ] Switch to NSec when it releases