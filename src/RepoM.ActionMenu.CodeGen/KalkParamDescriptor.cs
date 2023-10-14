namespace Kalk.Core
{
    public class KalkParamDescriptor
    {
        public KalkParamDescriptor()
        {
        }

        public KalkParamDescriptor(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public string Name { get; set; }

        public string Description { get; set; }

        public bool IsOptional { get; set; }
    }

    public class ExamplesDescriptor
    {
        public string? Description { get; set; }
        public string? Input { get; set; }
        public string? Output { get; set; }
    }
}