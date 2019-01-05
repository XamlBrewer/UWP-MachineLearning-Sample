using Microsoft.ML.Runtime.Api;

namespace XamlBrewer.Uwp.MachineLearningSample.Models
{
    public class BinaryClassificationData
    {
        [Column("0")] public float FixedAcidity;
        [Column("1")] public float VolatileAcidity;
        [Column("2")] public float CitricAcid;
        [Column("3")] public float ResidualSugar;
        [Column("4")] public float Chlorides;
        [Column("5")] public float FreeSulfurDioxide;
        [Column("6")] public float TotalSulfurDioxide;
        [Column("7")] public float Density;
        [Column("8")] public float Ph;
        [Column("9")] public float Sulphates;
        [Column("10")] public float Alcohol;
        [Column(ordinal: "11", name: "Label")] public float Label;
    }
}
