namespace WebApp.Helpers
{
    internal class ProductionPart6DataForParsingInProductionReportWorklfow
    {
        public int ReportingPeriodProductionId { get; set; }
        public float ContentValue { get; set; }
        public int ContentFieldId { get; set; }
        public float OriginalRawMaterialAmount { get; set; } // used for calculation of correct material amount in Produciton Report Part 6
        public float OriginalFermentedAmount { get; set; } // used for calculation of correct material amount in Produciton Report Part 6
        public float FermentedAmountWentIntoCurrentProduction { get; set; } // used for calculation of correct material amount in Produciton Report Part 6
        public int PurchaseId { get; set; }
        public int ProductionIdAssociatedWithPurchase { get; set; }
    }
}