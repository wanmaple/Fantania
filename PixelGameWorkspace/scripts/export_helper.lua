local ExportHelper = {}

function ExportHelper.fieldType2CppType(fieldType)
    if fieldType == FieldTypes.Boolean then
        return "bool"
    elseif fieldType == FieldTypes.Integer then
        return "s32"
    elseif fieldType == FieldTypes.Float then
        return "real_t"
    elseif fieldType == FieldTypes.String then
        return "u32"
    elseif fieldType == FieldTypes.Vector2 then
        return "godot::Vector2"
    elseif fieldType == FieldTypes.Vector2Int then
        return "godot::Vector2i"
    elseif fieldType == FieldTypes.Vector3 then
        return "godot::Vector3"
    elseif fieldType == FieldTypes.Color then
        return "godot::Vector4"
    elseif fieldType == FieldTypes.Direction3D then
        return "godot::Vector3"
    elseif fieldType == FieldTypes.Texture then
        return "TextureDefinition"
    elseif fieldType == FieldTypes.Curve then
        return "HermitCurve"
    elseif fieldType == FieldTypes.GroupReference then
        return "GroupReference"
    elseif fieldType == FieldTypes.TypeReference then
        return "TypeReference"
    elseif fieldType == FieldTypes.Enum then
        return "s32"
    elseif fieldType == FieldTypes.BooleanArray then
        return "ReadonlyArray<bool>"
    elseif fieldType == FieldTypes.IntegerArray then
        return "ReadonlyArray<s32>"
    elseif fieldType == FieldTypes.FloatArray then
        return "ReadonlyArray<real_t>"
    elseif fieldType == FieldTypes.StringArray then
        return "ReadonlyArray<u32>"
    elseif fieldType == FieldTypes.Vector2Array then
        return "ReadonlyArray<godot::Vector2>"
    elseif fieldType == FieldTypes.Vector2IntArray then
        return "ReadonlyArray<godot::Vector2i>"
    elseif fieldType == FieldTypes.Vector3Array then
        return "ReadonlyArray<godot::Vector3>"
    elseif fieldType == FieldTypes.ColorArray then
        return "ReadonlyArray<godot::Vector4>"
    elseif fieldType == FieldTypes.Direction3DArray then
        return "ReadonlyArray<godot::Vector3>"
    elseif fieldType == FieldTypes.TextureArray then
        return "ReadonlyArray<TextureDefinition>"
    elseif fieldType == FieldTypes.CurveArray then
        return "ReadonlyArray<HermitCurve>"
    elseif fieldType == FieldTypes.GroupReferenceArray then
        return "ReadonlyArray<GroupReference>"
    elseif fieldType == FieldTypes.TypeReferenceArray then
        return "ReadonlyArray<TypeReference>"
    elseif fieldType == FieldTypes.EnumArray then
        return "ReadonlyArray<s32>"
    end
    return nil
end

return ExportHelper