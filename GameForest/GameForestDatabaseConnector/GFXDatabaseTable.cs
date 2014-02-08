using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using GameForestDatabaseConnector.Logger;

namespace GameForestCore.Database
{
    public sealed class GFXDatabaseTable<T> where T : new()
    {
        private readonly string                     columnValue = "";

        private readonly GFXDatabaseTranslator<T>   translator; 

        public GFXDatabaseTable                     (GFXDatabaseTranslator<T> translator)
        {
            this.translator = translator;

            var columns = new List<string>(this.translator.TableColumns);

            for (var i = 0; i < columns.Count - 1; i++)
            {
                columnValue += columns[i] + ", ";
            }

            columnValue += columns[columns.Count - 1];
        }

        public void             Insert              (T data)
        {
            using (var msc = GFXDatabaseCore.GetConnection())
            {
                using (var commandIns = new MySqlCommand { Connection = msc })
                {
                    var builder = new StringBuilder();
                    var svalues = new List<string>(translator.ToStringValues(data));

                    for (int i = 0; i < svalues.Count - 1; i++)
                    {
                        builder.AppendFormat("{0}, ", svalues[i]);
                    }

                    builder.AppendFormat("{0}", svalues[svalues.Count - 1]);

                    commandIns.CommandText = string.Format("INSERT INTO {0} ({1}) VALUES ({2})", translator.TableName, columnValue, builder);
                    commandIns.ExecuteNonQuery();
                }
            }
        }

        public void             Remove              (string condition)
        {
            using (var msc = GFXDatabaseCore.GetConnection())
            {
                using (var commandRem = new MySqlCommand { Connection = msc })
                {
                    commandRem.CommandText = string.Format("DELETE FROM {0} WHERE {1}", translator.TableName, condition);
                    commandRem.ExecuteNonQuery();
                }
            }
        }

        public void             Update              (string condition, T data)
        {
            using (var msc = GFXDatabaseCore.GetConnection())
            {
                using (var commandUpt = new MySqlCommand { Connection = msc })
                {
                    var values = new List<string>(translator.ToStringValues(data));
                    var column = new List<string>(translator.TableColumns);

                    if (column.Count != values.Count)
                    {
                        throw new InvalidOperationException("Values should match column count.");
                    }

                    var combinedValues = new StringBuilder();

                    for (var i = 0; i < values.Count - 1; i++)
                    {
                        combinedValues.AppendFormat("{0} = {1}, ", column[i], values[i]);
                    }

                    combinedValues.AppendFormat(" {0} = {1}", column[values.Count - 1], values[values.Count - 1]);

                    commandUpt.CommandText = string.Format("UPDATE {0} SET {1} WHERE {2}", translator.TableName, combinedValues, condition);
                    commandUpt.ExecuteNonQuery();
                }
            }
        }

        public IEnumerable<T>   Select              (string condition, int maxRows = int.MaxValue)
        {
            using (var msc = GFXDatabaseCore.GetConnection())
            {
                using (var commandSel = new MySqlCommand { Connection = msc })
                {
                    commandSel.CommandText = string.IsNullOrEmpty(condition) ?
                    string.Format("SELECT * FROM {0} LIMIT {1}", translator.TableName, maxRows) :
                    string.Format("SELECT * FROM {0} WHERE {1} LIMIT {2}", translator.TableName, condition, maxRows);

                    var reader = commandSel.ExecuteReader();
                    var rtlist = new List<T>();

                    try
                    {
                        while (reader.Read())
                        {
                            var ta = translator.ToNativeData(reader);

                            rtlist.Add(ta);
                        }
                    }
                    catch (Exception exp)
                    {
                        GFXLogger.GetInstance().Log(GFXLoggerLevel.ERROR, "Database", exp.Message);

                        return null;
                    }
                    finally
                    {
                        reader.Close();
                        reader.Dispose();
                    }

                    return rtlist.ToArray();
                }
            }
        }

        public int              Count               (string condition = "")
        {
            using (var msc = GFXDatabaseCore.GetConnection())
            {
                using (var commandNum = new MySqlCommand { Connection = msc })
                {
                    if (string.IsNullOrEmpty(condition))
                    {
                        commandNum.CommandText = "SELECT COUNT(*) FROM " + translator.TableName;
                    }
                    else
                    {
                        commandNum.CommandText = "SELECT COUNT(*) FROM " + translator.TableName + " WHERE " + condition;
                    }

                    var result = commandNum.ExecuteScalar();

                    return int.Parse(result.ToString());
                }
            }
        }
    }
}
