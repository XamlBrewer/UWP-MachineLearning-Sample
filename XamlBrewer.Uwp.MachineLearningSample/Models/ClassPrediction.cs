namespace XamlBrewer.Uwp.MachineLearningSample.Models
{
    using Microsoft.ML.Runtime.Api;

    /// <summary>
    /// This is the class used for prediction after the model has been trained.
    /// It has a single boolean  <see cref="Class"/> and a PredictedLabel
    /// ColumnName attribute.
    /// 
    /// The Label is used to create and train the model, and it's also used with a
    /// second dataset to evaluate the model. The PredictedLabel is used during
    /// prediction and evaluation. For evaluation, an input with training data,
    /// the predicted values, and the model are used.
    /// </summary>
    public class ClassPrediction
    {
        string[] classNames = { "German", "English", "French", "Italian", "Romanian", "Spanish" };

        [ColumnName("PredictedLabel")]
        public float Class;

        public string PredictedLanguage => classNames[(int)Class];
    }
}
