using Microsoft.ML.Runtime.Api;

namespace XamlBrewer.Uwp.MachineLearningSample.Models
{
    public class CorrelationData
    {
        [Column("1")]
        public float Survived;

        [Column("2")]
        public float PClass;

        [Column("5")]
        public float Age;

        [Column("6")]
        public float SibSp;

        [Column("7")]
        public float Parch;

        [Column("9")]
        public float Fare;
    }
}
