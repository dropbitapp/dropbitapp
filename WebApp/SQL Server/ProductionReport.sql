declare @startDate as date
declare @endDate as date

--declare @gallons as real
--declare @alcohol as real
--declare @proof as real

set @startDate = '2017/02/01'
set @endDate = '2017/02/28'


/*
	Report of Production Operations: Part 1 - Transactions
*/
-- WHISKY TABEL DEFINITION REGION
drop table #WhiskyLessThen160 
create table #WhiskyLessThen160 
(
TaxPayment real, -- line 1
UseOfUS real, -- line 2
HospScientificOrEducUse real, -- line 3
Export real, -- line 4
Transfer2ForeignTradeZone real, -- line 5
Transfer2CMBW real, -- line 6
UseAsSuppliesOnVesselsOrAircraft real, -- line 7
UseInWineProduction real, -- line 8
EnteredInProcessingAccount real, -- line 9
Entered4TransferInBond real, -- line 10
EnteredInStorageAccount real, -- line 11
Withdrawn4RAndDOrtesting real, -- line 12
Other real, -- line 13
ProducedTotal real, -- line 14
Received4Redistillation real, -- line 15
PhysInvReceived4Redistillation real, -- line 16
PhysInvUnfinishedSpirits real -- line 17
)

drop table #WhiskyMoreThan160
create table #WhiskyMoreThan160
(
TaxPayment real, -- line 1
UseOfUS real, -- line 2
HospScientificOrEducUse real, -- line 3
Export real, -- line 4
Transfer2ForeignTradeZone real, -- line 5
Transfer2CMBW real, -- line 6
UseAsSuppliesOnVesselsOrAircraft real, -- line 7
UseInWineProduction real, -- line 8
EnteredInProcessingAccount real, -- line 9
Entered4TransferInBond real, -- line 10
EnteredInStorageAccount real, -- line 11
Withdrawn4RAndDOrtesting real, -- line 12
Other real, -- line 13
ProducedTotal real, -- line 14
Received4Redistillation real, -- line 15
PhysInvReceived4Redistillation real, -- line 16
PhysInvUnfinishedSpirits real -- line 17
)
-- END OF WHISKY REGION

-- BRANDY TABLE DEFINITION REGION
drop table #BrandyLessThen170 
create table #BrandyLessThen170 
(
TaxPayment real, -- line 1
UseOfUS real, -- line 2
HospScientificOrEducUse real, -- line 3
Export real, -- line 4
Transfer2ForeignTradeZone real, -- line 5
Transfer2CMBW real, -- line 6
UseAsSuppliesOnVesselsOrAircraft real, -- line 7
UseInWineProduction real, -- line 8
EnteredInProcessingAccount real, -- line 9
Entered4TransferInBond real, -- line 10
EnteredInStorageAccount real, -- line 11
Withdrawn4RAndDOrtesting real, -- line 12
Other real, -- line 13
ProducedTotal real, -- line 14
Received4Redistillation real, -- line 15
PhysInvReceived4Redistillation real, -- line 16
PhysInvUnfinishedSpirits real -- line 17
)

drop table #BrandyMoreThan170
create table #BrandyMoreThan170
(
TaxPayment real, -- line 1
UseOfUS real, -- line 2
HospScientificOrEducUse real, -- line 3
Export real, -- line 4
Transfer2ForeignTradeZone real, -- line 5
Transfer2CMBW real, -- line 6
UseAsSuppliesOnVesselsOrAircraft real, -- line 7
UseInWineProduction real, -- line 8
EnteredInProcessingAccount real, -- line 9
Entered4TransferInBond real, -- line 10
EnteredInStorageAccount real, -- line 11
Withdrawn4RAndDOrtesting real, -- line 12
Other real, -- line 13
ProducedTotal real, -- line 14
Received4Redistillation real, -- line 15
PhysInvReceived4Redistillation real, -- line 16
PhysInvUnfinishedSpirits real -- line 17
)
-- END OF BRANDY TABLE DEFINITION REGION

-- RUM TABLE DEFINITION REGION

-- END OF RUM TABLE DEFINITION REGION

-- GIN TABLE DEFINITION REGION

-- END OF GIN TABLE DEFINITION REGION

-- VODKA TABLE DEFINITION REGION

-- END OF VODKA TABLE DEFINITION REGION

-- ALCOHOLANDSPIRITS TABLE DEFINITION REGION

-- END OF ALCOHOLANDSPIRITS TABLE DEFINITION REGION

-- OTHER TABLE DEFINITION REGION

-- END OF OTHER TABLE DEFINITION REGION

