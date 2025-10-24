using System.Collections.Generic;

namespace Fantania;

public class ConstantOptionAttribute : EditAttribute
{
    public IReadOnlyDictionary<int, string> DisplayMap => _map;

    public ConstantOptionAttribute(params object[] options)
    {
        for (int i = 0; i < options.Length / 2; i++)
        {
            int num = (int)options[i * 2];
            string name = options[i * 2 + 1] as string;
            _map.Add(num, name);
        }
    }

    Dictionary<int, string> _map = new Dictionary<int, string>(0);
}