using System.Collections.Generic;

namespace BakalarkaWpf.Models;

public class OcrPage
{
    public int pageNum { get; set; }
    public List<OcrBox> OcrBoxes { get; set; }
}
