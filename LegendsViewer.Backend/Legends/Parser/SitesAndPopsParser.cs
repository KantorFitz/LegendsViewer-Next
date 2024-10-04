﻿using System.ComponentModel;
using System.Text;
using LegendsViewer.Backend.Legends.Enums;
using LegendsViewer.Backend.Legends.Events;
using LegendsViewer.Backend.Legends.WorldObjects;

namespace LegendsViewer.Backend.Legends.Parser;

public class SitesAndPopulationsParser : IDisposable
{
    private readonly World _world;
    private readonly StreamReader _sitesAndPops;

    private string _currentLine;
    private Site _site;
    private Entity _owner;

    public SitesAndPopulationsParser(World world, string sitesAndPopsFile)
    {
        _world = world;
        _sitesAndPops = new StreamReader(sitesAndPopsFile, Encoding.GetEncoding("windows-1252"));
    }

    public void Parse()
    {
        ReadLine();
        ReadWorldPopulations();
        var mainRaces = _world.CivilizedPopulations.Select(cp => cp.Race);
        foreach (Entity civilization in _world.Entities.Where(entity => entity.EntityType == EntityType.Civilization && mainRaces.Contains(entity.Race)))
        {
            civilization.IsCiv = true;
        }
        while (_currentLine != "" && InSites())
        {
            ReadSite();
            ReadSiteOwner();
            ReadParentCiv();
            CheckSiteOwnership();
            ReadOfficials();
            ReadPopulations();
        }
        ReadOutdoorPopulations();
        ReadUndergroundPopulations();
        _sitesAndPops.Close();
    }

    private void ReadLine()
    {
        _currentLine = _sitesAndPops.ReadLine();
    }

    private bool InSites()
    {
        return !_sitesAndPops.EndOfStream && _currentLine != "Outdoor Animal Populations (Including Undead)";
    }

    private bool SiteStart()
    {
        return !_sitesAndPops.EndOfStream && !_currentLine.StartsWith("\t");
    }

    private void ReadWorldPopulations()
    {
        if (_currentLine == "Civilized World Population")
        {
            ReadLine();
            _currentLine = _sitesAndPops.ReadLine();
            while (!string.IsNullOrEmpty(_currentLine) && !_sitesAndPops.EndOfStream)
            {
                if (string.IsNullOrEmpty(_currentLine))
                {
                    _currentLine = _sitesAndPops.ReadLine();
                    continue;
                }
                CreatureInfo population = _world.GetCreatureInfo(_currentLine.Substring(_currentLine.IndexOf(" ", StringComparison.Ordinal) + 1));
                string countString = _currentLine.Substring(1, _currentLine.IndexOf(" ", StringComparison.Ordinal) - 1);
                var count = countString == "Unnumbered" ? int.MaxValue : Convert.ToInt32(countString);

                _world.CivilizedPopulations.Add(new Population(_world, population, count));
                _currentLine = _sitesAndPops.ReadLine();
            }
            _world.CivilizedPopulations.AddRange(_world.CivilizedPopulations.OrderByDescending(population => population.Count));
            while (_currentLine != "Sites")
            {
                _currentLine = _sitesAndPops.ReadLine();
            }
        }
        if (_currentLine == "Sites")
        {
            ReadLine();
            ReadLine();
        }
    }

    private void ReadSite()
    {
        string siteId = _currentLine.Substring(0, _currentLine.IndexOf(":", StringComparison.Ordinal));
        _site = _world.GetSite(Convert.ToInt32(siteId));
        if (_site != null)
        {
            string untranslatedName = Formatting.InitCaps(Formatting.ReplaceNonAscii(_currentLine.Substring(_currentLine.IndexOf(' ') + 1, _currentLine.IndexOf(',') - _currentLine.IndexOf(' ') - 1)));
            if (!string.IsNullOrWhiteSpace(_site.Name))
            {
                _site.UntranslatedName = untranslatedName;
            }
        }
        _owner = null;
        ReadLine();
    }

    private void ReadSiteOwner()
    {
        if (_currentLine.Contains("Owner:"))
        {
            string entityName = _currentLine.Substring(_currentLine.IndexOf(":", StringComparison.Ordinal) + 2,
                _currentLine.IndexOf(",", StringComparison.Ordinal) - _currentLine.IndexOf(":", StringComparison.Ordinal) - 2);
            var entities = _world.Entities
                .Where(entity => string.Equals(entity.Name, entityName, StringComparison.OrdinalIgnoreCase)).ToList();
            if (entities.Count == 1)
            {
                _owner = entities[0];
            }
            else if (entities.Count == 0)
            {
                _world.ParsingErrors.Report($"Couldn\'t Find Entity: {entityName},\nSite Owner of {_site.Name}");
            }
            else
            {
                var siteOwners = _site.OwnerHistory.ConvertAll(entry => entry.Owner);
                if (siteOwners.Count > 0)
                {
                    foreach (var entity in entities)
                    {
                        if (siteOwners.Contains(entity))
                        {
                            _owner = entity;
                        }
                    }
                }
#if DEBUG
                if (_owner == null)
                {
                    _world.ParsingErrors.Report($"Ambiguous ({entities.Count}) Site Ownership:\n{entityName}, Site Owner of {_site.Name}");
                }
#endif
            }
            if (_owner != null)
            {
                var raceIdentifier = _currentLine.Substring(_currentLine.IndexOf(",", StringComparison.Ordinal) + 2,
                    _currentLine.Length - _currentLine.IndexOf(",", StringComparison.Ordinal) - 2);
                _owner.Race = _world.GetCreatureInfo(raceIdentifier);
                if (!_owner.Sites.Contains(_site))
                {
                    _owner.Sites.Add(_site);
                }
                if (!_owner.CurrentSites.Contains(_site))
                {
                    _owner.CurrentSites.Add(_site);
                }
            }
            ReadLine();
        }
    }

