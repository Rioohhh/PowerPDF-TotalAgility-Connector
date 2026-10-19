using System.Collections.Generic;

namespace PPDF.TotalAgility.Connector
{
    /// <summary>
    /// A document type (e.g. "Factuur", "Kantoorartikelen") mapped to a
    /// TotalAgility process and the initialization variable values to send with it.
    /// Profiles belong to one <see cref="TAConnection"/>, because process ids differ per environment.
    /// </summary>
    public class TAProfile
    {
        public string Name { get; set; }
        public string ProcessId { get; set; }
        public string ProcessName { get; set; }
        public List<TAInputVariable> Variables { get; set; } = new List<TAInputVariable>();

        public override string ToString() => Name;
    }
}
