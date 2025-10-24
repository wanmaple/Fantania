using System;

namespace Fantania;

public class DatabaseException : Exception
{
    public Type RelatedType { get; set; }

    public DatabaseException(Type type, string message)
    : base(message)
    {
        RelatedType = type;
    }
}