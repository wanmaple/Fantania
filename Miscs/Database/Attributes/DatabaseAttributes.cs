using System;

namespace Fantania;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public abstract class DatabaseAttribute : Attribute
{ }

public class DatabaseBooleanAttribute : DatabaseAttribute
{ }

public class DatabaseIntegerAttribute : DatabaseAttribute
{ }

public class DatabaseRealAttribute : DatabaseAttribute
{ }

public class DatabaseStringAttribute : DatabaseAttribute
{ }

public class DatabaseVector2Attribute : DatabaseAttribute
{ }

public class DatabaseVector3Attribute : DatabaseAttribute
{ }

public class DatabaseVector4Attribute : DatabaseAttribute
{ }

public class DatabaseCurveAttribute : DatabaseAttribute
{ }

public class DatabaseGradient1DAttribute : DatabaseAttribute
{ }

public class DatabaseGradient2DAttribute : DatabaseAttribute
{ }

public class DatabaseNoise2DAttribute : DatabaseAttribute
{ }

public class DatabaseCurvedEdgeAttribute : DatabaseAttribute
{ }

public class DatabaseGroupReferenceAttribute : DatabaseAttribute
{ }

public class DatabaseTypeReferenceAttribute : DatabaseAttribute
{ }