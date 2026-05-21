namespace Cryptiklemur.RimLogging.UI.Filter;

internal sealed class PresetStore
{
    private readonly IPresetStorage _storage;

    public PresetStore(IPresetStorage storage)
    {
        _storage = storage;
    }

    public System.Collections.Generic.IEnumerable<(string Name, string Expr)> List()
    {
        int count = System.Math.Min(_storage.Names.Count, _storage.Expressions.Count);
        for (int i = 0; i < count; i++)
            yield return (_storage.Names[i], _storage.Expressions[i]);
    }

    public void Add(string name, string expr)
    {
        _storage.Names.Add(name);
        _storage.Expressions.Add(expr);
        _storage.Persist();
    }

    public bool Remove(string name)
    {
        int index = _storage.Names.IndexOf(name);
        if (index < 0)
            return false;

        _storage.Names.RemoveAt(index);
        _storage.Expressions.RemoveAt(index);
        _storage.Persist();
        return true;
    }
}