-- TOTAL TABLE DEFINITION REGION

-- END OF TOTAL TABLE DEFINITION REGION


/*
	Report of Production Operations: Part II - Production of Alcohol and Spirits of 190 or more of Proof By Kind Of Material Used
*/

drop table #Part2ProofGallons
create table #Part2ProofGallons
(
Grain real, -- line 1
Fruit real, -- line 2
Molasses real, -- line 3
EthylSulfate real, -- line 4
Ethylenegas real, -- line 5
SulphiteLiquors real, -- line 6
FromRedistillation real, -- line 7
Other real -- line 8
)

/*
	Report of Production Operations: Part III - Production of Whisky By Kind and Cooperage Used
*/

-- NEW COOPERAGE PROOF GALLONS TABLE REGION
drop table #NewCooperage
create table #NewCooperage
(
	Bourbon real, -- line 1
	Corn real, -- line 2
	Rye real,-- line 3
	Light real, -- line 4
	OtherLine5 real, -- line 5
	OtherLine6 real, -- line 6
	OtherLine7 real, -- line 7
	OtherLine8 real -- line 8
)
-- END OF COOPERAGE PROOF GALLONS TABLE REGION

-- USED COOPERAGE PROOF GALLONS TABLE REGION
drop table #UsedCooperage
create table #UsedCooperage
(
	Bourbon real, -- line 1
	Corn real, -- line 2
	Rye real,-- line 3
	Light real, -- line 4
	OtherLine5 real, -- line 5
	OtherLine6 real, -- line 6
	OtherLine7 real, -- line 7
	OtherLine8 real -- line 8
)
-- END OF USED COOPERAGE PROOF GALLONS TABLE REGION

-- DEPOSITED IN TANKS PROOF GALLONS TABLE REGION
drop table #InTanks
create table #InTanks
(
	Bourbon real, -- line 1
	Corn real, -- line 2
	Rye real,-- line 3
	Light real, -- line 4
	OtherLine5 real, -- line 5
	OtherLine6 real, -- line 6
	OtherLine7 real, -- line 7
	OtherLine8 real -- line 8
)
-- END OF DEPOSITED IN TANKS PROOF GALLONS TABLE REGION

/*
	Report of Production Operations: Part IV - Production of Brandy By Kind
*/

drop table #Part4ProofGallons
create table #Part4ProofGallons
(
GrapeBrandy real, -- line 1
OtherBrandy real, -- line 2
NeutralGrapeBrandy real, -- line 3
AllOtherneutralbrandy real, -- line 4
OtherLine5 real, -- line 5
OtherLine6 real, -- line 6
OtherLine7 real, -- line 7
OtherLine8 real -- line 8
)

/*
	Report of Production Operations: Part V - Used in Redistillation
*/
drop table #Part5UsedInReDistilallation
create table #Part5UsedInReDistilallation
(
OtherValue real, -- line 1
OtherValue2 nvarchar(50) -- line 2
--OtherLine3 real, -- line 3
--OtherLine4 real, -- line 4
--OtherLine5 real, -- line 5
--OtherLine6 real, -- line 6
--OtherLine7 real, -- line 7
--OtherLine8 real -- line 8
)

/*
	Report of Production Operations: Part VI - Materias Used
*/

-- USED IN PRODUCTION OF DISTILLED SPIRITS POUNDS ABLE REGION
drop table #Part6UsedInProductionOfSpiritsPounds
create table #Part6UsedInProductionOfSpiritsPounds
(
Corn real,-- line 1
Rye real,-- line 2
Malt real, -- line 3
Wheat real, -- line 4
SorghumGrain real, -- line 5
Barley real, -- line 6
OtherLine7 real, -- line 7
OtherLie8 real, -- line 8
Grape real, -- line 9
OtherLine10 real, -- line 10
OtherLine11 real, -- line 11
OtherLine12 real, -- line 12
OtherLine13 real, -- line 13
OtherLine14 real, -- line 14
Molasses real, -- line 15
OtherLine16 real, -- line 16
OtherLine17 real, -- line 17
OtherLine18 real, -- line 18
EthylSulfate real, -- line 19
EthyleneGas real, -- line 20
SulphiteLiquors real, -- line 21
ButaneGas real, -- line 22
OtherLine23 real -- line 23
)

-- END OF USED IN PRODUCTION OF DISTILLED SPIRITS POUNDS ABLE REGION

