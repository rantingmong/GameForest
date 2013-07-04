using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;

namespace GameForestCore.Database
{
    public sealed class GFXDatabaseTable<T> where T : new()
    {
        private readonly string                     columnValue = "";

        private readonly MySqlCommand               commandIns;
        private readonly MySqlCommand               commandRem;
        private readonly MySqlCommand               commandUpt;
        private readonly MySqlCommand               commandSel;

        private readonly MySqlCommand               commandNum;

        private readonly GFXDatabaseTranslator<T>   translator; 

        public GFXDatabaseTable                     (GFXDatabaseTranslator<T> translator)
        {
            this.translator = translator;

            commandIns = new MySqlCommand { Connection = GFXDatabaseCore.Instance };
            commandRem = new MySqlCommand { Connection = GFXDatabaseCore.Instance };
            commandUpt = new MySqlCommand { Connection = GFXDatabaseCore.Instance };
            commandSel = new MySqlCommand { Connection = GFXDatabaseCore.Instance };

            commandNum = new MySqlCommand { Connection = GFXDatabaseCore.Instance };

            var columns = new List<string>(this.translator.TableColumns);

            for (var i = 0; i < columns.Count - 1; i++)
            {
                columnValue += columns[i] + ", ";
            }

            columnValue += columns[columns.Count - 1];
        }

        ~GFXDatabaseTable                           ()
        {
            commandIns.Dispose();
            commandRem.Dispose();
            commandUpt.Dispose();
            commandSel.Dispose();
        }

        public void             Insert      (T data)
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

        public void             Remove      (string condition)
        {
            commandRem.CommandText = string.Format("DELETE FROM {0} WHERE {1}", translator.TableName, condition);
            commandRem.ExecuteNonQuery();
        }

        public void             Update      (string condition, T data)
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

        public IEnumerable<T>   Select      (string condition)
        {
            this.commandSel.CommandText = string.IsNullOrEmpty(condition) ? string.Format("SELECT * FROM {0}", this.translator.TableName) : string.Format("SELECT * FROM {0} WHERE {1}", this.translator.TableName, condition);

            var reader = commandSel.ExecuteReader();
            var rtlist = new List<T>();

            try
            {
                while (reader.Read())
                {
                    rtlist.Add(translator.ToNativeData(reader));
                }
            }
            catch (Exception exp)
            {
                return null;
            }
            finally
            {
                reader.Close();
                reader.Dispose();    
            }
            
            return rtlist.ToArray();
        }

        public int              Count       (string condition)
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
