using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Fantania.Models;

namespace Fantania;

public class GroupedStylegrounds
{
    public IEnumerable<Stylegrounds> AllStylegrounds => _all.Values;

    public GroupedStylegrounds()
    {
        _all = new Dictionary<string, Stylegrounds>(8);
    }

    public Stylegrounds GetStylegrounds(string group, out bool isNew)
    {
        isNew = false;
        if (!_all.TryGetValue(group, out Stylegrounds ret))
        {
            ret = new Stylegrounds();
            _all.Add(group, ret);
            isNew = true;
        }
        return ret;
    }

    public bool CheckModified()
    {
        return _all.Values.Any(sg => sg.IsModified);
    }

    public async Task Load(Workspace workspace)
    {
        string binPath = workspace.GetAbsolutePath(Workspace.STYLEGROUNDS_NAME);
        await Task.Run(() =>
        {
            var serializer = new ObjectSerializer(1);
            using (var fs = new FileStream(binPath, FileMode.Open, FileAccess.Read))
            {
                using (var br = new BinaryReader(fs))
                {
                    int num = br.ReadInt32();
                    for (int i = 0; i < num; i++)
                    {
                        string group = br.ReadString();
                        var stylegrounds = new Stylegrounds();
                        stylegrounds.Deserialize(serializer, br, workspace);
                        _all.Add(group, stylegrounds);
                    }
                }
            }
        });
    }

    public async Task Save(Workspace workspace)
    {
        string binPath = workspace.GetAbsolutePath(Workspace.STYLEGROUNDS_NAME);
        var toRm = new List<string>();
        foreach (var pair in _all)
        {
            if (!workspace.WorldGrouping.HasGroup(pair.Key))
                toRm.Add(pair.Key);
        }
        foreach (var group in toRm)
        {
            _all.Remove(group);
        }
        await Task.Run(() =>
        {
            var serializer = new ObjectSerializer(1);
            using (var fs = new FileStream(binPath, FileMode.Create, FileAccess.Write))
            {
                using (var bw = new BinaryWriter(fs))
                {
                    bw.Write(_all.Count);
                    foreach (var pair in _all)
                    {
                        bw.Write(pair.Key);
                        pair.Value.Serialize(serializer, bw);
                        pair.Value.IsModified = false;
                    }
                }
            }
        });
    }

    Dictionary<string, Stylegrounds> _all;
}