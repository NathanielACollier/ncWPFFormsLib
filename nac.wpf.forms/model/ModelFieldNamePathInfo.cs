using System;
using System.Collections.Generic;
using System.Linq;

namespace nac.wpf.forms.model
{
    public class ModelFieldNamePathInfo
    {
        private static nac.Logging.Logger log = new();

        public string Current { get; set; }
        public string ChildPath { get; set; }

        public ModelFieldNamePathInfo(string modelFieldName)
        {
            var fields = processModelFieldNameForPathInfo(modelFieldName);
            this.Current = fields[0];
            this.ChildPath = "";
            if (fields.Count > 1)
            {
                this.ChildPath = string.Join(".", fields.Skip(1));
                log.Info($"Processing [modelFieldName: {modelFieldName}] found field path.  [Current Path Val: {this.Current}] [Next Path: {this.ChildPath}]");
            }
        }

        private static List<string> processModelFieldNameForPathInfo(string modelFieldName)
        {
            if (string.IsNullOrWhiteSpace(modelFieldName))
            {
                throw new Exception("Model field name was null or whitespace");
            }

            var fields = modelFieldName.Split('.')
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Select(l => l.Trim())
                .ToList();

            return fields;
        }
    }
}
