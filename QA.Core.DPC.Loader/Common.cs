﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.IO;
using System.Net.Http;
using NLog;
using NLog.Fluent;
using Quantumart.QPublishing.Database;

namespace QA.Core.DPC.Loader
{
    public class Common
    {
        
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        
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

		public static string GetFileFromQpFieldPath(DBConnector dbConnector, int fieldId, string fieldValue)
		{
			if (String.IsNullOrEmpty(fieldValue))
				return null;
            
			return String.Format(@"{0}\{1}", dbConnector.GetDirectoryForFileAttribute(fieldId), fieldValue.Replace("/", "\\"));
	    }

        public static string GetFileStorageUrl(DBConnector dbConnector, int fieldId, string url)
        {
            if (String.IsNullOrEmpty(url))
                return null;

            var field = dbConnector.GetContentAttributeObject(fieldId);
            var uploadUrlPrefix = dbConnector.GetUploadUrlPrefix(field.SiteId);
            var storageUrlPrefix = uploadUrlPrefix + "/_filesize";
            var storageUrl = url.Replace(uploadUrlPrefix, storageUrlPrefix);
            return storageUrl;
        }
        

        public static string GetFileNameByUrl(DBConnector dbConnector, int fieldId, string url)
        {
            if (String.IsNullOrEmpty(url))
                return null;

            var path = dbConnector.GetUrlForFileAttribute(fieldId, true, true);
            var fileName = url.Replace(path, String.Empty);

            if (fileName.StartsWith("/"))
                fileName = fileName.Remove(0, 1);

            return fileName;
        }

        public static long GetFileSize(IHttpClientFactory httpClientFactory, LoaderProperties loaderProperties, DBConnector cnn, int fieldId, string shortFieldUrl, string longFieldUrl)
        {
            if (loaderProperties.UseFileSizeService)
            {
                var url = GetFileStorageUrl(cnn, fieldId, longFieldUrl);
                var client = httpClientFactory.CreateClient();
                try
                {
                    var response = client.GetAsync(url).Result.Content.ReadAsStringAsync().Result;
                    return int.Parse(response);
                }
                catch (Exception ex)
                {
                    Logger.ForWarnEvent()
                        .Exception(ex)
                        .Message("Cannot receive file size with url: {url}", url)
                        .Log();
                    
                    return 0;
                }
            }

            var path= GetFileFromQpFieldPath(cnn, fieldId, shortFieldUrl);
            if (cnn.FileSystem.FileExists(path))
            {
                using var file = cnn.FileSystem.LoadStream(path);
                return file.Length;
            }
            
            Logger.ForWarnEvent()
                .Message("Cannot find file with path: {path}", path)
                .Log();
                    
            return 0;

        }
    }
}
