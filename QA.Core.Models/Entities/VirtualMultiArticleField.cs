﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.Core.Models.Entities
{
	public class VirtualMultiArticleField : ArticleField
	{
		public VirtualArticleField[] VirtualArticles { get; set; }
	}
}
