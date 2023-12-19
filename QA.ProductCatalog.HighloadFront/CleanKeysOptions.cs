using System;

namespace QA.ProductCatalog.HighloadFront;

public class CleanKeysOptions
{

    public CleanKeysOptions()
    {
        RunInterval = TimeSpan.FromMinutes(30);
        CleanInterval = TimeSpan.FromMinutes(15);
    }
    
    public TimeSpan RunInterval { get; set; }
    
    public TimeSpan CleanInterval { get; set; }
}