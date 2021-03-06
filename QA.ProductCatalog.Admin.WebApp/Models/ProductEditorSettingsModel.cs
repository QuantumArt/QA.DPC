﻿using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace QA.ProductCatalog.Admin.WebApp.Models
{
    public class ProductEditorSettingsModel
    {
        /// <summary>
        /// Id корневой статьи
        /// </summary>
        public int? ArticleId { get; set; }

        /// <summary>
        /// Id описания продукта
        /// </summary>
        public int ProductDefinitionId { get; set; }

        /// <summary>
        /// ISO-код локали текущего пользователя
        /// </summary>
        public string UserLocale { get; set; }
        
        public HtmlString SerializeSettings()
        {
            string settings = JsonConvert.SerializeObject(this);
            return new HtmlString($"<script type=\"text/javascript\">window.ProductEditorSettings={settings}</script>");
        }
    }
}