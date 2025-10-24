namespace Fantania;

public class BuiltinBatchBuilders
{
    public const int QUAD_BATCH = 0;
    public const int QUAD_ATLAS_BATCH = 1;

    private static BuiltinBatchBuilders _singleton = null;
    public static BuiltinBatchBuilders Singleton
    {
        get
        {
            if (_singleton == null)
                _singleton = new BuiltinBatchBuilders();
            return _singleton;
        }
    }
    
    public IBatchBuilder this[int index]
    {
        get
        {
            return _builders[index];
        }
    }

    private BuiltinBatchBuilders()
    {
        _builders[QUAD_BATCH] = new QuadBatchBuilder();
        _builders[QUAD_ATLAS_BATCH] = new QuadAtlasBatchBuilder();
    }

    IBatchBuilder[] _builders = new IBatchBuilder[8];
}