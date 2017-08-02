declare @startDate nvarchar(50)
declare @endDate nvarchar (50)

set @startDate = '2016-11-01'
set @endDate = '2016-11-30'

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
select @TotalLine24
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
--select @GrapeBrandy
-- End of Production

