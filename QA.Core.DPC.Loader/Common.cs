using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.IO;
using QA.Core.Data.Collections;
using Quantumart.QPublishing.Database;

namespace QA.Core.DPC.Loader
{
    public class Common
    {
        public static string GetEmbeddedResourceText(string path)
        {
            using (var stream = Assembly.GetExecutingAssembly()
               .GetManifestResourceStream(path))
            {
                using (var textReader = new StreamReader(stream))
                {
                    return textReader.ReadToEnd();
                }
            }
        }

	    public static SqlParameter GetIdsTvp(IEnumerable<int> ids, string paramName)
	    {
		    var idsParam = new SqlParameter(paramName, SqlDbType.Structured) {SqlValue = ids.CreateSqlDataRecords().Ensure()};

			const string idsTypeName = "Ids";

		    idsParam.TypeName = idsTypeName;

		    return idsParam;
	    }


		public static string GetFileFromQpFieldPath(DBConnector dbConnector, int fieldId, string fieldValue)
		{
			if (string.IsNullOrEmpty(fieldValue))
				return null;

			return string.Format(@"{0}\{1}", dbConnector.GetDirectoryForFileAttribute(fieldId), fieldValue.Replace("/", "\\"));
	    }

        public static string GetFileNameByUrl(DBConnector dbConnector, int fieldId, string url)
        {
            if (string.IsNullOrEmpty(url))
                return null;

            var path = dbConnector.GetUrlForFileAttribute(fieldId, true, true);
            var fileName = url.Replace(path, string.Empty);

            if (fileName.StartsWith("/"))
                fileName = fileName.Remove(0, 1);

            return fileName;
        }
    }
}
