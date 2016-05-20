using QA.Core.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.Core.Models
{
    /// <summary>
    /// Постобработчик модели
    /// </summary>
    /// <typeparam name="TParameter"></typeparam>
    public interface IModelPostProcessor
    {
        Article ProcessModel(Article input);
    }
}