    private void ReadParentCiv()
    {
        if (_currentLine.Contains("Parent Civ:"))
        {
            Entity parent = null;
            string civName = _currentLine.Substring(_currentLine.IndexOf(":", StringComparison.Ordinal) + 2,
                _currentLine.IndexOf(",", StringComparison.Ordinal) - _currentLine.IndexOf(":", StringComparison.Ordinal) - 2);
            CreatureInfo civRace = _world.GetCreatureInfo(_currentLine.Substring(_currentLine.IndexOf(",", StringComparison.Ordinal) + 2,
                _currentLine.Length - _currentLine.IndexOf(",", StringComparison.Ordinal) - 2));
            var entities = _world.Entities
                .Where(entity => string.Equals(entity.Name, civName, StringComparison.OrdinalIgnoreCase)).ToList();
            if (entities.Count == 1)
            {
                parent = entities[0];
            }
            else if (entities.Count == 0)
            {
                _world.ParsingErrors.Report($"Couldn\'t Find Entity:\n{civName}, Parent Civ of {_owner?.Name ?? "UNKNOWN"}");
            }
            else
            {
                var possibleEntities = entities.Where(entity => entity.Race == civRace).ToList();
                if (possibleEntities.Count == 1)
                {
                    parent = possibleEntities[0];
                }
                else if (possibleEntities.Count == 0)
                {
                    _world.ParsingErrors.Report($"Couldn\'t Find Entity by Name and Race:\n{civName}, Parent Civ of {_owner?.Name ?? "UNKNOWN"}");
                }
                else
                {
                    var possibleCivilizations = possibleEntities.Where(entity => entity.EntityType == EntityType.Civilization).ToList();
                    if (possibleCivilizations.Count == 1)
                    {
                        parent = possibleEntities[0];
                    }
                    else
                    {
#if DEBUG
                        _world.ParsingErrors.Report(
                            $"Ambiguous Parent Entity Name:\n{civName}, Parent Civ of {_owner?.Name ?? "UNKNOWN"}");
#endif
                    }
                }
            }

            if (parent != null)
            {
                parent.Race = civRace;
                if (_owner != null)
                {
                    var current = _owner;
                    while (!current.IsCiv && current.Parent != null)
                    {
                        current = current.Parent;
                    }
                    if (!current.IsCiv && current.Parent == null)
                    {
                        current.SetParent(parent);
                    }
                    if (!parent.Groups.Contains(_owner))
                    {
                        parent.Groups.Add(_owner);
                    }
                }
            }

            ReadLine();
        }
    }

    private void ReadOfficials()
    {
        if (_currentLine.Contains(":")) //Read in Officials, law-givers, mayors etc, ex: law-giver: Oled Sugarynestled, human
        {
            while (!_sitesAndPops.EndOfStream && _currentLine.Contains(":") && !SiteStart())
            {
                string officialName = Formatting.ReplaceNonAscii(_currentLine.Substring(_currentLine.IndexOf(":", StringComparison.Ordinal) + 2,
                    _currentLine.IndexOf(",", StringComparison.Ordinal) - _currentLine.IndexOf(":", StringComparison.Ordinal) - 2));
                var officials = _world.HistoricalFigures.Where(hf =>
                        string.Equals(hf.Name, officialName.Replace("'", "`"), StringComparison.OrdinalIgnoreCase)).ToList();
                if (officials.Count == 1)
                {
                    var siteOfficial = officials[0];
                    string siteOfficialPosition = _currentLine.Substring(1, _currentLine.IndexOf(":", StringComparison.Ordinal) - 1);
                    _site.Officials.Add(new Site.Official(siteOfficial, siteOfficialPosition));
                }
                else if (officials.Count == 0)
                {
                    _world.ParsingErrors.Report($"Couldn\'t Find Official:\n{officialName}, Official of {_site.Name}");
                }
#if DEBUG
                else
                {
                    _world.ParsingErrors.Report($"Ambiguous ({officials.Count}) Official Name:\n{officialName}, Official of {_site.Name}");
                }
#endif
                ReadLine();
            }
        }
    }

