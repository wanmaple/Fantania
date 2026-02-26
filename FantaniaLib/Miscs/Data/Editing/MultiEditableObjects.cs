
namespace FantaniaLib;

public class MultiEditableObjects : IEditableObject
{
    class EditableFieldEqualityComparer : IEqualityComparer<IEditableField>
    {
        public static readonly EditableFieldEqualityComparer Instance = new EditableFieldEqualityComparer();

        public bool Equals(IEditableField? x, IEditableField? y)
        {
            if (x == null || y == null) return false;
            if (x.FieldName != y.FieldName) return false;
            if (!x.EditInfo.Equals(y.EditInfo)) return false;
            if (!x.FieldValue.Equals(y.FieldValue)) return false;
            if (x.FieldValidator != null && y.FieldValidator != null)
            {
                if (!x.FieldValidator.GetType().Equals(y.FieldValidator.GetType())) return false;
            }
            else if (x.FieldValidator != null || y.FieldValidator != null)
            {
                return false;
            }
            return true;
        }

        public int GetHashCode(IEditableField obj)
        {
            int hash = obj.FieldName.GetHashCode();
            hash = hash * 31 + obj.EditInfo.GetHashCode();
            hash = hash * 31 + obj.FieldValue.GetHashCode();
            if (obj.FieldValidator != null)
                hash = hash * 31 + obj.FieldValidator.GetType().GetHashCode();
            return hash;
        }
    }

    public MultiEditableObjects(IEnumerable<IEditableObject> objects)
    {
        _objects = objects;
    }

    public IReadOnlyList<IEditableField> GetEditableFields(IWorkspace workspace)
    {
        if (_editableFields == null)
        {
            _editableFields = new List<IEditableField>();
            var objs = _objects.ToArray();
            if (objs.Length > 0)
            {
                var first = objs[0];
                var fields = new HashSet<IEditableField>(EditableFieldEqualityComparer.Instance);
                foreach (var field in first.GetEditableFields(workspace))
                {
                    fields.Add(field);
                }
                for (int i = 1; i < objs.Length; i++)
                {
                    var objFields = objs[i].GetEditableFields(workspace);
                    fields.IntersectWith(objFields);
                }
                foreach (var field in fields)
                {
                    var sameFields = new List<IEditableField>();
                    foreach (var obj in objs)
                    {
                        var objField = obj.GetEditableFields(workspace).First(f => f.FieldName == field.FieldName);
                        sameFields.Add(objField);
                    }
                    _editableFields.Add(new MultiObjectsEditableField(workspace, sameFields));
                }
            }
        }
        return _editableFields;
    }

    IEnumerable<IEditableObject> _objects;
    List<IEditableField>? _editableFields;
}