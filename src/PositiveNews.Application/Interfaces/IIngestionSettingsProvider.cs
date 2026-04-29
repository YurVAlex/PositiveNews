using PositiveNews.Application.DTOs;

namespace PositiveNews.Application.Interfaces;

public interface IIngestionSettingsProvider
{
    IngestionSettingsSnapshot GetCurrentSettings();
}
