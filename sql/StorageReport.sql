declare @startDate as date
declare @endDate as date

--declare @gallons as real
--declare @alcohol as real
--declare @proof as real

set @startDate = '2016/11/01'
set @endDate = '2016/11/30'


/*
	Report of Storage Operations
*/
-- WHISKY TABEL DEFINITION REGION
drop table if exists #StorageWhiskyLessThen160 
create table #StorageWhiskyLessThen160 
(
OnHandFirstMonth real, -- line 1
DepositedInBulkStorage real,  -- line 2
ReceivedFromCustomsCustody real, -- line 3
Returned2BulkStorage real, -- line 4
OtherLine5 real, -- line 5
Total real, -- line 6
TaxPaid real, -- line 7
UseOfTheUS real, -- line 8
HospitalScientificEducationalUse real, --line 9
Export real, -- line 10
Transfer2ForeignTradeZone real, -- line 11
Transfer2CMBW real, -- line 12
UseAsSuppliesVesselsAircraft real, -- line 13
TransferToBondedWinery real, -- line 14
Transfer2SBW real, -- line 15
ResearchDevelopmentTesting real, -- line 16
Transfer2ProcessingAccount real, -- line 17
Transfer2ProducionAccount real, -- line 18
Transfer2OtherBondedPremises real, -- line 19
Destroyed real, -- line 20
OtherLine21 real, -- line 21
OtherLosses real, -- line 22
OnHandEndOfMonth real, -- line 23
TotalLine24 real, -- line 24
)

drop table if exists #StorageWhiskyMoreThan160
create table #StorageWhiskyMoreThan160
(
OnHandFirstMonth real, -- line 1
DepositedInBulkStorage real,  -- line 2
ReceivedFromCustomsCustody real, -- line 3
Returned2BulkStorage real, -- line 4
OtherLine5 real, -- line 5
Total real, -- line 6
TaxPaid real, -- line 7
UseOfTheUS real, -- line 8
HospitalScientificEducationalUse real, --line 9
Export real, -- line 10
Transfer2ForeignTradeZone real, -- line 11
Transfer2CMBW real, -- line 12
UseAsSuppliesVesselsAircraft real, -- line 13
TransferToBondedWinery real, -- line 14
Transfer2SBW real, -- line 15
ResearchDevelopmentTesting real, -- line 16
Transfer2ProcessingAccount real, -- line 17
Transfer2ProducionAccount real, -- line 18
Transfer2OtherBondedPremises real, -- line 19
Destroyed real, -- line 20
OtherLine21 real, -- line 21
OtherLosses real, -- line 22
OnHandEndOfMonth real, -- line 23
TotalLine24 real, -- line 24
)
-- END OF WHISKY REGION

-- BRANDY TABLE DEFINITION REGION
drop table if exists #StorageBrandyLessThen170 
create table #StorageBrandyLessThen170 
(
OnHandFirstMonth real, -- line 1
DepositedInBulkStorage real,  -- line 2
ReceivedFromCustomsCustody real, -- line 3
Returned2BulkStorage real, -- line 4
OtherLine5 real, -- line 5
Total real, -- line 6
TaxPaid real, -- line 7
UseOfTheUS real, -- line 8
HospitalScientificEducationalUse real, --line 9
Export real, -- line 10
Transfer2ForeignTradeZone real, -- line 11
Transfer2CMBW real, -- line 12
UseAsSuppliesVesselsAircraft real, -- line 13
TransferToBondedWinery real, -- line 14
Transfer2SBW real, -- line 15
ResearchDevelopmentTesting real, -- line 16
Transfer2ProcessingAccount real, -- line 17
Transfer2ProducionAccount real, -- line 18
Transfer2OtherBondedPremises real, -- line 19
Destroyed real, -- line 20
OtherLine21 real, -- line 21
OtherLosses real, -- line 22
OnHandEndOfMonth real, -- line 23
TotalLine24 real, -- line 24
)

drop table if exists #StorageBrandyMoreThan170
create table #StorageBrandyMoreThan170
(
OnHandFirstMonth real, -- line 1
DepositedInBulkStorage real,  -- line 2
ReceivedFromCustomsCustody real, -- line 3
Returned2BulkStorage real, -- line 4
OtherLine5 real, -- line 5
Total real, -- line 6
TaxPaid real, -- line 7
UseOfTheUS real, -- line 8
HospitalScientificEducationalUse real, --line 9
Export real, -- line 10
Transfer2ForeignTradeZone real, -- line 11
Transfer2CMBW real, -- line 12
UseAsSuppliesVesselsAircraft real, -- line 13
TransferToBondedWinery real, -- line 14
Transfer2SBW real, -- line 15
ResearchDevelopmentTesting real, -- line 16
Transfer2ProcessingAccount real, -- line 17
Transfer2ProducionAccount real, -- line 18
Transfer2OtherBondedPremises real, -- line 19
Destroyed real, -- line 20
OtherLine21 real, -- line 21
OtherLosses real, -- line 22
OnHandEndOfMonth real, -- line 23
TotalLine24 real, -- line 24
)
-- END OF BRANDY TABLE DEFINITION REGION

