using Microsoft.ML.Runtime.Api;

namespace XamlBrewer.Uwp.MachineLearningSample.Models
{
    public class RegressionPercentage
    {
        [Column("9")]
        public float Ts;

        [Column("12")]
        public float Orb;

        [Column("13")]
        public float Drb;

        [Column("14")]
        public float Trb;

        [Column("15")]
        public float Ast;

        [Column("16")]
        public float Stl;

        [Column("17")]
        public float Blk;

        [Column("18")]
        public float Tov;

        [Column("19")]
        public float Usg;

        [Column("4")]
        public float Age;
    }
}
