using System.Text.RegularExpressions;

namespace ReshapeMetrics.ElasticSearch
{
    public class ElasticSearchUriTemplate
    {
        private static readonly Regex rxTemplate = new Regex(@"\{(.*?)\}", RegexOptions.Compiled);
        private readonly string template;

        public ElasticSearchUriTemplate(string template)
        {
            this.template = template;
        }

        public string Evaluate(EnvironmentLookup environment)
        {
            // ElasticSearch index names must be lowercase.
            return rxTemplate.Replace(template, m => environment.GetEnvironmentValue(m.Groups[1].Value)?.ToString().ToLower());
        }
    }
}
