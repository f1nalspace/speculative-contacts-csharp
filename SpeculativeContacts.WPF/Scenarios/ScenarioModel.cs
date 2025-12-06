namespace SpeculativeContacts.Scenarios
{
    public class ScenarioModel : IEquatable<ScenarioModel>
    {
        public string Name { get; set; }
        public string Source { get; set; }

        public ScenarioModel() { }

        public ScenarioModel(string name, string source)
        {
            Name = name;
            Source = source;
        }

        public override string ToString() => Name;

        public bool Equals(ScenarioModel other) => other is not null && string.Equals(other.Name, Name);
        public override int GetHashCode() => Name.GetHashCode();
        public override bool Equals(object obj) => Equals(obj as ScenarioModel);
    }
}
