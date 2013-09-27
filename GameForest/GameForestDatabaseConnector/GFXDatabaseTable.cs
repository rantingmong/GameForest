using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;

namespace GameForestCore.Database
{
    public sealed class GFXDatabaseTable<T> where T : new()
    {
        private readonly string                     columnValue = "";

        private MySqlCommand                        commandIns;
        private MySqlCommand                        commandRem;
        private MySqlCommand                        commandUpt;
        private MySqlCommand                        commandSel;

        private MySqlCommand                        commandNum;

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
                commandIns = new MySqlCommand { Connection = msc };

                var builder = new StringBuilder();
                var svalues = new List<string>(translator.ToStringValues(data));

                for (int i = 0; i < svalues.Count - 1; i++)
                {
                    builder.AppendFormat("{0}, ", svalues[i]);
                }

                builder.AppendFormat("{0}", svalues[svalues.Count - 1]);

                commandIns.CommandText = string.Format("INSERT INTO {0} ({1}) VALUES ({2})", translator.TableName, columnValue, builder);
                commandIns.ExecuteNonQuery();

                commandIns.Dispose();

                msc.Close();
            }
        }

        public void             Remove              (string condition)
        {
            using (var msc = GFXDatabaseCore.GetConnection())
            {
                commandRem = new MySqlCommand { Connection = msc };

                commandRem.CommandText = string.Format("DELETE FROM {0} WHERE {1}", translator.TableName, condition);
                commandRem.ExecuteNonQuery();

                commandRem.Dispose();

                msc.Close();
            }
        }

        public void             Update              (string condition, T data)
        {
            using (var msc = GFXDatabaseCore.GetConnection())
            {
                commandUpt = new MySqlCommand { Connection = msc };

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

                commandUpt.Dispose();

                msc.Close();
            }
        }

        public IEnumerable<T>   Select              (string condition, int maxRows = int.MaxValue)
        {
            using (var msc = GFXDatabaseCore.GetConnection())
            {
                commandSel = new MySqlCommand { Connection = GFXDatabaseCore.GetConnection() };

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
                    // TODO: log the exception
                    Console.WriteLine(exp.Message);

                    return null;
                }
                finally
                {
                    reader.Close();
                    reader.Dispose();

                    commandSel.Dispose();
                }

                msc.Close();

                return rtlist.ToArray();
            }
        }

        public int              Count               (string condition = "")
        {
            using (var msc = GFXDatabaseCore.GetConnection())
            {
                try
                {
                    commandNum = new MySqlCommand { Connection = GFXDatabaseCore.GetConnection() };

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
                finally
                {
                    msc.Close();
                    commandNum.Dispose();
                }
            }
        }

        private string          ColumnsString       (IEnumerable<string> columns)
        {
            string          rs = "";
            List<string>    cc = new List<string>(columns);

            for (int i = 0; i < cc.Count - 1; i++)
            {
                rs += cc[i] + ", ";
            }

            rs += cc[cc.Count - 1];

            return rs;
        }
    }
}
