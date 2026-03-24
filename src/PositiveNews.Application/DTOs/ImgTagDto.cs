using System;
using System.Collections.Generic;
using System.Text;

namespace PositiveNews.Application.DTOs;

public class ImgTagDto
{
    public string? Url { get; set; }

    public string? Alt { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }

    public string? SrcSet { get; set; }

    public string? ClassName { get; set; }

    public string? Style { get; set; }
}