    public void ReadPopulations()
    {
        List<Population> populations = [];
        while (!SiteStart() && _currentLine != "")
        {
            CreatureInfo race = _world.GetCreatureInfo(_currentLine.Substring(_currentLine.IndexOf(' ') + 1));
            int count = Convert.ToInt32(_currentLine.Substring(1, _currentLine.IndexOf(' ') - 1));
            populations.Add(new Population(_world, race, count));
            ReadLine();
        }

        _site.Populations = populations.OrderByDescending(pop => pop.Count).ToList();
        _owner?.AddPopulations(populations);
        _world.SitePopulations.AddRange(populations);
    }

    public void CheckSiteOwnership()
    {
        //Check if a site was gained without a proper event from the xml
        if (_owner != null)
        {
            if (_site.OwnerHistory.Count == 0)
            {
                _site.OwnerHistory.Add(new OwnerPeriod(_site, _owner, -1, "founded"));
            }
            else if (_site.OwnerHistory.Last().Owner != _owner)
            {
                bool found = false;
                var founder = _site.OwnerHistory.Last().Founder;
                if (founder != null)
                {
                    if (founder.DeathYear != -1)
                    {
                        _site.OwnerHistory.Add(new OwnerPeriod(_site, _owner, founder.DeathYear, "after death of its founder (" + founder.DeathCause + ") took over"));
                        found = true;
                    }
                    else if (_site.Type == "Vault")
                    {
                        if (_owner is Entity entity)
                        {
                            _site.OwnerHistory.Add(new OwnerPeriod(_site, entity, -1, "moved into"));
                            found = true;
                        }
                    }
                }
                else if (_site.CurrentOwner == null)
                {
                    _site.OwnerHistory.Add(new OwnerPeriod(_site, _owner, _site.OwnerHistory.Last().EndYear, "slowly repopulated after the site was " + _site.OwnerHistory.Last().EndCause));
                    found = true;
                }
                if (!found)
                {
                    ChangeHfState lastSettledEvent = _site.Events.OfType<ChangeHfState>().LastOrDefault();
                    if (lastSettledEvent != null)
                    {
                        _site.OwnerHistory.Add(new OwnerPeriod(_site, _owner, lastSettledEvent.Year, "settled in"));
                    }
                    else
                    {
                        _world.ParsingErrors.Report("Site ownership conflict: " + _site.Name + ". Actually owned by " + _owner.ToLink(false) + " instead of " + _site.OwnerHistory.Last().Owner.ToLink(false));
                    }
                }
            }
        }
        //check for loss of period ownership, since some some loss of ownership eventsList are missing
        if (_owner == null && _site.OwnerHistory.Count > 0 && _site.OwnerHistory.Last().EndYear == -1)
        {
            _site.OwnerHistory.Last().EndYear = _world.Events.Last().Year - 1;
            _site.OwnerHistory.Last().EndCause = "abandoned";
        }
    }

    private void ReadOutdoorPopulations()
    {
        ReadLine();
        ReadLine();
        while (!string.IsNullOrEmpty(_currentLine) && !_sitesAndPops.EndOfStream)
        {
            if (string.IsNullOrEmpty(_currentLine))
            {
                _currentLine = _sitesAndPops.ReadLine();
                continue;
            }

            CreatureInfo population = _world.GetCreatureInfo(_currentLine.Substring(_currentLine.IndexOf(" ", StringComparison.Ordinal) + 1));
            var countString = _currentLine.Substring(1, _currentLine.IndexOf(" ", StringComparison.Ordinal) - 1);
            var count = countString == "Unnumbered" ? int.MaxValue : Convert.ToInt32(countString);

            _world.OutdoorPopulations.Add(new Population(_world, population, count));
            _currentLine = _sitesAndPops.ReadLine();
        }
        _world.OutdoorPopulations.AddRange(_world.OutdoorPopulations.OrderByDescending(population => population.Count));
    }

    private void ReadUndergroundPopulations()
    {
        ReadLine();
        ReadLine();
        while (!string.IsNullOrEmpty(_currentLine) && !_sitesAndPops.EndOfStream)
        {
            if (string.IsNullOrEmpty(_currentLine))
            {
                _currentLine = _sitesAndPops.ReadLine(); continue;
            }

            CreatureInfo population = _world.GetCreatureInfo(_currentLine.Substring(_currentLine.IndexOf(" ", StringComparison.Ordinal) + 1));
            var countString = _currentLine.Substring(1, _currentLine.IndexOf(" ", StringComparison.Ordinal) - 1);
            var count = countString == "Unnumbered" ? int.MaxValue : Convert.ToInt32(countString);

            _world.UndergroundPopulations.Add(new Population(_world, population, count));
            _currentLine = _sitesAndPops.ReadLine();
        }
        _world.UndergroundPopulations.AddRange(_world.UndergroundPopulations.OrderByDescending(population => population.Count));
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sitesAndPops.Dispose();
        }
    }
}
