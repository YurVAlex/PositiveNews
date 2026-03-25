using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace PositiveNews.Application.Interfaces;

public interface IImgTagExtractor
{
    public string? ExtractImgTag(XElement itemElement);
    
}
