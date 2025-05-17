using System.Web;
using LegendsViewer.Backend.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace LegendsViewer.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FileSystemController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<FilesAndSubdirectoriesDto>(StatusCodes.Status200OK)]
    public ActionResult<FilesAndSubdirectoriesDto> Get()
    {
        return Ok(GetRootInformation());
    }

    [HttpGet("{encodedPath}")]
    [ProducesResponseType<FilesAndSubdirectoriesDto>(StatusCodes.Status200OK)]
    public ActionResult<FilesAndSubdirectoriesDto> Get([FromRoute] string encodedPath)
    {
        var path = HttpUtility.UrlDecode(encodedPath);
        if (!Path.Exists(path) && !Directory.Exists(path))
        {
            return Ok(GetRootInformation());
        }
        string directoryName = Directory.GetCurrentDirectory();
        if (Directory.Exists(path))
        {
            directoryName = path;
        }
        else if (Path.Exists(path))
        {
            directoryName = Path.GetDirectoryName(path) ?? Directory.GetCurrentDirectory();
        }
        var response = new FilesAndSubdirectoriesDto
        {
            CurrentDirectory = directoryName,
            ParentDirectory = Directory.GetParent(directoryName)?.FullName,
            Subdirectories = Directory.GetDirectories(directoryName)
                .Select(subDirectoryPath => Path.GetRelativePath(directoryName, subDirectoryPath))
                .Where(IsNotUnixHiddenPath)
                .Order() // sort alphabetically
                .ToArray(),
            Files = Directory.GetFiles(directoryName, $"*{BookmarkController.FileIdentifierLegendsXml}")
                .Select(f => Path.GetFileName(f) ?? "")
                .Where(IsNotUnixHiddenPath)
                .Order()
                .ToArray()
        };
        return Ok(response);
    }

    [HttpGet("{encodedCurrentPath}/{encodedSubFolder}")]
    [ProducesResponseType<string>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<FilesAndSubdirectoriesDto> Get([FromRoute] string encodedCurrentPath, [FromRoute] string encodedSubFolder)
    {
        var currentPath = HttpUtility.UrlDecode(encodedCurrentPath);
        var subFolder = HttpUtility.UrlDecode(encodedSubFolder);
        var fullPath = Path.Combine(currentPath, subFolder);
        if (!Path.Exists(fullPath))
        {
            return BadRequest("File does not exist!");
        }
        return Get(fullPath);
    }

    private static FilesAndSubdirectoriesDto GetRootInformation()
    {
        if (OperatingSystem.IsWindows())
        {
            // Windows: return logical drives (C:\, D:\, etc.)
            var logicalDrives = Directory.GetLogicalDrives();
            return new FilesAndSubdirectoriesDto
            {
                CurrentDirectory = Path.DirectorySeparatorChar.ToString(),
                ParentDirectory = null,
                Subdirectories = logicalDrives,
                Files = Array.Empty<string>()
            };
        }
        else
        {
            // Unix-like systems (Linux, macOS): start from root directory
            var rootDir = new DirectoryInfo("/");
            return new FilesAndSubdirectoriesDto
            {
                CurrentDirectory = "/",
                ParentDirectory = null,
                Subdirectories = rootDir.GetDirectories()
                    .Select(d => d.FullName)
                    .Where(IsNotUnixHiddenPath)
                    .Order() // sort alphabetically
                    .ToArray(),
                Files = rootDir.GetFiles()
                    .Select(f => f.FullName)
                    .Where(IsNotUnixHiddenPath)
                    .Order()
                    .ToArray()
            };
        }
    }

    private static bool IsNotUnixHiddenPath(string path)
    {
        // Hide all '.' paths, except steam path
        return !(path.StartsWith('.') && path != ".steam");
    }
}
