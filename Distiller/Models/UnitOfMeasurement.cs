using Newtonsoft.Json;

namespace Distiller.Models;

internal class UnitsOfMeasurement {
    private static readonly Lazy<UnitsOfMeasurement> lazy = new Lazy<UnitsOfMeasurement>(() => Load());

    public static UnitsOfMeasurement Instance { get { return lazy.Value; } }

    public Dictionary<string, List<UnitOfMeasurement>> ByKind { get; set; }
    public Dictionary<string, List<UnitOfMeasurement>> BySystem { get; set; }
    public Dictionary<string, Dictionary<string, List<UnitOfMeasurement>>> ByKindAndSystem { get; set; }
    public Dictionary<string, UnitOfMeasurement> ByName { get; set; }

    private UnitsOfMeasurement()
    {
        ByKind = new Dictionary<string, List<UnitOfMeasurement>>();
        BySystem = new Dictionary<string, List<UnitOfMeasurement>>();
        ByKindAndSystem = new Dictionary<string, Dictionary<string, List<UnitOfMeasurement>>>();
        ByName = new Dictionary<string, UnitOfMeasurement>();
    }

    private static UnitsOfMeasurement Load()
    {
        IEnumerable<UnitOfMeasurement> lst = UnitOfMeasurement.LoadAll();
        UnitsOfMeasurement units = new();

        foreach (UnitOfMeasurement unit in lst) {
            if (!units.ByKind.ContainsKey(unit.Kind))
                units.ByKind[unit.Kind] = new List<UnitOfMeasurement>();
            if (!units.BySystem.ContainsKey(unit.System))
                units.BySystem[unit.System] = new List<UnitOfMeasurement>();
            if (!units.ByKindAndSystem.ContainsKey(unit.Kind))
                units.ByKindAndSystem[unit.Kind] = new Dictionary<string, List<UnitOfMeasurement>>();
            if (!units.ByKindAndSystem[unit.Kind].ContainsKey(unit.System))
                units.ByKindAndSystem[unit.Kind][unit.System] = new List<UnitOfMeasurement>();
            units.ByKind[unit.Kind].Add(unit);
            units.BySystem[unit.System].Add(unit);
            units.ByKindAndSystem[unit.Kind][unit.System].Add(unit);
            units.ByName[unit.Name]=unit;
        }

        return units;
    }
}

internal class UnitOfMeasurement {
    internal class ConversionOp {
        public string Op { get; set; }
        public double Value { get; set; }

        public double Execute(double input)
        {
            if (Op == "Multiply")
                return input * Value;
            
            if (Op == "Add")
                return input + Value;

            throw new ArgumentException($"Invalid Op: {Op}");
        }
    }

    public string Kind { get; set; }
    public string System { get; set; }
    public string Name { get; set; }
    public string Abbreviation { get; set; }
    public bool IsBaseUnit { get; set; }
    public List<ConversionOp> ToBaseFormulla { get; set; }
    public List<ConversionOp> FromBaseFormulla { get; set; }

    public UnitOfMeasurement()
    {
    }

    public async static Task<IEnumerable<UnitOfMeasurement>> LoadAllAsync()
    {
        var serializer = new JsonSerializer();
        List<UnitOfMeasurement> units = new();

        using (var stream = await FileSystem.OpenAppPackageFileAsync("Data/UnitsOfMeasurement.json"))
        using (var streamReader = new StreamReader(stream))
        using (var textReader = new JsonTextReader(streamReader))
        {
            units = serializer.Deserialize<List<UnitOfMeasurement>>(textReader);
        }

        return units;
    }

    public static IEnumerable<UnitOfMeasurement> LoadAll()
    {
        Task<IEnumerable<UnitOfMeasurement>> task = Task.Run(LoadAllAsync);
        return task.Result;
    }

    public double ToBase(double value)
    {
        if (IsBaseUnit)
            return value;

        double resValue = value;
        foreach (var op in ToBaseFormulla)
            resValue = op.Execute(resValue);

        return resValue;
    }

    public double FromBase(double value)
    {
        if (IsBaseUnit)
            return value;

        double resValue = value;
        foreach (var op in FromBaseFormulla)
            resValue = op.Execute(resValue);

        return resValue;
    }

    public double Convert(double value, UnitOfMeasurement to)
    {
        if (Kind != to.Kind)
            throw new ArgumentException($"Units are not of the same kind ({Kind} != {to.Kind})");

        double baseValue = ToBase(value);
        return to.FromBase(baseValue);
    }
}