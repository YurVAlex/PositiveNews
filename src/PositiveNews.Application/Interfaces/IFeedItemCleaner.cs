using PositiveNews.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace PositiveNews.Application.Interfaces
{
    public interface IFeedItemCleaner
    {
        RssFeedItemDto Clean(RssFeedItemDto dto);
    }
}
