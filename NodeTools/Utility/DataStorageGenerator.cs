using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.UI;
using NodeTools.Commands.CleanDimension;
using System.Runtime.Serialization;
using System.IO;
using System.Xml;

namespace NodeTools.Utility
{
    public static class DataStorageGenerator
    {
        public static void SetDataToStorage(Document doc, string GUID, string storageName, string fieldName, object dictionary)
        {
            try
            {
                Schema schema = Schema.Lookup(new Guid(GUID));
                if (schema != null)
                {
                    DataStorage storage = new FilteredElementCollector(doc).OfClass(typeof(DataStorage)).Cast<DataStorage>()
                    .Where(x => x.Name == storageName).FirstOrDefault();
                    if (storage != null)
                    {
                        Entity entity = storage.GetEntity(schema);
                        Serializer(dictionary, schema, storage, fieldName);
                    }
                    else
                    {
                        schema.Dispose();
                        BuildSchema(GUID, fieldName, doc, storageName, dictionary);
                    }
                }
                else
                {
                    BuildSchema(GUID, fieldName, doc, storageName, dictionary);
                }
            }
            catch (Exception)
            {
                throw;
            }
           
        }

        internal static void BuildSchema(string guid,string fieldName,Document doc,string storageName,object dict)
        {
            SchemaBuilder sb = new SchemaBuilder(new Guid(guid));
            sb.SetSchemaName("Node" + guid.Replace("-", ""));
            FieldBuilder fb = sb.AddSimpleField(fieldName, typeof(string));
            DataStorage Storage = DataStorage.Create(doc);
            Storage.Name = storageName;
            Serializer(dict, sb.Finish(), Storage, fieldName);
        }

        internal static void Serializer(object obj, Schema schema, DataStorage storage, string fieldName)
        {
            DataContractSerializer serializer = new DataContractSerializer(obj.GetType());
            using (MemoryStream ms = new MemoryStream())
            {
                serializer.WriteObject(ms, obj);
                ms.Seek(0, SeekOrigin.Begin);
                using (StreamReader reader = new StreamReader(ms))
                {
                    Entity entity = new Entity(schema);
                    entity.Set<string>(fieldName, reader.ReadToEnd());
                    storage.SetEntity(entity);
                }
            }
        }

        public static T GetDataFromStorage<T>(Document doc, string guid, string fieldName, string storageName)
        {
            Schema sch = Schema.Lookup(new Guid(guid));
            if(sch != null)
            {
                DataStorage storage = new FilteredElementCollector(doc).OfClass(typeof(DataStorage)).Cast<DataStorage>()
                    .Where(x => x.Name == storageName).FirstOrDefault();
                if(storage != null)
                {
                    string s = storage.GetEntity(sch).Get<string>(fieldName);

                    DataContractSerializer serializer = new DataContractSerializer(typeof(T));
                    using (XmlReader reader = XmlReader.Create(new StringReader(s)))
                    {
                        return (T)serializer.ReadObject(reader);
                    }
                }
                else
                {
                    sch.Dispose();
                }
               
            }
            return default;
            
        }
    }
}
