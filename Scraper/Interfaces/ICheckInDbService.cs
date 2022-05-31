using Shared.Entities;
using Shared.Models;

namespace Scraper.Interfaces;

public interface ICheckInDbService
{
    Task<List<NewAdMessage>> Check(List<AdLink> adLinks);
}