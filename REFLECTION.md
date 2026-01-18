# Persistence

The persistent cache stores the aggregated KYC snapshot returned by the service, rather than vendor-specific DTOs. 
This reduces coupling to external schemas, simplifies persistence, and ensures the cache can directly satisfy future 
requests even after application restarts
