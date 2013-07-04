using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace GameForestCore.Database
{
    public interface GFXDatabaseTranslator<T>
    {
        string              TableName       { get; }
        IEnumerable<string> TableColumns    { get; }

        IEnumerable<string> ToStringValues  (T data);
        T                   ToNativeData    (MySqlDataReader reader);
    }
}
