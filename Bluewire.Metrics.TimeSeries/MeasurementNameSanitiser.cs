using System.Text.RegularExpressions;

namespace Bluewire.Metrics.TimeSeries
{
    public struct MeasurementNameSanitiser
    {
        private readonly char? replacementCharacter;

        public static MeasurementNameSanitiser None => default(MeasurementNameSanitiser);
        public static MeasurementNameSanitiser Hyphens => new MeasurementNameSanitiser('-');

        public MeasurementNameSanitiser(char replacementCharacter)
        {
            this.replacementCharacter = replacementCharacter;
        }

        private static readonly Regex rxQuestionableCharacters = new Regex(@"\W+");

#if NET46
        [System.Diagnostics.Contracts.Pure]
#endif
        public string Sanitise(string name)
        {
            if (replacementCharacter == null) return name;
            return rxQuestionableCharacters.Replace(name, new string(replacementCharacter.Value, 1)).Trim(replacementCharacter.Value);
        }
    }
}
