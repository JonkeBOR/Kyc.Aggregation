# Persistence

The persistent cache stores the aggregated KYC snapshot returned by the service, rather than vendor-specific DTOs. 
This reduces coupling to external schemas, simplifies persistence, and ensures the cache can directly satisfy future 
requests even after application restarts

Given more time and a larger amout of usecases/endpoints, I would consider moving the persistence logic into the mediatR behaviour pipeline instead, this would
further separate concerns and allow for easier testing and maintenance.