-- USED IN PRODUCTION OF DISTILLED SPIRITS GALLONS ABLE REGION
drop table #Part6UsedInProductionOfSpiritsGallons
create table #Part6UsedInProductionOfSpiritsGallons
(
Corn real,-- line 1
Rye real,-- line 2
Malt real, -- line 3
Wheat real, -- line 4
SorghumGrain real, -- line 5
Barley real, -- line 6
OtherLine7 real, -- line 7
OtherLie8 real, -- line 8
Grape real, -- line 9
OtherLine10 real, -- line 10
OtherLine11 real, -- line 11
OtherLine12 real, -- line 12
OtherLine13 real, -- line 13
OtherLine14 real, -- line 14
Molasses real, -- line 15
OtherLine16 real, -- line 16
OtherLine17 real, -- line 17
OtherLine18 real, -- line 18
EthylSulfate real, -- line 19
EthyleneGas real, -- line 20
SulphiteLiquors real, -- line 21
ButaneGas real, -- line 22
OtherLine23 real -- line 23
)
-- END OF USED IN PRODUCTION OF DISTILLED SPIRITS GALLONS ABLE REGION

-- USED IN MANUFACTURE OF SUBSTANCES OTHER THAN DISTILLED SPIRITS POUNDS TABLE REGION
drop table #Part6UsedInManufactureOfSubstancesPounds
create table #Part6UsedInManufactureOfSubstancesPounds
(
Corn real,-- line 1
Rye real,-- line 2
Malt real, -- line 3
Wheat real, -- line 4
SorghumGrain real, -- line 5
Barley real, -- line 6
OtherLine7 real, -- line 7
OtherLie8 real, -- line 8
Grape real, -- line 9
OtherLine10 real, -- line 10
OtherLine11 real, -- line 11
OtherLine12 real, -- line 12
OtherLine13 real, -- line 13
OtherLine14 real, -- line 14
Molasses real, -- line 15
OtherLine16 real, -- line 16
OtherLine17 real, -- line 17
OtherLine18 real, -- line 18
EthylSulfate real, -- line 19
EthyleneGas real, -- line 20
SulphiteLiquors real, -- line 21
ButaneGas real, -- line 22
OtherLine23 real -- line 23
)
-- END OF USED IN MANUFACTURE OF SUBSTANCES OTHER THAN DISTILLED SPIRITS POUNDS TABLE REGION

-- USED IN MANUFACTURE OF SUBSTANCES OTHER THAN DISTILLED SPIRITS GALLONS TABLE REGION
drop table #Part6UsedInManufactureOfSubstancesGallons
create table #Part6UsedInManufactureOfSubstancesGallons
(
Corn real,-- line 1
Rye real,-- line 2
Malt real, -- line 3
Wheat real, -- line 4
SorghumGrain real, -- line 5
Barley real, -- line 6
OtherLine7 real, -- line 7
OtherLie8 real, -- line 8
Grape real, -- line 9
OtherLine10 real, -- line 10
OtherLine11 real, -- line 11
OtherLine12 real, -- line 12
OtherLine13 real, -- line 13
OtherLine14 real, -- line 14
Molasses real, -- line 15
OtherLine16 real, -- line 16
OtherLine17 real, -- line 17
OtherLine18 real, -- line 18
EthylSulfate real, -- line 19
EthyleneGas real, -- line 20
SulphiteLiquors real, -- line 21
ButaneGas real, -- line 22
OtherLine23 real -- line 23
)
-- END OF USED IN MANUFACTURE OF SUBSTANCES OTHER THAN DISTILLED SPIRITS GALLONS TABLE REGION


-- On Hands first of month
--insert into #StorageBrandyLessThen170 (OnHandFirstMonth)

-- Deposited In Bulk Storage
--insert into #StorageBrandyLessThen170 (DepositedInBulkStorage) 
--select
--sum(proofT.Value) [Proof]
--from dbo.Production_v3 as prodT 
--left join dbo.Proof_v3 as proofT on prodT.ProofID = proofT.ProofID 
--where prodT.ProductionEndTime <= @endDate and prodT.ProductionStartTime >= @startDate and prodT.ProductionTypeID in (2,3) and proofT.Value <> 0

---- transfered to processing account
--insert into #StorageBrandyLessThen170 (Transfer2ProcessingAccount) 
--select
--sum(proofT.Value) [Proof]
--from dbo.Production_v3 as prodT 
--left join dbo.Proof_v3 as proofT on prodT.ProofID = proofT.ProofID 
--where prodT.ProductionEndTime <= @endDate and prodT.ProductionStartTime >= @startDate and prodT.ProductionTypeID in (4) and proofT.Value <> 0


--select * from  #StorageBrandyLessThen170

