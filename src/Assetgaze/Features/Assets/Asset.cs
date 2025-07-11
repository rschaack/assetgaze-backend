// using Assetgaze.Domain;
// using LinqToDB.Mapping;
//
// namespace Assetgaze.Features.Assets;
//
// [Table("Assets")] 
// public class Asset
// {
//     [PrimaryKey]
//     public Guid Id { get; set; }
//     
//     [Column("Name")]
//     public string Name { get; set; }
//     
//     [Column("Category")]
//     public AssetCategory Category { get; set; }
//     
//     [Column("IncomeTreatment")]
//     public IncomeTreatment IncomeTreatment { get; set; }
//     
//     [Column("TotalExpenseRatio")]
//     public decimal TotalExpenseRatio { get; set; }
//     
//     [Column("ISIN"), NotNull] 
//     public string ISIN { get; set; }
//     
//     [Column("Sedol")] 
//     public string Sedol { get; set; }
//     
//     [Column("PriceSource"), NotNull] 
//     public PriceSource PriceSource { get; set; }
//     
//     [Column("FetchCode"), NotNull] 
//     public string FetchCode { get; set; }
//     
//     [Column("Type"), NotNull]
//     public AssetType Type { get; set; } 
//     
//     [Column("Denomination"), NotNull]
//     public string Denomination { get; set; }
// }