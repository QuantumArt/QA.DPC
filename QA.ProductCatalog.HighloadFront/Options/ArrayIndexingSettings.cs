namespace QA.ProductCatalog.HighloadFront.Options
{
    public class ArrayIndexingSettings
    {
        /// <summary>
        /// имя поля в которое будет положен объект
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// путь к массиву (JsonPath)
        /// </summary>
        public string Path { get; set; }


        /// <summary>
        /// пути внутри массива по которым генерить имя поля (JsonPath)
        /// </summary>
        public string[] Keys { get; set; }

        /// <summary>
        /// разделитель между значениями разных ключей
        /// </summary>
        public string CompositeKeySeparator { get; set; } = "_";

        /// <summary>
        /// разделитель между значениями ключа если селектор ключа вернул массив
        /// </summary>
        public string KeyPartsSeparator { get; set; } = "";
    }
}