-- Storage
-- On Hands first of month
declare @OnHand1stMonth float -- in proof gallons. Line 23 from previous storage report
set @OnHand1stMonth = 2.4

declare @DepositedInBulkStorage float -- line 11 in Production Report
-- here lets get information from Production Report
declare @EnteredInStorageAccount float -- line 11

set @EnteredInStorageAccount = (select
sum(proofT.Value) [Proof]
from dbo.Production_v3 as prodT 
left join dbo.Proof_v3 as proofT on prodT.ProofID = proofT.ProofID 
where prodT.ProductionEndTime <= @endDate and prodT.ProductionEndTime >= @startDate and prodT.ProductionTypeID in (2) and proofT.Value <> 0)
-- end of section where we get information from Production report

set @DepositedInBulkStorage = @EnteredInStorageAccount
--select @DepositedInBulkStorage

-- Line 4 Returned to bulk storage
declare @ReturnedToBulkStorage float
set @ReturnedToBulkStorage = 0
--Line 6 of Storage Report
declare @TotalLine6 float
set @TotalLine6 = @OnHand1stMonth + @DepositedInBulkStorage + @ReturnedToBulkStorage

--select @TotalLine6
--insert into #StorageBrandyLessThen170 (OnHandFirstMonth)

-- Line 17 Transfered to Processing Account
declare @Transfered2ProcessingAccount float
set @Transfered2ProcessingAccount = 0

-- Line 18 - Transfered to Production Account. In Produciton report it is the amount that came from Storage into Production for redistillation
declare @Transfered2ProductionAccount float
set @Transfered2ProductionAccount = 0

-- Line 23 On Hand end of mine
declare @OnHandEndOfMonth float
set @OnHandEndOfMonth = @TotalLine6

-- Line 24 Total
declare @TotalLine24 float
set @TotalLine24 = @TotalLine6 - @Transfered2ProcessingAccount - @Transfered2ProductionAccount
-- End of Storage

-- Production
declare @EnteredInProcessingAccount float -- line 9

set @EnteredInProcessingAccount = (select
sum(proofT.Value) [Proof]
from dbo.Production_v3 as prodT 
left join dbo.Proof_v3 as proofT on prodT.ProofID = proofT.ProofID 
where prodT.ProductionEndTime <= @endDate and prodT.ProductionEndTime >= @startDate and prodT.ProductionTypeID in (3,4) and proofT.Value <> 0)-- we should later replace proofT.Value with production Status
--select @EnteredInProcessingAccount

declare @EnteredInStorageAccountProdRep float -- line 11

set @EnteredInStorageAccountProdRep = (select
sum(proofT.Value) [Proof]
from dbo.Production_v3 as prodT 
left join dbo.Proof_v3 as proofT on prodT.ProofID = proofT.ProofID 
where prodT.ProductionEndTime <= @endDate and prodT.ProductionEndTime >= @startDate and prodT.ProductionTypeID in (2) and proofT.Value <> 0) -- we should later replace proofT.Value with production Status

--select @EnteredInStorageAccountProdRep
declare @ProducedTotal float -- line 14 (total of all produced lines 1 through 13, in the report)

set @ProducedTotal = (select
sum(proofT.Value) [Proof]
from dbo.Production_v3 as prodT 
left join dbo.Proof_v3 as proofT on prodT.ProofID = proofT.ProofID 
where prodT.ProductionEndTime <= @endDate and prodT.ProductionEndTime >= @startDate and prodT.ProductionTypeID in (2) and proofT.Value <> 0) -- we should later replace proofT.Value with production Status
--select @ProducedTotal

--Part IV 
declare @GrapeBrandy float -- line 1
set @GrapeBrandy = (select
sum(proofT.Value) [Proof]
from dbo.Production_v3 as prodT 
left join dbo.Proof_v3 as proofT on prodT.ProofID = proofT.ProofID 
where prodT.ProductionEndTime <= @endDate and prodT.ProductionEndTime >= @startDate and prodT.ProductionTypeID in (2) and proofT.Value <> 0) -- we should later replace proofT.Value with production Status
select @GrapeBrandy
-- End of Production


-- Deposited In Bulk Storage
--insert into #StorageBrandyLessThen170 (DepositedInBulkStorage) 
--select
--sum(proofT.Value) [Proof]
--from dbo.Production_v3 as prodT 
--left join dbo.Proof_v3 as proofT on prodT.ProofID = proofT.ProofID 
--where prodT.ProductionEndTime <= @endDate and prodT.ProductionStartTime >= @startDate and prodT.ProductionTypeID in (2,3) and proofT.Value <> 0

-- transfered to processing account
--insert into #StorageBrandyLessThen170 (Transfer2ProcessingAccount) 
--select
--sum(proofT.Value) [Proof]
--from dbo.Production_v3 as prodT 
--left join dbo.Proof_v3 as proofT on prodT.ProofID = proofT.ProofID 
--where prodT.ProductionEndTime <= @endDate and prodT.ProductionStartTime >= @startDate and prodT.ProductionTypeID in (4) and proofT.Value <> 0


--select * from  #StorageBrandyLessThen170

