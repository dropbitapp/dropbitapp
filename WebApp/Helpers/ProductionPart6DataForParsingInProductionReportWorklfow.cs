namespace WebApp.Helpers
{
    internal class ProductionPart6DataForParsing
    {
        public int ReportingPeriodProductionId { get; set; }
        public float ContentValue { get; set; }
        public int ContentFieldId { get; set; } // this value holds information about the original quantity type in a purchase raw material amount record. Eg: Weight/Volume
        public float OriginalRawMaterialAmount { get; set; } // used for calculation of correct material amount in Produciton Report Part 6
        public float OriginalFermentedAmount { get; set; } // used for calculation of correct material amount in Produciton Report Part 6
        public float FermentedAmountWentIntoCurrentProduction { get; set; } // used for calculation of correct material amount in Produciton Report Part 6
        public int PurchaseId { get; set; }
        public int ProductionIdAssociatedWithPurchase { get; set; }
        public bool NeedsMappingFromFermentedToPurchase { get; set; } // we need to set this flag to false when we are dealing with Pomace. Pomace is being distilled directly without going into fermenation first
    }
}